using System;
using Dapper;
using Microsoft.Extensions.Configuration;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Services.DataServices
{
    public class CommunityDataService : BaseDataService, ICommunityDataService
    {
        private readonly IUrlProcessingService _urlProcessingService;

        public CommunityDataService(IConfiguration config, IUrlProcessingService urlProcessingService) : base(config)
        {
            _urlProcessingService = urlProcessingService;
        }

        public Task<ChatRoom?> GetChatRoomAsync(int chatRoomId) => GetRecordAsync<ChatRoom>("""SELECT * FROM "ChatRooms" WHERE "ChatRoomId" = @ChatRoomId""", new { ChatRoomId = chatRoomId });
        public Task<ChatRoomWithDetails?> GetChatRoomWithDetailsAsync(int chatRoomId) => GetRecordAsync<ChatRoomWithDetails>("""SELECT * FROM "ChatRoomsWithDetails" WHERE "ChatRoomId" = @ChatRoomId""", new { ChatRoomId = chatRoomId });
        public Task<int> CreateNewChatRoomAsync(int userId) => GetScalarAsync<int>("""INSERT INTO "ChatRooms" ("SubmittedByUserId") VALUES (@UserId) RETURNING "ChatRoomId";""", new { UserId = userId });
        public Task DeleteChatRoomAsync(int chatRoomId) => ExecuteAsync("""DELETE FROM "ChatRooms" WHERE "ChatRoomId" = @ChatRoomId;""", new { ChatRoomId = chatRoomId });
        public Task<IEnumerable<ChatRoomPostsWithDetails>> GetChatRoomPostsAsync(int chatRoomId, int lastPostId) => GetRecordsAsync<ChatRoomPostsWithDetails>("""SELECT * FROM "ChatRoomPostsWithDetails" WHERE "ChatRoomId" = @chatRoomId AND "ChatPostId" > @lastPostId ORDER BY "ChatPostId" """, new { chatRoomId, lastPostId });
        public Task AddChatRoomPostAsync(int chatRoomId, int userId, int characterId, string postContent, string? characterThumbnail, string? characterNameClass, string? characterDisplayName) => ExecuteAsync("""INSERT INTO "ChatRoomPosts" ("ChatRoomId", "UserId", "CharacterId", "PostContent", "CharacterThumbnail", "CharacterNameClass", "CharacterDisplayName", "PostDateTime") VALUES (@chatRoomId, @userId, @characterId, @postContent, @characterThumbnail, @characterNameClass, @characterDisplayName, NOW())""", new { chatRoomId, userId, characterId, postContent, characterThumbnail, characterNameClass, characterDisplayName });

        public Task UpdateChatRoomAsync(ChatRoomInputModel model)
        {
            return ExecuteAsync("""UPDATE "ChatRooms" SET "ChatRoomName" = @ChatRoomName, "ChatRoomDescription" = @ChatRoomDescription, "ContentRatingId" = @ContentRatingId, "UniverseId" = @UniverseId WHERE "ChatRoomId" = @ChatRoomId""",
                new { model.ChatRoomName, model.ChatRoomDescription, model.ContentRatingId, UniverseId = model.UniverseId == 0 ? (int?)null : model.UniverseId, model.ChatRoomId });
        }

        public Task<int> CreateNewThreadAsync(string threadTitle, int creatorUserId)
        {
            const string sql = """
                INSERT INTO "Threads" ("ThreadTitle", "CreatedBy") 
                VALUES (@ThreadTitle, @CreatorUserId) 
                RETURNING "ThreadId";
                """;
            return GetScalarAsync<int>(sql, new { ThreadTitle = threadTitle, CreatorUserId = creatorUserId });
        }
        public Task InsertThreadUserAsync(int userId, int threadId, int readStatusId, int characterId, int permissionId) => ExecuteAsync("""INSERT INTO "ThreadUsers" ("UserId", "ThreadId", "ReadStatusId", "CharacterId", "PermissionId") VALUES (@UserId, @ThreadId, @ReadStatusId, @CharacterId, @PermissionId);""", new { UserId = userId, ThreadId = threadId, ReadStatusId = readStatusId, CharacterId = characterId, PermissionId = permissionId });
        public Task InsertMessageAsync(int threadId, int creatorId, string messageContent) => ExecuteAsync("""CALL "AddThreadMessage"(@ThreadId, @CreatorCharacterId, @MessageContent)""", new { ThreadId = threadId, CreatorCharacterId = creatorId, MessageContent = messageContent });
        public Task RemoveThreadCharacterAsync(int characterId, int threadId) => ExecuteAsync("""DELETE FROM "ThreadUsers" WHERE "ThreadId" = @ThreadId AND "CharacterId" = @CharacterId""", new { ThreadId = threadId, CharacterId = characterId });
        public Task RemoveThreadUserAsync(int userId, int threadId) => ExecuteAsync("""DELETE FROM "ThreadUsers" WHERE "ThreadId" = @ThreadId AND "UserId" = @UserId""", new { ThreadId = threadId, UserId = userId });
        public Task NukeThreadAsync(int threadId) => ExecuteAsync("""DELETE FROM "Threads" WHERE "ThreadId" = @ThreadId;""", new { ThreadId = threadId });
        public Task MarkUnreadForOthersOnThreadAsync(int threadId, int currentUserId) => ExecuteAsync("""UPDATE "ThreadUsers" SET "ReadStatusId" = 2 WHERE "ThreadId" = @ThreadId AND "UserId" <> @CurrentUserId""", new { ThreadId = threadId, CurrentUserId = currentUserId });
        public Task MarkReadForCurrentUserAsync(int threadId, int currentUserId) => ExecuteAsync("""UPDATE "ThreadUsers" SET "ReadStatusId" = 1 WHERE "ThreadId" = @ThreadId AND "UserId" = @CurrentUserId""", new { ThreadId = threadId, CurrentUserId = currentUserId });
        public Task ChangeReadStatusForCurrentUserAsync(int threadId, int readStatus, int currentUserId) => ExecuteAsync("""UPDATE "ThreadUsers" SET "ReadStatusId" = @ReadStatus WHERE "ThreadId" = @ThreadId AND "UserId" = @CurrentUserId""", new { ReadStatus = readStatus, ThreadId = threadId, CurrentUserId = currentUserId });
        public Task<ThreadDetails?> GetThreadDetailsForUserAsync(int threadId, int currentUserId) => GetRecordAsync<ThreadDetails>("""SELECT * FROM "ThreadsWithDetails" WHERE "UserId" = @CurrentUserId AND "ThreadId" = @ThreadId""", new { CurrentUserId = currentUserId, ThreadId = threadId });
        public Task<IEnumerable<DashboardChatRoom>> GetActiveChatRoomsForDashboardAsync(int userId) => GetRecordsAsync<DashboardChatRoom>("""SELECT * FROM "ActiveChatrooms" WHERE "ChatRoomId" NOT IN (SELECT "ChatRoomId" FROM "ChatRoomLocks" WHERE "UserId" = @userId) AND "SubmittedByUserId" NOT IN (SELECT "UserBlocked" FROM "UserBlocking" WHERE "UserBlockedBy" = @userId) AND "SubmittedByUserId" NOT IN (SELECT "UserBlockedBy" FROM "UserBlocking" WHERE "UserBlocked" = @userId) ORDER BY "LastPostTime" DESC LIMIT 5""", new { userId });
        
        public Task<IEnumerable<ThreadDetails>> GetUserThreadsAsync(int userId, string filter)
        {
            string sql = """SELECT * FROM "CurrentUserThreadsWithDetails" WHERE "UserId" = @UserId """;
            switch (filter.ToLower())
            {
                case "unread": sql += """AND "ReadStatusId" = 2 """; break;
                case "unanswered": sql += """AND "LastPostByUserId" <> @UserId """; break;
            }
            sql += """ORDER BY "LastUpdateDate" DESC""";
            return GetRecordsAsync<ThreadDetails>(sql, new { UserId = userId });
        }

        public async Task<IEnumerable<ThreadMessageViewModel>> GetThreadMessagesAsync(int threadId)
        {
            var messages = await GetRecordsAsync<ThreadMessageViewModel>("""SELECT * FROM "ThreadMessagesWithCharacterInfo" WHERE "ThreadId" = @ThreadId ORDER BY "Timestamp" ASC""", new { ThreadId = threadId });
            foreach (var message in messages)
            {
                message.AvatarImageUrl = _urlProcessingService.GetCharacterImageUrl((ImageUploadPath)message.AvatarImageUrl);
            }
            return messages;
        }

        public Task<IEnumerable<ThreadParticipantViewModel>> GetThreadParticipantsAsync(int threadId) => GetRecordsAsync<ThreadParticipantViewModel>("""SELECT "CharacterId", "CharacterDisplayName" FROM "ThreadCharacters" WHERE "ThreadId" = @ThreadId""", new { ThreadId = threadId });
        
        public async Task<PagedResult<ChatRoomWithDetails>> SearchChatRoomsAsync(int pageIndex, int pageSize, string? searchTerm, List<int> genreIds)
        {
            var parameters = new DynamicParameters();
            var whereClauses = new List<string> { """CR."ChatRoomStatusId" = 2""" };

            // This will need to be refactored to get the CurrentUserId from the UserService
            // int currentUserId = CurrentUserId;
            // parameters.Add("CurrentUserId", currentUserId);
            // whereClauses.Add("""(CR."SubmittedByUserId" NOT IN (SELECT "UserBlocked" FROM "UserBlocking" WHERE "UserBlockedBy" = @CurrentUserId) AND CR."SubmittedByUserId" NOT IN (SELECT "UserBlockedBy" FROM "UserBlocking" WHERE "UserBlocked" = @CurrentUserId))""");

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                whereClauses.Add("""CR."ChatRoomName" LIKE @SearchTerm""");
                parameters.Add("SearchTerm", $"%{searchTerm}%");
            }

            if (genreIds is not null && genreIds.Any())
            {
                whereClauses.Add("""CR."UniverseId" IN (SELECT "UniverseId" FROM "UniverseGenres" WHERE "GenreId" = ANY(@GenreIds))""");
                parameters.Add("GenreIds", genreIds);
            }

            var whereSql = string.Join(" AND ", whereClauses);

            var fromSql = $"""
                                  FROM "ChatRoomsWithDetails" CR
                                  LEFT JOIN (SELECT "ChatRoomId", MAX("PostDateTime") AS "LastPostTime" FROM "ChatRoomPosts" GROUP BY "ChatRoomId") AS LP
                                  ON CR."ChatRoomId" = LP."ChatRoomId"
                                  WHERE {whereSql}
                                  """;

            var countSql = $"SELECT COUNT(CR.\"ChatRoomId\") {fromSql}";
            var totalCount = await GetScalarAsync<int>(countSql, parameters);

            if (totalCount == 0)
            {
                return new PagedResult<ChatRoomWithDetails> { Items = Enumerable.Empty<ChatRoomWithDetails>(), TotalCount = 0, PageIndex = pageIndex, PageSize = pageSize };
            }

            var pagingSql = $"""
                                  SELECT CR.*, LP."LastPostTime"
                                  {fromSql}
                                  ORDER BY LP."LastPostTime" DESC
                                  LIMIT @Take OFFSET @Skip
                                  """;
            parameters.Add("Skip", (pageIndex - 1) * pageSize);
            parameters.Add("Take", pageSize);

            var items = await GetRecordsAsync<ChatRoomWithDetails>(pagingSql, parameters);
            return new PagedResult<ChatRoomWithDetails> { Items = items, TotalCount = totalCount, PageIndex = pageIndex, PageSize = pageSize };
        }
        public async Task<IEnumerable<DashboardChatRoom>> GetDashboardChatRoomsAsync(int userId)
        {
            try
            {
                const string sql = """
SELECT r."ChatRoomId", r."ChatRoomName", cr."ContentRatingName" as "ContentRating", lp."LastPostTime"
FROM "ChatRooms" r
JOIN "ContentRatings" cr ON r."ContentRatingId" = cr."ContentRatingId"
LEFT JOIN (
    SELECT "ChatRoomId", MAX("PostDateTime") as "LastPostTime"
    FROM "ChatRoomPosts"
    GROUP BY "ChatRoomId"
) lp ON r."ChatRoomId" = lp."ChatRoomId"
WHERE r."ChatRoomStatusId" = 2
AND r."SubmittedByUserId" NOT IN (SELECT "UserBlocked" FROM "UserBlocking" WHERE "UserBlockedBy" = @userId)
AND r."SubmittedByUserId" NOT IN (SELECT "UserBlockedBy" FROM "UserBlocking" WHERE "UserBlocked" = @userId)
ORDER BY lp."LastPostTime" DESC NULLS LAST, r."ChatRoomId" DESC
LIMIT 6
""";

                return await GetRecordsAsync<DashboardChatRoom>(sql, new { userId });
            }
            catch (Exception ex)
            {
                Console.WriteLine("*********************************");
                Console.WriteLine("EXCEPTION IN GetDashboardChatRoomsAsync");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("*********************************");
                throw;
            }
        }
        public async Task<IEnumerable<ChatParticipantViewModel>> GetChatRoomParticipantsAsync(int chatRoomId)
        {
            const string sql = """
                                SELECT DISTINCT ON (p."CharacterId") p."CharacterId", c."CharacterDisplayName", ca."AvatarImageUrl"
                                FROM "ChatRoomPosts" p
                                JOIN "Characters" c ON p."CharacterId" = c."CharacterId"
                                LEFT JOIN "CharacterAvatars" ca ON p."CharacterId" = ca."CharacterId"
                                WHERE p."ChatRoomId" = @chatRoomId
                                ORDER BY p."CharacterId";
                                """;
            return await GetRecordsAsync<ChatParticipantViewModel>(sql, new { chatRoomId });
        }

        public Task<IEnumerable<ChatRoomWithDetails>> GetMyChatRoomsAsync(int userId)
        {
            const string sql = """
                SELECT * FROM "ChatRoomsForListing" 
                WHERE ("UniverseOwnerId" = @UserId OR "SubmittedByUserId" = @UserId) 
                ORDER BY "ChatRoomName"
                """;
            return GetRecordsAsync<ChatRoomWithDetails>(sql, new { UserId = userId });
        }

        public Task<int> GetUnreadThreadCountAsync(int userId) => GetScalarAsync<int>("""SELECT COUNT(*) FROM "ThreadUsers" WHERE "ReadStatusId" = 2 AND "UserId" = @UserId""", new { UserId = userId });

        public Task<IEnumerable<User>> GetThreadRecipientsAsync(int threadId)
        {
            const string sql = """
                SELECT U.* FROM "Users" U JOIN "ThreadUsers" TU ON U."UserId" = TU."UserId"
                WHERE TU."ThreadId" = @Id AND U."ReceivesThreadNotifications" = TRUE
                """;
            return GetRecordsAsync<User>(sql, new { Id = threadId });
        }

        public Task<IEnumerable<ChannelViewModel>> GetPublicChannelsAsync()
        {
            const string sql = @"SELECT ""ChatRoomId"" AS ""Id"", ""ChatRoomName"" as ""Title"", 0 AS ""UserCount"" FROM ""ChatRooms"" WHERE ""IsPublic"" = TRUE ORDER BY ""ChatRoomName""";
            return GetRecordsAsync<ChannelViewModel>(sql);
        }

        public Task UpdateThreadTitleAsync(int threadId, string title) => ExecuteAsync("""UPDATE "Threads" SET "ThreadTitle" = @Title WHERE "ThreadId" = @ThreadId""", new { ThreadId = threadId, Title = title });
    }
}
