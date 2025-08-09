using Dapper;
using Microsoft.Extensions.Configuration;
using RoleplayersGuild.Site.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Services.DataServices
{
    public class UniverseDataService : BaseDataService, IUniverseDataService
    {
        public UniverseDataService(IConfiguration config) : base(config) { }

        public Task<Universe?> GetUniverseAsync(int universeId) => GetRecordAsync<Universe>("""SELECT * FROM "Universes" WHERE "UniverseId" = @UniverseId""", new { UniverseId = universeId });
        public Task<UniverseWithDetails?> GetUniverseWithDetailsAsync(int universeId) => GetRecordAsync<UniverseWithDetails>("""SELECT * FROM "UniversesWithDetails" WHERE "UniverseId" = @UniverseId""", new { UniverseId = universeId });
        public Task<int> CreateNewUniverseAsync(int userId) => GetScalarAsync<int>("""INSERT INTO "Universes" ("UniverseOwnerId", "SubmittedById", "StatusId") VALUES (@UserId, @UserId, 1) RETURNING "UniverseId";""", new { UserId = userId });
        public Task DeleteUniverseAsync(int universeId) => ExecuteAsync("""DELETE FROM "Universes" WHERE "UniverseId" = @UniverseId;""", new { UniverseId = universeId });
        
        public async Task<IEnumerable<UniverseWithDetails>> GetUserUniversesAsync(int userId)
        {
            const string sql = """
                SELECT "UniverseId", "UniverseName", "UniverseDescription", "SourceType", "ContentRating", "CharacterCount"
                FROM "UniversesForListing" 
                WHERE "UniverseOwnerId" = @UserId 
                ORDER BY "UniverseName"
                """;
            var universes = (await GetRecordsAsync<UniverseWithDetails>(sql, new { UserId = userId })).ToList();

            if (!universes.Any())
            {
                return universes;
            }

            var universeIds = universes.Select(u => u.UniverseId).ToList();
            const string genreSql = """
                SELECT UG."UniverseId", G."GenreName" 
                FROM "UniverseGenres" UG 
                JOIN "Genres" G ON UG."GenreId" = G."GenreId" 
                WHERE UG."UniverseId" = ANY(@UniverseIds)
                """;
            var genreLookup = (await GetRecordsAsync<(int UniverseId, string GenreName)>(genreSql, new { UniverseIds = universeIds }))
                                .ToLookup(g => g.UniverseId, g => g.GenreName);

            foreach (var universe in universes)
            {
                universe.Genres = genreLookup[universe.UniverseId].ToList();
            }

            return universes;
        }

        public Task<Universe?> GetUniverseForEditAsync(int universeId, int userId) => GetRecordAsync<Universe>("""SELECT * FROM "Universes" WHERE "UniverseId" = @UniverseId AND "UniverseOwnerId" = @UserId""", new { UniverseId = universeId, UserId = userId });

        public Task UpdateUniverseAsync(UniverseInputModel model, int userId)
        {
            return ExecuteAsync("""UPDATE "Universes" SET "UniverseName" = @UniverseName, "UniverseDescription" = @UniverseDescription, "ContentRatingId" = @ContentRatingId, "SourceTypeId" = @SourceTypeId, "RequiresApprovalOnJoin" = @RequiresApprovalOnJoin, "DisableLinkify" = @DisableLinkify WHERE "UniverseId" = @UniverseId AND "UniverseOwnerId" = @userId""",
                new { model.UniverseName, model.UniverseDescription, model.ContentRatingId, model.SourceTypeId, model.RequiresApprovalOnJoin, model.DisableLinkify, model.UniverseId, userId });
        }

        public async Task UpdateUniverseGenresAsync(int universeId, List<int> genreIds)
        {
            await ExecuteAsync("""DELETE FROM "UniverseGenres" WHERE "UniverseId" = @universeId""", new { universeId });
            if (genreIds is not null && genreIds.Any())
            {
                var sql = """INSERT INTO "UniverseGenres" ("UniverseId", "GenreId") VALUES (@universeId, @genreId);""";
                foreach (var genreId in genreIds) { await ExecuteAsync(sql, new { universeId, genreId }); }
            }
        }

        public Task<IEnumerable<int>> GetUniverseGenresAsync(int universeId) => GetRecordsAsync<int>("""SELECT "GenreId" FROM "UniverseGenres" WHERE "UniverseId" = @universeId""", new { universeId });
        
        public async Task<PagedResult<UniverseWithDetails>> SearchUniversesAsync(int pageIndex, int pageSize, string? searchTerm, List<int> genreIds, int currentUserId)
        {
            var parameters = new DynamicParameters();
            var whereClauses = new List<string> { """U."StatusId" = 2""" };

            parameters.Add("CurrentUserId", currentUserId);
            whereClauses.Add("""(U."UniverseOwnerId" NOT IN (SELECT "UserBlocked" FROM "UserBlocking" WHERE "UserBlockedBy" = @CurrentUserId) AND U."UniverseOwnerId" NOT IN (SELECT "UserBlockedBy" FROM "UserBlocking" WHERE "UserBlocked" = @CurrentUserId))""");

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                if (int.TryParse(searchTerm, out var universeId))
                {
                    whereClauses.Add("""U."UniverseId" = @UniverseId""");
                    parameters.Add("UniverseId", universeId);
                }
                else
                {
                    whereClauses.Add("""U."UniverseName" LIKE @SearchTerm""");
                    parameters.Add("SearchTerm", $"%{searchTerm}%");
                }
            }

            if (genreIds is not null && genreIds.Any())
            {
                whereClauses.Add("""U."UniverseId" IN (SELECT "UniverseId" FROM "UniverseGenres" WHERE "GenreId" = ANY(@GenreIds))""");
                parameters.Add("GenreIds", genreIds);
            }

            var whereSql = string.Join(" AND ", whereClauses);
            var sql = $"FROM \"UniversesForListing\" U WHERE {whereSql}";

            var countSql = $"SELECT COUNT(U.\"UniverseId\") {sql}";
            var totalCount = await GetScalarAsync<int>(countSql, parameters);

            if (totalCount == 0)
            {
                return new PagedResult<UniverseWithDetails> { Items = Enumerable.Empty<UniverseWithDetails>(), TotalCount = 0, PageIndex = pageIndex, PageSize = pageSize };
            }

            var pagingSql = $"SELECT * {sql} ORDER BY U.\"UniverseName\" LIMIT @Take OFFSET @Skip";
            parameters.Add("Skip", (pageIndex - 1) * pageSize);
            parameters.Add("Take", pageSize);

            var items = (await GetRecordsAsync<UniverseWithDetails>(pagingSql, parameters)).ToList();

            var universeIds = items.Select(i => i.UniverseId);
            if (universeIds.Any())
            {
                const string genreSql = """SELECT UG."UniverseId", G."GenreName" FROM "UniverseGenres" UG JOIN "Genres" G ON UG."GenreId" = G."GenreId" WHERE UG."UniverseId" = ANY(@universeIds)""";
                var genresLookup = (await GetRecordsAsync<(int UniverseId, string GenreName)>(genreSql, new { universeIds = universeIds.ToList() })).ToLookup(r => r.UniverseId, r => r.GenreName);
                foreach (var item in items)
                {
                    item.Genres = genresLookup[item.UniverseId].ToList();
                }
            }

            return new PagedResult<UniverseWithDetails> { Items = items, TotalCount = totalCount, PageIndex = pageIndex, PageSize = pageSize };
        }

        public Task<IEnumerable<ArticleForListingViewModel>> GetUniverseArticlesByCategoryAsync(int universeId, int categoryId)
        {
            const string sql = """SELECT * FROM "ArticlesForListing" WHERE "CategoryId" = @CategoryId AND "UniverseId" = @UniverseId ORDER BY "ArticleTitle" """;
            return GetRecordsAsync<ArticleForListingViewModel>(sql, new { CategoryId = categoryId, UniverseId = universeId });
        }

        public Task<IEnumerable<Character>> GetUserCharactersNotInUniverseAsync(int userId, int universeId)
        {
            const string sql = """SELECT "CharacterId", "CharacterDisplayName" FROM "Characters" WHERE "UserId" = @UserId AND "CharacterId" NOT IN (SELECT "CharacterId" FROM "CharacterUniverses" WHERE "UniverseId" = @UniverseId) ORDER BY "CharacterDisplayName" """;
            return GetRecordsAsync<Character>(sql, new { UserId = userId, UniverseId = universeId });
        }

        public Task<IEnumerable<Character>> GetUserCharactersInUniverseAsync(int userId, int universeId)
        {
            const string sql = """SELECT "CharacterId", "CharacterDisplayName" FROM "Characters" WHERE "UserId" = @UserId AND "CharacterId" IN (SELECT "CharacterId" FROM "CharacterUniverses" WHERE "UniverseId" = @UniverseId) ORDER BY "CharacterDisplayName" """;
            return GetRecordsAsync<Character>(sql, new { UserId = userId, UniverseId = universeId });
        }

        public async Task AddCharactersToUniverseAsync(int universeId, List<int> characterIds)
        {
            if (characterIds is null || !characterIds.Any()) return;
            const string sql = """INSERT INTO "CharacterUniverses" ("CharacterId", "UniverseId") VALUES (@CharacterId, @UniverseId);""";
            foreach (var charId in characterIds)
            {
                await ExecuteAsync(sql, new { CharacterId = charId, UniverseId = universeId });
            }
        }

        public async Task RemoveCharactersFromUniverseAsync(int universeId, List<int> characterIds)
        {
            if (characterIds is null || !characterIds.Any()) return;
            const string sql = """DELETE FROM "CharacterUniverses" WHERE "UniverseId" = @UniverseId AND "CharacterId" = @CharacterId;""";
            foreach (var charId in characterIds)
            {
                await ExecuteAsync(sql, new { CharacterId = charId, UniverseId = universeId });
            }
        }
    }
}
