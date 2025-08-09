using Dapper;
using Microsoft.Extensions.Configuration;
using RoleplayersGuild.Site.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Services.DataServices
{
    public class MiscDataService : BaseDataService, IMiscDataService
    {
        public MiscDataService(IConfiguration config) : base(config) { }

        public Task LogErrorAsync(string errorDetails) => ExecuteAsync("""INSERT INTO "ErrorLog" ("ErrorDetails") VALUES (@ErrorDetails)""", new { ErrorDetails = errorDetails });
        public Task<Ad?> GetRandomAdAsync(int adType) => GetRecordAsync<Ad>("""SELECT * FROM "Ads" WHERE "AdTypeId" = @AdType AND "IsActive" = TRUE ORDER BY RANDOM() LIMIT 1""", new { AdType = adType });
        public Task<IEnumerable<Category>> GetCategoriesAsync() => GetRecordsAsync<Category>("""SELECT * FROM "Categories" ORDER BY "CategoryName" """);
        public Task<IEnumerable<ContentRating>> GetContentRatingsAsync() => GetRecordsAsync<ContentRating>("""SELECT "ContentRatingId", "ContentRatingDescription", "ContentRatingName" FROM "ContentRatings" ORDER BY "ContentRatingId" """);
        public Task<IEnumerable<Genre>> GetGenresAsync() => GetRecordsAsync<Genre>("""SELECT "GenreId", "GenreName" FROM "Genres" ORDER BY "GenreName" """);
        public Task<IEnumerable<Gender>> GetGendersAsync() => GetRecordsAsync<Gender>("""SELECT "GenderId", "GenderName" FROM "CharacterGenders" ORDER BY "GenderId" """);
        public Task<IEnumerable<SexualOrientation>> GetSexualOrientationsAsync() => GetRecordsAsync<SexualOrientation>("""SELECT "SexualOrientationId", "OrientationName" FROM "CharacterSexualOrientations" ORDER BY "SexualOrientationId" """);
        public Task<IEnumerable<CharacterSource>> GetCharacterSourcesAsync() => GetRecordsAsync<CharacterSource>("""SELECT "SourceId", "SourceName" FROM "Sources" ORDER BY "SourceId" """);
        public Task<IEnumerable<PostLength>> GetPostLengthsAsync() => GetRecordsAsync<PostLength>("""SELECT "PostLengthId", "PostLengthName" FROM "CharacterPostLengths" ORDER BY "PostLengthId" """);
        public Task<IEnumerable<LiteracyLevel>> GetLiteracyLevelsAsync() => GetRecordsAsync<LiteracyLevel>("""SELECT "LiteracyLevelId", "LevelName" FROM "CharacterLiteracyLevels" ORDER BY "LiteracyLevelId" """);
        public Task<IEnumerable<LfrpStatus>> GetLfrpStatusesAsync() => GetRecordsAsync<LfrpStatus>("""SELECT "LfrpStatusId", "StatusName" FROM "CharacterLfrpStatuses" ORDER BY "LfrpStatusId" """);
        public Task<IEnumerable<EroticaPreference>> GetEroticaPreferencesAsync() => GetRecordsAsync<EroticaPreference>("""SELECT "EroticaPreferenceId", "PreferenceName" FROM "CharacterEroticaPreferences" ORDER BY "EroticaPreferenceId" """);
        
        public async Task<DashboardFunding> GetDashboardFundingAsync()
        {
            const decimal goalAmount = 150.00m;
            var currentAmount = await GetScalarAsync<decimal?>("""SELECT "CurrentFundingAmount" FROM "GeneralSettings" """) ?? 0m;
            var percentage = goalAmount > 0 ? (int)((currentAmount / goalAmount) * 100) : 0;
            return new DashboardFunding { CurrentFundingAmount = currentAmount, GoalAmount = goalAmount, ProgressPercentage = System.Math.Min(100, percentage) };
        }

        public Task<int> GetOpenAdminItemCountAsync(int userId) => GetScalarAsync<int>("""SELECT COUNT("ItemId") FROM "TodoItems" WHERE "AssignedToUserId" = @userId AND "StatusId" = 1""", new { userId });
        public Task<IEnumerable<ToDoItemViewModel>> GetDevelopmentItemsAsync() => GetRecordsAsync<ToDoItemViewModel>("""SELECT "ItemId", "ItemName", "ItemDescription" FROM "TodoItemsWithDetails" WHERE "TypeId" = 2 AND "StatusId" = 4 AND "AssignedToUserId" = 2 ORDER BY "ItemId" """);
        public Task<IEnumerable<ToDoItemViewModel>> GetConsiderationItemsAsync() => GetRecordsAsync<ToDoItemViewModel>("""SELECT "ItemId", "ItemName", "ItemDescription", "VoteCount" FROM "TodoItemsWithDetails" WHERE "TypeId" = 1 AND ("StatusId" = 1 OR "StatusId" = 4) ORDER BY "VoteCount" DESC, "ItemId" """);
        public Task<int> AddToDoItemAsync(string name, string description, int userId) => GetScalarAsync<int>("""INSERT INTO "TodoItems" ("ItemName", "ItemDescription", "StatusId", "TypeId", "CreatedByUserId") VALUES (@ItemName, @ItemDescription, 1, 1, @CreatedByUserId) RETURNING "ItemId" """, new { ItemName = name, ItemDescription = description, CreatedByUserId = userId });
        public Task AddVoteAsync(int itemId, int userId) => ExecuteAsync("""INSERT INTO "TodoItemVotes" ("TodoItemId", "UserId") VALUES (@ItemId, @UserId)""", new { ItemId = itemId, UserId = userId });
        public Task RemoveVoteAsync(int itemId, int userId) => ExecuteAsync("""DELETE FROM "TodoItemVotes" WHERE "TodoItemId" = @ItemId AND "UserId" = @UserId""", new { ItemId = itemId, UserId = userId });
        public async Task<bool> HasUserVotedAsync(int itemId, int userId) => await GetScalarAsync<int>("""SELECT COUNT(1) FROM "TodoItemVotes" WHERE "TodoItemId" = @ItemId AND "UserId" = @UserId""", new { ItemId = itemId, UserId = userId }) > 0;
        public Task<IEnumerable<QuickLink>> GetUserQuickLinksAsync(int userId) => GetRecordsAsync<QuickLink>("""SELECT * FROM "QuickLinks" WHERE ("UserId" = @UserId) ORDER BY "OrderNumber", "LinkName" """, new { UserId = userId });
        public Task AddQuickLinkAsync(QuickLink newLink) => ExecuteAsync("""INSERT INTO "QuickLinks" ("UserId", "LinkName", "LinkAddress", "OrderNumber") VALUES (@UserId, @LinkName, @LinkAddress, @OrderNumber)""", newLink);
        public Task DeleteQuickLinkAsync(int quickLinkId, int userId) => ExecuteAsync("""DELETE FROM "QuickLinks" WHERE "QuickLinkId" = @QuickLinkId AND "UserId" = @UserId""", new { QuickLinkId = quickLinkId, UserId = userId });
        
        public Task<IEnumerable<DashboardItemViewModel>> GetDashboardItemsAsync(string itemType, string filter, int userId)
        {
            string sql;
            var baseWhere = """
            "UserId" NOT IN (SELECT "UserBlocked" FROM "UserBlocking" WHERE "UserBlockedBy" = @userId) 
            AND "UserId" NOT IN (SELECT "UserBlockedBy" FROM "UserBlocking" WHERE "UserBlocked" = @userId)
            """;

            switch (itemType.ToLower())
            {
                case "articles":
                    var articleWhere = $"""({baseWhere.Replace("\"UserId\"", "\"OwnerUserId\"")}) AND (("IsPrivate" = FALSE AND "CategoryId" NOT IN (7, 8, 9, 10)) OR "OwnerUserId" = @userId)""";
                    const string articleOrder = "\"CreatedDateTime\" DESC";
                    sql = $"""
                    SELECT '/Community/Articles/View/' || "ArticleId" AS "Url", "ArticleTitle" AS "Title", 
                           '[' || "ContentRating" || ']' as "DetailLeft", 'by ' || "Username" as "DetailRight"
                    FROM "ArticlesForListing" WHERE {articleWhere} ORDER BY {articleOrder} LIMIT 5
                    """;
                    break;
                case "stories":
                    var storyWhere = $"({baseWhere}) AND (\"IsPrivate\" = FALSE OR \"UserId\" = @userId)";
                    var storyOrder = filter == "popular" ? "\"LastUpdated\" DESC" : "\"DateCreated\" DESC";
                    sql = $"""
                    SELECT '/Community/Stories/View/' || "StoryId" AS "Url", "StoryTitle" AS "Title",
                           '[' || "ContentRating" || ']' as "DetailLeft", 'by ' || "AuthorUsername" as "DetailRight"
                    FROM "StoriesWithDetails" WHERE {storyWhere} ORDER BY {storyOrder} LIMIT 5
                    """;
                    break;
                case "proposals":
                    var proposalWhere = $"({baseWhere}) AND (\"IsPrivate\" = FALSE OR \"UserId\" = @userId)";
                    var proposalOrder = filter == "active" ? "\"LastUpdated\" DESC" : "\"CreatedDateTime\" DESC";
                    sql = $"""
                    SELECT '/Community/Proposals/View/' || "ProposalId" AS "Url", "Title",
                           '[' || "ContentRating" || ']' as "DetailLeft", 'by ' || "Username" as "DetailRight"
                    FROM "ProposalsWithDetails" WHERE {proposalWhere} AND "StatusId" = 1 ORDER BY {proposalOrder} LIMIT 5
                    """;
                    break;
                default:
                    return Task.FromResult(Enumerable.Empty<DashboardItemViewModel>());
            }

            return GetRecordsAsync<DashboardItemViewModel>(sql, new { userId });
        }
    }
}
