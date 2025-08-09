using Dapper;
using Microsoft.Extensions.Configuration;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Services.DataServices
{
    public class ContentDataService : BaseDataService, IContentDataService
    {
        private readonly IUrlProcessingService _urlProcessingService;

        public ContentDataService(IConfiguration config, IUrlProcessingService urlProcessingService) : base(config)
        {
            _urlProcessingService = urlProcessingService;
        }

        public Task<ArticleWithDetails?> GetArticleWithDetailsAsync(int articleId) => GetRecordAsync<ArticleWithDetails>("""SELECT * FROM "ArticlesWithDetails" WHERE "ArticleId" = @ArticleId""", new { ArticleId = articleId });
        public Task<int> CreateNewArticleAsync(int userId) => GetScalarAsync<int>("""INSERT INTO "Articles" ("OwnerUserId") VALUES (@UserId) RETURNING "ArticleId";""", new { UserId = userId });
        public Task DeleteArticleAsync(int articleId) => ExecuteAsync("""DELETE FROM "Articles" WHERE "ArticleId" = @ArticleId;""", new { ArticleId = articleId });
        public Task<IEnumerable<int>> GetArticleGenresAsync(int articleId) => GetRecordsAsync<int>("""SELECT "GenreId" FROM "ArticleGenres" WHERE "ArticleId" = @articleId""", new { articleId });

        public Task UpdateArticleAsync(int articleId, ArticleInputModel model)
        {
            return ExecuteAsync("""UPDATE "Articles" SET "ArticleTitle" = @ArticleTitle, "ArticleContent" = @ArticleContent, "ContentRatingId" = @ContentRatingId, "IsPrivate" = @IsPrivate, "CategoryId" = @CategoryId, "DisableLinkify" = @DisableLinkify, "UniverseId" = @UniverseId WHERE "ArticleId" = @ArticleId""",
                new { model.ArticleTitle, model.ArticleContent, model.ContentRatingId, model.IsPrivate, model.CategoryId, model.DisableLinkify, UniverseId = model.UniverseId == 0 ? (int?)null : model.UniverseId, ArticleId = articleId });
        }

        public async Task UpdateArticleGenresAsync(int articleId, List<int> genreIds)
        {
            await ExecuteAsync("""DELETE FROM "ArticleGenres" WHERE "ArticleId" = @articleId""", new { articleId });
            if (genreIds is not null && genreIds.Any())
            {
                var sql = """INSERT INTO "ArticleGenres" ("ArticleId", "GenreId") VALUES (@articleId, @genreId);""";
                foreach (var genreId in genreIds) { await ExecuteAsync(sql, new { articleId, genreId }); }
            }
        }

        public Task<StoryWithDetails?> GetStoryWithDetailsAsync(int storyId) => GetRecordAsync<StoryWithDetails>("""SELECT * FROM "StoriesWithDetails" WHERE "StoryId" = @StoryId""", new { StoryId = storyId });
        public Task<int> CreateNewStoryAsync(int userId) => GetScalarAsync<int>("""INSERT INTO "Stories" ("UserId") VALUES (@UserId) RETURNING "StoryId";""", new { UserId = userId });
        public Task DeleteStoryAsync(int storyId) => ExecuteAsync("""DELETE FROM "Stories" WHERE "StoryId" = @StoryId;""", new { StoryId = storyId });
        public Task<IEnumerable<int>> GetStoryGenresAsync(int storyId) => GetRecordsAsync<int>("""SELECT "GenreId" FROM "StoryGenres" WHERE "StoryId" = @storyId""", new { storyId });

        public Task UpdateStoryAsync(StoryInputModel model)
        {
            return ExecuteAsync("""UPDATE "Stories" SET "StoryTitle" = @StoryTitle, "StoryDescription" = @StoryDescription, "ContentRatingId" = @ContentRatingId, "IsPrivate" = @IsPrivate, "UniverseId" = @UniverseId, "LastUpdated" = NOW() WHERE "StoryId" = @StoryId""",
                new { model.StoryTitle, model.StoryDescription, model.ContentRatingId, model.IsPrivate, UniverseId = model.UniverseId == 0 ? (int?)null : model.UniverseId, model.StoryId });
        }

        public async Task UpdateStoryGenresAsync(int storyId, List<int> genreIds)
        {
            await ExecuteAsync("""DELETE FROM "StoryGenres" WHERE "StoryId" = @storyId""", new { storyId });
            if (genreIds is not null && genreIds.Any())
            {
                var sql = """INSERT INTO "StoryGenres" ("StoryId", "GenreId") VALUES (@storyId, @genreId);""";
                foreach (var genreId in genreIds) { await ExecuteAsync(sql, new { storyId, genreId }); }
            }
        }

        public Task<Proposal?> GetProposalAsync(int proposalId) => GetRecordAsync<Proposal>("""SELECT * FROM "Proposals" WHERE "ProposalId" = @proposalId""", new { proposalId });
        public Task<IEnumerable<ProposalStatus>> GetProposalStatusesAsync() => GetRecordsAsync<ProposalStatus>("""SELECT * FROM "ProposalStatuses" ORDER BY "StatusId" """);
        public Task<IEnumerable<int>> GetProposalGenresAsync(int proposalId) => GetRecordsAsync<int>("""SELECT "GenreId" FROM "ProposalGenres" WHERE "ProposalId" = @proposalId""", new { proposalId });

        public Task<int> CreateProposalAsync(ProposalInputModel model, int userId)
        {
            return GetScalarAsync<int>("""INSERT INTO "Proposals" ("UserId", "Title", "Description", "ContentRatingId", "StatusId", "IsPrivate", "DisableLinkify") VALUES (@userId, @Title, @Description, @ContentRatingId, @StatusId, @IsPrivate, @DisableLinkify) RETURNING "ProposalId";""",
                new { userId, model.Title, model.Description, model.ContentRatingId, model.StatusId, model.IsPrivate, model.DisableLinkify });
        }

        public Task UpdateProposalAsync(ProposalInputModel model, int userId)
        {
            return ExecuteAsync("""UPDATE "Proposals" SET "Title" = @Title, "Description" = @Description, "ContentRatingId" = @ContentRatingId, "StatusId" = @StatusId, "IsPrivate" = @IsPrivate, "DisableLinkify" = @DisableLinkify, "LastUpdated" = NOW() WHERE "ProposalId" = @ProposalId AND "UserId" = @userId""",
                new { model.Title, model.Description, model.ContentRatingId, model.StatusId, model.IsPrivate, model.DisableLinkify, model.ProposalId, userId });
        }

        public async Task UpdateProposalGenresAsync(int proposalId, List<int> genreIds)
        {
            await ExecuteAsync("""DELETE FROM "ProposalGenres" WHERE "ProposalId" = @proposalId""", new { proposalId });
            if (genreIds is not null && genreIds.Any())
            {
                var sql = """INSERT INTO "ProposalGenres" ("ProposalId", "GenreId") VALUES (@proposalId, @genreId);""";
                foreach (var genreId in genreIds) { await ExecuteAsync(sql, new { proposalId, genreId }); }
            }
        }

        public Task DeleteProposalAsync(int proposalId, int userId) => ExecuteAsync("""DELETE FROM "Proposals" WHERE "ProposalId" = @proposalId AND "UserId" = @userId""", new { proposalId, userId });
        public Task<IEnumerable<ArticleForListingViewModel>> GetUserArticlesAsync(int userId) => GetRecordsAsync<ArticleForListingViewModel>("""SELECT * FROM "ArticlesForListing" WHERE "OwnerUserId" = @userId ORDER BY "ArticleTitle" """, new { userId });

        public async Task<ILookup<int, string>> GetGenresForArticleListAsync(IEnumerable<int> articleIds)
        {
            if (!articleIds.Any()) return Enumerable.Empty<string>().ToLookup(x => 0);
            const string sql = """SELECT AG."ArticleId", G."GenreName" FROM "ArticleGenres" AG JOIN "Genres" G ON AG."GenreId" = G."GenreId" WHERE AG."ArticleId" = ANY(@articleIds)""";
            var results = await GetRecordsAsync<(int ArticleId, string GenreName)>(sql, new { articleIds = articleIds.ToList() });
            return results.ToLookup(r => r.ArticleId, r => r.GenreName);
        }

        public async Task<PagedResult<ArticleForListingViewModel>> GetPublicArticlesAsync(int pageIndex, int pageSize)
        {
            const string query = """FROM "ArticlesForListing" WHERE "IsPrivate" = FALSE AND "CategoryId" NOT IN (7, 8, 9, 10)""";
            var countSql = "SELECT COUNT(\"ArticleId\")" + query;
            var pagingSql = $"SELECT * {query} ORDER BY \"ArticleTitle\" LIMIT @Take OFFSET @Skip";

            var totalCount = await GetScalarAsync<int>(countSql);
            if (totalCount == 0) return new PagedResult<ArticleForListingViewModel> { PageIndex = pageIndex, PageSize = pageSize };

            var items = await GetRecordsAsync<ArticleForListingViewModel>(pagingSql, new { Skip = (pageIndex - 1) * pageSize, Take = pageSize });
            return new PagedResult<ArticleForListingViewModel> { Items = items, TotalCount = totalCount, PageIndex = pageIndex, PageSize = pageSize };
        }

        public Task<IEnumerable<DashboardStory>> GetPopularStoriesForDashboardAsync(int userId) => GetRecordsAsync<DashboardStory>("""SELECT Pop.*, CR."ContentRatingName" FROM "PopularStories" AS Pop INNER JOIN "Stories" AS S ON Pop."StoryId" = S."StoryId" INNER JOIN "ContentRatings" AS CR ON S."ContentRatingId" = CR."ContentRatingId" WHERE Pop."UserId" NOT IN (SELECT "UserBlocked" FROM "UserBlocking" WHERE "UserBlockedBy" = @userId) AND Pop."UserId" NOT IN (SELECT "UserBlockedBy" FROM "UserBlocking" WHERE "UserBlocked" = @userId) ORDER BY Pop."LastPostDateTime" DESC LIMIT 5""", new { userId });
        public Task<IEnumerable<DashboardArticle>> GetNewestArticlesForDashboardAsync(int userId) => GetRecordsAsync<DashboardArticle>("""SELECT * FROM "ArticlesForListing" WHERE ("CategoryId" <> 7 AND "CategoryId" <> 8 AND "CategoryId" <> 9 AND "CategoryId" <> 10) AND "OwnerUserId" NOT IN (SELECT "UserBlocked" FROM "UserBlocking" WHERE "UserBlockedBy" = @userId) AND "OwnerUserId" NOT IN (SELECT "UserBlockedBy" FROM "UserBlocking" WHERE "UserBlocked" = @userId) ORDER BY "CreatedDateTime" DESC LIMIT 5""", new { userId });
        public Task<IEnumerable<DashboardProposal>> GetNewestProposalsForDashboardAsync(int userId) => GetRecordsAsync<DashboardProposal>("""SELECT * FROM "ProposalsWithDetails" WHERE "StatusId" = 1 AND "UserId" NOT IN (SELECT "UserBlocked" FROM "UserBlocking" WHERE "UserBlockedBy" = @userId) AND "UserId" NOT IN (SELECT "UserBlockedBy" FROM "UserBlocking" WHERE "UserBlocked" = @userId) ORDER BY "LastUpdated" DESC LIMIT 5""", new { userId });
        public Task<IEnumerable<ProposalWithDetails>> GetUserProposalsAsync(int userId) => GetRecordsAsync<ProposalWithDetails>("""SELECT * FROM "ProposalsWithDetails" WHERE ("UserId" = @UserId) ORDER BY "Title" """, new { UserId = userId });
        public Task<StoryPost?> GetStoryPostForEditAsync(int storyPostId, int userId) => GetRecordAsync<StoryPost>("""SELECT SP.* FROM "StoryPosts" SP JOIN "Characters" C ON SP."CharacterId" = C."CharacterId" WHERE SP."StoryPostId" = @StoryPostId AND C."UserId" = @UserId""", new { StoryPostId = storyPostId, UserId = userId });
        public Task UpdateStoryPostAsync(int storyPostId, int characterId, string content) => ExecuteAsync("""UPDATE "StoryPosts" SET "CharacterId" = @CharacterId, "PostContent" = @PostContent WHERE "StoryPostId" = @StoryPostId""", new { StoryPostId = storyPostId, CharacterId = characterId, PostContent = content });
        public Task<IEnumerable<StoryWithDetails>> GetUserStoriesAsync(int userId) => GetRecordsAsync<StoryWithDetails>("""SELECT * FROM "StoriesWithDetails" WHERE "UserId" = @UserId ORDER BY "StoryTitle" """, new { UserId = userId });

        public async Task<ILookup<int, string>> GetGenresForStoryListAsync(IEnumerable<int> storyIds)
        {
            if (!storyIds.Any())
            {
                return Enumerable.Empty<(int, string)>().ToLookup(x => x.Item1, x => x.Item2);
            }
            const string sql = """SELECT SG."StoryId", G."GenreName" FROM "StoryGenres" SG JOIN "Genres" G ON SG."GenreId" = G."GenreId" WHERE SG."StoryId" = ANY(@storyIds)""";
            var results = await GetRecordsAsync<(int StoryId, string GenreName)>(sql, new { storyIds = storyIds.ToList() });
            return results.ToLookup(r => r.StoryId, r => r.GenreName);
        }

        public async Task<PagedResult<StoryPostViewModel>> GetStoryPostsPagedAsync(int storyId, int pageIndex, int pageSize)
        {
            var parameters = new { StoryId = storyId, Skip = (pageIndex - 1) * pageSize, Take = pageSize };
            var sql = """FROM "StoryPostsWithCharacterInfo" WHERE "StoryId" = @StoryId""";

            var countSql = $"SELECT COUNT(\"StoryPostId\") {sql}";
            var totalCount = await GetScalarAsync<int>(countSql, parameters);

            if (totalCount == 0)
            {
                return new PagedResult<StoryPostViewModel> { Items = Enumerable.Empty<StoryPostViewModel>(), TotalCount = 0, PageIndex = pageIndex, PageSize = pageSize };
            }

            var pagingSql = $"SELECT * {sql} ORDER BY \"DatePosted\" ASC LIMIT @Take OFFSET @Skip";
            var items = await GetRecordsAsync<StoryPostViewModel>(pagingSql, parameters);

            foreach (var post in items)
            {
                post.CardImageUrl = _urlProcessingService.GetCharacterImageUrl((ImageUploadPath)post.CardImageUrl);
            }
            
            return new PagedResult<StoryPostViewModel>
            {
                Items = items,
                TotalCount = totalCount,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
        }

        public Task AddStoryPostAsync(int storyId, int characterId, string content)
        {
            const string sql = """INSERT INTO "StoryPosts" ("StoryId", "CharacterId", "PostContent") VALUES (@StoryId, @CharacterId, @Content); UPDATE "Stories" SET "LastUpdated" = NOW() WHERE "StoryId" = @StoryId;""";
            return ExecuteAsync(sql, new { storyId, characterId, content });
        }

        public Task DeleteStoryPostAsync(int storyPostId, int currentUserId, int storyOwnerId)
        {
            const string sql = """
                DELETE FROM "StoryPosts" AS SP
                USING "Characters" AS C
                WHERE SP."CharacterId" = C."CharacterId"
                  AND SP."StoryPostId" = @StoryPostId
                  AND (C."UserId" = @CurrentUserId OR @CurrentUserId = @StoryOwnerId)
                """;

            return ExecuteAsync(sql, new { storyPostId, currentUserId, storyOwnerId });
        }

        public async Task<int> UpsertArticleAsync(ArticleInputModel input, int userId)
        {
            if (input.ArticleId == 0)
            {
                const string sql = """
                                 INSERT INTO "Articles" (
                                     "OwnerUserId", "ArticleTitle", "ArticleContent", "CategoryId", "UniverseId", 
                                     "ContentRatingId", "IsPrivate", "DisableLinkify"
                                 ) VALUES (
                                     @OwnerUserId, @ArticleTitle, @ArticleContent, @CategoryId, @UniverseId, 
                                     @ContentRatingId, @IsPrivate, @DisableLinkify
                                 )
                                 RETURNING "ArticleId";
                """;

                return await GetScalarAsync<int>(sql, new
                {
                    OwnerUserId = userId,
                    input.ArticleTitle,
                    input.ArticleContent,
                    input.CategoryId,
                    UniverseId = input.UniverseId == 0 ? (int?)null : input.UniverseId,
                    input.ContentRatingId,
                    input.IsPrivate,
                    input.DisableLinkify
                });
            }
            else
            {
                const string sql = """
                                 UPDATE "Articles" SET
                                     "ArticleTitle" = @ArticleTitle,
                                     "ArticleContent" = @ArticleContent,
                                     "CategoryId" = @CategoryId,
                                     "UniverseId" = @UniverseId,
                                     "ContentRatingId" = @ContentRatingId,
                                     "IsPrivate" = @IsPrivate,
                                     "DisableLinkify" = @DisableLinkify
                                 WHERE "ArticleId" = @ArticleId AND "OwnerUserId" = @OwnerUserId;
                """;

                await ExecuteAsync(sql, new
                {
                    input.ArticleId,
                    OwnerUserId = userId,
                    input.ArticleTitle,
                    input.ArticleContent,
                    input.CategoryId,
                    UniverseId = input.UniverseId == 0 ? (int?)null : input.UniverseId,
                    input.ContentRatingId,
                    input.IsPrivate,
                    input.DisableLinkify
                });

                return input.ArticleId;
            }
        }

        public async Task<int> UpsertProposalAsync(ProposalInputModel input, int userId)
        {
            if (input.ProposalId == 0)
            {
                return await CreateProposalAsync(input, userId);
            }
            else
            {
                await UpdateProposalAsync(input, userId);
                return input.ProposalId;
            }
        }

        public async Task<int> UpsertStoryAsync(StoryInputModel input, int userId)
        {
            if (input.StoryId == 0)
            {
                const string sql = """
                       INSERT INTO "Stories" ("UserId", "StoryTitle", "StoryDescription", "UniverseId", "ContentRatingId", "IsPrivate")
                       VALUES (@UserId, @StoryTitle, @StoryDescription, @UniverseId, @ContentRatingId, @IsPrivate)
                       RETURNING "StoryId";
                """;
                return await GetScalarAsync<int>(sql, new
                {
                    UserId = userId,
                    input.StoryTitle,
                    input.StoryDescription,
                    UniverseId = input.UniverseId == 0 ? (int?)null : input.UniverseId,
                    input.ContentRatingId,
                    input.IsPrivate
                });
            }
            else
            {
                const string sql = """
                       UPDATE "Stories" SET
                           "StoryTitle" = @StoryTitle,
                           "StoryDescription" = @StoryDescription,
                           "UniverseId" = @UniverseId,
                           "ContentRatingId" = @ContentRatingId,
                           "IsPrivate" = @IsPrivate,
                           "LastUpdated" = NOW() AT TIME ZONE 'UTC'
                       WHERE "StoryId" = @StoryId AND "UserId" = @UserId;
                """;
                await ExecuteAsync(sql, new
                {
                    input.StoryId,
                    UserId = userId,
                    input.StoryTitle,
                    input.StoryDescription,
                    UniverseId = input.UniverseId == 0 ? (int?)null : input.UniverseId,
                    input.ContentRatingId,
                    input.IsPrivate
                });
                return input.StoryId;
            }
        }
        public async Task<PagedResult<ProposalWithDetails>> SearchProposalsAsync(int pageIndex, int pageSize, List<int> genreIds)
        {
            var parameters = new DynamicParameters();
            var whereClauses = new List<string> { """P."StatusId" = 1""" };

            // This will need to be refactored to get the CurrentUserId from the UserService
            // int currentUserId = CurrentUserId; 
            // parameters.Add("CurrentUserId", currentUserId);
            // whereClauses.Add("""(P."UserId" NOT IN (SELECT "UserBlocked" FROM "UserBlocking" WHERE "UserBlockedBy" = @CurrentUserId) AND P."UserId" NOT IN (SELECT "UserBlockedBy" FROM "UserBlocking" WHERE "UserBlocked" = @CurrentUserId))""");

            if (genreIds is not null && genreIds.Any())
            {
                whereClauses.Add("""P."ProposalId" IN (SELECT "ProposalId" FROM "ProposalGenres" WHERE "GenreId" = ANY(@GenreIds))""");
                parameters.Add("GenreIds", genreIds);
            }

            var whereSql = string.Join(" AND ", whereClauses);
            var fromSql = $"FROM \"ProposalsWithDetails\" P WHERE {whereSql}";

            var countSql = $"SELECT COUNT(P.\"ProposalId\") {fromSql}";
            var totalCount = await GetScalarAsync<int>(countSql, parameters);

            if (totalCount == 0)
            {
                return new PagedResult<ProposalWithDetails> { Items = Enumerable.Empty<ProposalWithDetails>(), TotalCount = 0, PageIndex = pageIndex, PageSize = pageSize };
            }

            var pagingSql = $@"SELECT *
                                  {fromSql}
                                  ORDER BY P.""LastUpdated"" DESC
                                  LIMIT @Take OFFSET @Skip";
            parameters.Add("Skip", (pageIndex - 1) * pageSize);
            parameters.Add("Take", pageSize);

            var items = await GetRecordsAsync<ProposalWithDetails>(pagingSql, parameters);

            return new PagedResult<ProposalWithDetails> { Items = items, TotalCount = totalCount, PageIndex = pageIndex, PageSize = pageSize };
        }

        public async Task<PagedResult<StoryForListingViewModel>> SearchStoriesAsync(int pageIndex, int pageSize, string? searchTerm, List<int> genreIds, bool includeAdult, int? universeId, int currentUserId)
        {
            var parameters = new DynamicParameters();
            var whereClauses = new List<string> { """S."IsPrivate" = FALSE""" };

            parameters.Add("CurrentUserId", currentUserId);
            whereClauses.Add("""(S."UserId" NOT IN (SELECT "UserBlocked" FROM "UserBlocking" WHERE "UserBlockedBy" = @CurrentUserId) AND S."UserId" NOT IN (SELECT "UserBlockedBy" FROM "UserBlocking" WHERE "UserBlocked" = @CurrentUserId))""");

            if (!includeAdult)
            {
                whereClauses.Add("""S."ContentRatingId" <> 3""");
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                if (int.TryParse(searchTerm, out var storyId))
                {
                    whereClauses.Add("""S."StoryId" = @StoryId""");
                    parameters.Add("StoryId", storyId);
                }
                else
                {
                    whereClauses.Add("""S."StoryTitle" LIKE @SearchTerm""");
                    parameters.Add("SearchTerm", $"%{searchTerm}%");
                }
            }

            if (genreIds is not null && genreIds.Any())
            {
                whereClauses.Add("""S."StoryId" IN (SELECT "StoryId" FROM "StoryGenres" WHERE "GenreId" = ANY(@GenreIds))""");
                parameters.Add("GenreIds", genreIds);
            }

            if (universeId.HasValue && universeId > 0)
            {
                whereClauses.Add("""S."UniverseId" = @UniverseId""");
                parameters.Add("UniverseId", universeId.Value);
            }

            var whereSql = string.Join(" AND ", whereClauses);
            var sql = $"FROM \"StoriesWithDetails\" S WHERE {whereSql}";

            var countSql = $"SELECT COUNT(S.\"StoryId\") {sql}";
            var totalCount = await GetScalarAsync<int>(countSql, parameters);

            if (totalCount == 0)
            {
                return new PagedResult<StoryForListingViewModel> { Items = Enumerable.Empty<StoryForListingViewModel>(), TotalCount = 0, PageIndex = pageIndex, PageSize = pageSize };
            }

            var pagingSql = $"SELECT * {sql} ORDER BY S.\"LastUpdated\" DESC, S.\"StoryTitle\" LIMIT @Take OFFSET @Skip";
            parameters.Add("Skip", (pageIndex - 1) * pageSize);
            parameters.Add("Take", pageSize);

            var items = (await GetRecordsAsync<StoryForListingViewModel>(pagingSql, parameters)).ToList();

            return new PagedResult<StoryForListingViewModel> { Items = items, TotalCount = totalCount, PageIndex = pageIndex, PageSize = pageSize };
        }

        public async Task<PagedResult<ArticleViewModel>> SearchArticlesAsync(int pageIndex, int pageSize, string? searchTerm, List<int> genreIds)
        {
            var parameters = new DynamicParameters();
            var whereClauses = new List<string>();

            whereClauses.Add("""A."IsPrivate" = FALSE""");
            whereClauses.Add("""A."CategoryId" NOT IN (7, 8, 9, 10)""");

            // This will need to be refactored to get the CurrentUserId from the UserService
            // int currentUserId = CurrentUserId;
            // parameters.Add("CurrentUserId", currentUserId);
            // whereClauses.Add("""(A."OwnerUserId" NOT IN (SELECT "UserBlocked" FROM "UserBlocking" WHERE "UserBlockedBy" = @CurrentUserId) AND A."OwnerUserId" NOT IN (SELECT "UserBlockedBy" FROM "UserBlocking" WHERE "UserBlocked" = @CurrentUserId))""");

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                whereClauses.Add("""A."ArticleTitle" LIKE @SearchTerm""");
                parameters.Add("SearchTerm", $"%{searchTerm}%");
            }

            if (genreIds is not null && genreIds.Any())
            {
                whereClauses.Add("""A."ArticleId" IN (SELECT "ArticleId" FROM "ArticleGenres" WHERE "GenreId" = ANY(@GenreIds))""");
                parameters.Add("GenreIds", genreIds);
            }

            var whereSql = string.Join(" AND ", whereClauses);
            var fromSql = $"FROM \"ArticlesForListing\" A WHERE {whereSql}";

            var countSql = $"SELECT COUNT(A.\"ArticleId\") {fromSql}";
            var totalCount = await GetScalarAsync<int>(countSql, parameters);

            if (totalCount == 0)
            {
                return new PagedResult<ArticleViewModel> { Items = Enumerable.Empty<ArticleViewModel>(), TotalCount = 0, PageIndex = pageIndex, PageSize = pageSize };
            }

            var pagingSql = $"""
                                  SELECT A."ArticleId", A."ArticleTitle", A."CategoryName", A."ContentRating"
                                  {fromSql}
                                  ORDER BY A."ArticleTitle"
                                  LIMIT @Take OFFSET @Skip
                                  """;
            parameters.Add("Skip", (pageIndex - 1) * pageSize);
            parameters.Add("Take", pageSize);

            var items = (await GetRecordsAsync<ArticleViewModel>(pagingSql, parameters)).ToList();

            var articleIds = items.Select(i => i.ArticleId);
            if (articleIds.Any())
            {
                var genresLookup = await GetGenresForArticleListAsync(articleIds);
                foreach (var item in items)
                {
                    item.Genres = genresLookup[item.ArticleId].ToList();
                }
            }

            return new PagedResult<ArticleViewModel> { Items = items, TotalCount = totalCount, PageIndex = pageIndex, PageSize = pageSize };
        }

        public Task<IEnumerable<ArticleWithDetails>> GetRecentArticlesAsync()
        {
            const string sql = """SELECT * FROM "ArticlesForListing" WHERE ("CategoryId" <> 7 AND "CategoryId" <> 8 AND "CategoryId" <> 9 AND "CategoryId" <> 10) ORDER BY "CreatedDateTime" DESC LIMIT 5""";
            return GetRecordsAsync<ArticleWithDetails>(sql);
        }
    }
}
