using Dapper;
using Microsoft.AspNetCore.Http;
using Npgsql;
using Microsoft.Extensions.Configuration;
using RoleplayersGuild.Site.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Services
{
    public class DataService : IDataService
    {
        private readonly string _connectionString;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICookieService _cookieService;
        private readonly IImageService _imageService;

        public DataService(IConfiguration config, IHttpContextAccessor httpContextAccessor, ICookieService cookieService, IImageService imageService)
        {
            _connectionString = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found in configuration.");
            _httpContextAccessor = httpContextAccessor;
            _cookieService = cookieService;
            _imageService = imageService;
        }

        private HttpContext? CurrentContext => _httpContextAccessor.HttpContext;
        private int CurrentUserId => _cookieService.GetUserId();
        private IDbConnection GetConnection() => new NpgsqlConnection(_connectionString);

        #region --- Generic Query & Execute Methods ---
        public async Task<T> GetScalarAsync<T>(string sql, object? parameters = null)
        {
            using var connection = GetConnection();
            return (await connection.ExecuteScalarAsync<T>(sql, parameters))!;
        }

        public async Task<T?> GetRecordAsync<T>(string sql, object? parameters = null)
        {
            using var connection = GetConnection();
            return await connection.QuerySingleOrDefaultAsync<T>(sql, parameters);
        }

        public async Task<IEnumerable<T>> GetRecordsAsync<T>(string sql, object? parameters = null)
        {
            using var connection = GetConnection();
            return await connection.QueryAsync<T>(sql, parameters);
        }

        public async Task<int> ExecuteAsync(string sql, object? parameters = null)
        {
            using var connection = GetConnection();
            return await connection.ExecuteAsync(sql, parameters);
        }
        #endregion

        #region --- Business Logic Methods ---
        public Task<User?> GetUserAsync(string emailAddress, string password) => GetRecordAsync<User>("""SELECT * FROM "Users" WHERE "EmailAddress" = @EmailAddress AND "Password" = @Password""", new { EmailAddress = emailAddress, Password = password });
        public Task<User?> GetUserAsync(int userId) => GetRecordAsync<User>("""SELECT * FROM "Users" WHERE "UserId" = @UserId""", new { UserId = userId });
        public Task LogErrorAsync(string errorDetails) => ExecuteAsync("""INSERT INTO "ErrorLog" ("ErrorDetails") VALUES (@ErrorDetails)""", new { ErrorDetails = errorDetails });
        public Task<int> CreateNewUserAsync(string emailAddress, string password, string username) => GetScalarAsync<int>("""INSERT INTO "Users" ("EmailAddress", "Password", "Username") VALUES (@EmailAddress, @Password, @Username) RETURNING "UserId";""", new { EmailAddress = emailAddress, Password = password, Username = username });
        public Task<User?> GetUserByEmailAsync(string emailAddress) => GetRecordAsync<User>("""SELECT * FROM "Users" WHERE "EmailAddress" = @EmailAddress""", new { EmailAddress = emailAddress });
        public Task<int> GetUserIdFromCharacterAsync(int characterId) => GetScalarAsync<int>("""SELECT "UserId" FROM "Characters" WHERE "CharacterId" = @CharacterId""", new { CharacterId = characterId });
        public Task<int> GetUserIdByEmailAsync(string emailAddress) => GetScalarAsync<int>("""SELECT "UserId" FROM "Users" WHERE "EmailAddress" = @EmailAddress""", new { EmailAddress = emailAddress });
        public Task<int> GetUserIdByUsernameAsync(string username) => GetScalarAsync<int>("""SELECT "UserId" FROM "Users" WHERE "Username" = @Username""", new { Username = username });
        public Task<int> GetUnreadThreadCountAsync(int userId) => GetScalarAsync<int>("""SELECT COUNT(*) FROM "ThreadUsers" WHERE "ReadStatusId" = 2 AND "UserId" = @UserId""", new { UserId = userId });
        public Task BanUserAsync(int userId) => ExecuteAsync("""UPDATE "Users" SET "Password" = 'UserBanned' || TO_CHAR(NOW(), 'YYYYMMDDHH12MISS'), "Username" = 'UserBanned' || TO_CHAR(NOW(), 'YYYYMMDDHH12MISS') WHERE "UserId" = @UserId""", new { UserId = userId });
        public Task<int> GetUnreadImageCommentCountAsync(int userId) => GetScalarAsync<int>("""SELECT COUNT(CIC."ImageCommentId") FROM "CharacterImageComments" CIC JOIN "CharacterImages" CI ON CIC."ImageId" = CI."CharacterImageId" JOIN "Characters" C ON CI."CharacterId" = C."CharacterId" WHERE C."UserId" = @UserId AND CIC."IsRead" = FALSE""", new { UserId = userId });
        public Task<IEnumerable<(int CharacterId, int UserId)>> GetCharacterAndUserIdsByDisplayNamesAsync(IEnumerable<string> displayNames)
        {
            const string sql = """
                SELECT "CharacterId", "UserId" 
                FROM "Characters" 
                WHERE "CharacterDisplayName" = ANY(@DisplayNames)
                """;
            return GetRecordsAsync<(int CharacterId, int UserId)>(sql, new { DisplayNames = displayNames.ToList() });
        }
        public async Task<int> GetCurrentSendAsCharacterIdAsync()
        {
            var session = CurrentContext?.Session;
            if (session is not null && session.TryGetValue("CurrentSendAsCharacter", out byte[]? value)) return BitConverter.ToInt32(value, 0);
            var userId = CurrentUserId;
            if (userId == 0) return 0;
            var sql = """SELECT "CurrentSendAsCharacter" FROM "Users" WHERE "UserId" = @UserId""";
            var characterId = await GetScalarAsync<int>(sql, new { UserId = userId });
            session?.Set("CurrentSendAsCharacter", BitConverter.GetBytes(characterId));
            return characterId;
        }

        public async Task SetCurrentSendAsCharacterIdAsync(int characterId)
        {
            var userId = CurrentUserId;
            if (userId == 0) return;
            var sql = """UPDATE "Users" SET "CurrentSendAsCharacter" = @CharacterId WHERE "UserId" = @UserId""";
            await ExecuteAsync(sql, new { CharacterId = characterId, UserId = userId });
            CurrentContext?.Session.Set("CurrentSendAsCharacter", BitConverter.GetBytes(characterId));
        }

        public Task<int> GetMembershipTypeIdAsync(int userId) => GetScalarAsync<int>("""SELECT "MembershipTypeId" FROM "Memberships" WHERE "UserId" = @UserId AND ("EndDate" IS NULL OR "EndDate" > NOW()) ORDER BY "StartDate" DESC LIMIT 1""", new { UserId = userId });
        public Task<Character?> GetCharacterAsync(int characterId) => GetRecordAsync<Character>("""SELECT * FROM "Characters" WHERE "CharacterId" = @CharacterId""", new { CharacterId = characterId });
        public Task<int> GetCharacterCountAsync(int userId) => GetScalarAsync<int>("""SELECT COUNT("CharacterId") FROM "Characters" WHERE "UserId" = @UserId""", new { UserId = userId });
        public Task<int> CreateNewCharacterAsync(int userId) =>
            GetScalarAsync<int>("""INSERT INTO "Characters" ("UserId", "IsApproved", "IsActive") VALUES (@UserId, TRUE, TRUE) RETURNING "CharacterId";""", new { UserId = userId });
        public async Task DeleteCharacterAsync(int characterId)
        {
            var images = await GetRecordsAsync<CharacterImage>("""SELECT "CharacterImageUrl" FROM "CharacterImages" WHERE "CharacterId" = @CharacterId""", new { CharacterId = characterId });
            foreach (var image in images)
            {
                await _imageService.DeleteImageAsync(image.CharacterImageUrl);
            }
            await ExecuteAsync("""DELETE FROM "Characters" WHERE "CharacterId" = @CharacterId""", new { CharacterId = characterId });
        }

        public Task UpdateCharacterAsync(CharacterInputModel model)
        {
            const string sql = """
                               UPDATE "Characters" SET 
                                   "CharacterDisplayName" = @CharacterDisplayName, 
                                   "CharacterFirstName" = @CharacterFirstName, 
                                   "CharacterMiddleName" = @CharacterMiddleName, 
                                   "CharacterLastName" = @CharacterLastName, 
                                   "CharacterBio" = @CharacterBio, 
                                   "CharacterGender" = @CharacterGender, 
                                   "SexualOrientation" = @SexualOrientation, 
                                   "CharacterSourceId" = @CharacterSourceId, 
                                   "PostLengthMin" = @PostLengthMin, 
                                   "PostLengthMax" = @PostLengthMax, 
                                   "LiteracyLevel" = @LiteracyLevel, 
                                   "LfrpStatus" = @LfrpStatus, 
                                   "EroticaPreferences" = @EroticaPreferences, 
                                   "MatureContent" = @MatureContent, 
                                   "IsPrivate" = @IsPrivate, 
                                   "DisableLinkify" = @DisableLinkify,
                                   "CardImageUrl" = @CardImageUrl
                               WHERE "CharacterId" = @CharacterId
                               """;
            return ExecuteAsync(sql, model);
        }

        public async Task UpdateCharacterGenresAsync(int characterId, List<int> genreIds)
        {
            await ExecuteAsync("""DELETE FROM "CharacterGenres" WHERE "CharacterId" = @characterId""", new { characterId });
            if (genreIds is not null && genreIds.Any())
            {
                var sql = """INSERT INTO "CharacterGenres" ("CharacterId", "GenreId") VALUES (@characterId, @genreId);""";
                foreach (var genreId in genreIds) { await ExecuteAsync(sql, new { characterId, genreId }); }
            }
        }

        public Task UpdateCharacterBadgeAssignmentAsync(int characterId, int userBadgeId)
        {
            const string sql = """UPDATE "UserBadges" SET "AssignedToCharacterId" = 0 WHERE "AssignedToCharacterId" = @characterId; UPDATE "UserBadges" SET "AssignedToCharacterId" = @characterId WHERE "UserBadgeId" = @userBadgeId;""";
            return ExecuteAsync(sql, new { characterId, userBadgeId });
        }

        public Task<int> GetAssignedUserBadgeIdAsync(int characterId) => GetScalarAsync<int>("""SELECT "UserBadgeId" FROM "UserBadges" WHERE "AssignedToCharacterId" = @characterId""", new { characterId });
        public Task<IEnumerable<Character>> GetActiveCharactersForUserAsync(int userId) => GetRecordsAsync<Character>("""SELECT * FROM "CharactersForListing" WHERE "UserId" = @userId AND "IsApproved" = TRUE""", new { userId });
        public Task<Universe?> GetUniverseAsync(int universeId) => GetRecordAsync<Universe>("""SELECT * FROM "Universes" WHERE "UniverseId" = @UniverseId""", new { UniverseId = universeId });
        public Task<UniverseWithDetails?> GetUniverseWithDetailsAsync(int universeId) => GetRecordAsync<UniverseWithDetails>("""SELECT * FROM "UniversesWithDetails" WHERE "UniverseId" = @UniverseId""", new { UniverseId = universeId });
        public Task<int> CreateNewUniverseAsync(int userId) => GetScalarAsync<int>("""INSERT INTO "Universes" ("UniverseOwnerId", "SubmittedById", "StatusId") VALUES (@UserId, @UserId, 1) RETURNING "UniverseId";""", new { UserId = userId });
        public Task DeleteUniverseAsync(int universeId) => ExecuteAsync("""DELETE FROM "Universes" WHERE "UniverseId" = @UniverseId;""", new { UniverseId = universeId });
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
        public Task<CharacterImage?> GetImageAsync(int imageId) => GetRecordAsync<CharacterImage>("""SELECT CI.*, C."UserId" FROM "CharacterImages" CI JOIN "Characters" C ON CI."CharacterId" = C."CharacterId" WHERE CI."CharacterImageId" = @imageId""", new { imageId });
        public Task<CharacterImage?> GetDisplayImageAsync(int characterId) => GetRecordAsync<CharacterImage>("""SELECT * FROM "CharacterImages" WHERE "CharacterId" = @characterId AND "IsPrimary" = TRUE""", new { characterId });
        public Task AddImageAsync(string imageUrl, int characterId, int userId, bool isPrimary, bool isMature, string imageCaption) =>
            ExecuteAsync("""
            INSERT INTO "CharacterImages" 
                ("CharacterImageUrl", "CharacterId", "UserId", "IsPrimary", "IsMature", "ImageCaption") 
            VALUES 
                (@ImageUrl, @CharacterId, @UserId, @IsPrimary, @IsMature, @ImageCaption)
            """,
                new { ImageUrl = imageUrl, CharacterId = characterId, UserId = userId, IsPrimary = isPrimary, IsMature = isMature, ImageCaption = imageCaption });
        public Task RemoveDefaultFlagFromImagesAsync(int characterId) => ExecuteAsync("""UPDATE "CharacterImages" SET "IsPrimary" = FALSE WHERE "CharacterId" = @characterId""", new { characterId });
        public Task UpdateImageAsync(int imageId, bool isPrimary, bool isMature, string imageCaption) => ExecuteAsync("""UPDATE "CharacterImages" SET "IsPrimary" = @IsPrimary, "IsMature" = @IsMature, "ImageCaption" = @ImageCaption WHERE "CharacterImageId" = @imageId""", new { imageId, isPrimary, isMature, imageCaption });
        public Task<CharacterImage?> GetImageWithOwnerAsync(int imageId) => GetRecordAsync<CharacterImage>("""SELECT CI.*, C."UserId" FROM "CharacterImages" CI JOIN "Characters" C ON CI."CharacterId" = C."CharacterId" WHERE CI."CharacterImageId" = @imageId""", new { imageId });

        public async Task<int> GetAvailableImageSlotCountAsync(int userId, int characterId)
        {
            var membershipTypeId = await GetMembershipTypeIdAsync(userId);
            int maxImages = membershipTypeId switch { 1 => 20, 2 => 30, 3 => 40, 4 => 100, _ => 10, };
            var usedSlots = await GetScalarAsync<int>("""SELECT COUNT("CharacterImageId") FROM "CharacterImages" CI JOIN "Characters" C ON CI."CharacterId" = C."CharacterId" WHERE C."UserId" = @userId""", new { userId });
            return maxImages - usedSlots > 0 ? maxImages - usedSlots : 0;
        }

        public Task UpdateImageDetailsAsync(int imageId, string caption, bool isPrimary) => ExecuteAsync("""UPDATE "CharacterImages" SET "ImageCaption" = @caption, "IsPrimary" = @isPrimary WHERE "CharacterImageId" = @imageId""", new { imageId, caption, isPrimary });
        public Task DeleteImageRecordAsync(int imageId) => ExecuteAsync("""DELETE FROM "CharacterImages" WHERE "CharacterImageId" = @ImageId""", new { ImageId = imageId });
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
        public Task<int> GetBlockRecordIdAsync(int blockeeUserId, int blockerUserId) => GetScalarAsync<int>("""SELECT "UserBlockId" FROM "UserBlocking" WHERE "UserBlocked" = @BlockeeUserId AND "UserBlockedBy" = @BlockerUserId""", new { BlockeeUserId = blockeeUserId, BlockerUserId = blockerUserId });
        public Task BlockUserAsync(int blockerUserId, int blockeeUserId) => ExecuteAsync("""INSERT INTO "UserBlocking" ("UserBlocked", "UserBlockedBy") VALUES (@BlockeeUserId, @BlockerUserId)""", new { BlockeeUserId = blockeeUserId, BlockerUserId = blockerUserId });
        public Task UnblockUserAsync(int blockerUserId, int blockedUserId) => ExecuteAsync("""DELETE FROM "UserBlocking" WHERE "UserBlockedBy" = @blockerUserId AND "UserBlocked" = @blockedUserId""", new { blockerUserId, blockedUserId });

        public Task AwardHolidayBadgeAsync(string badgeType, int badgeId, int userId)
        {
            string sql;
            switch (badgeType)
            {
                case "LastHalloweenBadge":
                    sql = """INSERT INTO "UserBadges" ("UserId", "BadgeId") VALUES (@UserId, @BadgeId); UPDATE "Users" SET "LastHalloweenBadge" = NOW() WHERE "UserId" = @UserId;""";
                    break;
                case "LastChristmasBadge":
                    sql = """INSERT INTO "UserBadges" ("UserId", "BadgeId") VALUES (@UserId, @BadgeId); UPDATE "Users" SET "LastChristmasBadge" = NOW() WHERE "UserId" = @UserId;""";
                    break;
                default:
                    throw new ArgumentException("Invalid badge type specified.", nameof(badgeType));
            }
            return ExecuteAsync(sql, new { UserId = userId, BadgeId = badgeId });
        }

        public Task AwardBadgeIfNotExistingAsync(int badgeId, int userId) => ExecuteAsync("""CALL "AwardNewBadgeIfNotAwarded"(@BadgeId, @UserId)""", new { BadgeId = badgeId, UserId = userId });
        public Task<IEnumerable<AssignableBadge>> GetAssignableBadgesAsync(int userId, int characterId) => GetRecordsAsync<AssignableBadge>("""SELECT B."BadgeName", UB."UserBadgeId" FROM "Badges" B JOIN "UserBadges" UB ON B."BadgeId" = UB."BadgeId" WHERE UB."UserId" = @userId AND B."CharacterAssignable" = TRUE AND (UB."AssignedToCharacterId" = 0 OR UB."AssignedToCharacterId" = @characterId) ORDER BY UB."DateReceived" DESC""", new { userId, characterId });
        public Task<IEnumerable<UserBadgeViewModel>> GetAvailableBadgesAsync() => GetRecordsAsync<UserBadgeViewModel>("""SELECT "BadgeName", "BadgeImageUrl", "BadgeDescription" FROM "Badges" WHERE "BadgeId" <> 69 ORDER BY "SortOrder" """);
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

        public async Task<DashboardFunding> GetDashboardFundingAsync()
        {
            const decimal goalAmount = 150.00m;
            var currentAmount = await GetScalarAsync<decimal?>("""SELECT "CurrentFundingAmount" FROM "GeneralSettings" """) ?? 0m;
            var percentage = goalAmount > 0 ? (int)((currentAmount / goalAmount) * 100) : 0;
            return new DashboardFunding { CurrentFundingAmount = currentAmount, GoalAmount = goalAmount, ProgressPercentage = Math.Min(100, percentage) };
        }

        public Task<int> GetOpenAdminItemCountAsync(int userId) => GetScalarAsync<int>("""SELECT COUNT("ItemId") FROM "TodoItems" WHERE "AssignedToUserId" = @userId AND "StatusId" = 1""", new { userId });
        public Task<IEnumerable<DashboardChatRoom>> GetActiveChatRoomsForDashboardAsync(int userId) => GetRecordsAsync<DashboardChatRoom>("""SELECT * FROM "ActiveChatrooms" WHERE "ChatRoomId" NOT IN (SELECT "ChatRoomId" FROM "ChatRoomLocks" WHERE "UserId" = @userId) AND "SubmittedByUserId" NOT IN (SELECT "UserBlocked" FROM "UserBlocking" WHERE "UserBlockedBy" = @userId) AND "SubmittedByUserId" NOT IN (SELECT "UserBlockedBy" FROM "UserBlocking" WHERE "UserBlocked" = @userId) ORDER BY "LastPostTime" DESC LIMIT 5""", new { userId });
        public Task<IEnumerable<DashboardStory>> GetPopularStoriesForDashboardAsync(int userId) => GetRecordsAsync<DashboardStory>("""SELECT Pop.*, CR."ContentRatingName" FROM "PopularStories" AS Pop INNER JOIN "Stories" AS S ON Pop."StoryId" = S."StoryId" INNER JOIN "ContentRatings" AS CR ON S."ContentRatingId" = CR."ContentRatingId" WHERE Pop."UserId" NOT IN (SELECT "UserBlocked" FROM "UserBlocking" WHERE "UserBlockedBy" = @userId) AND Pop."UserId" NOT IN (SELECT "UserBlockedBy" FROM "UserBlocking" WHERE "UserBlocked" = @userId) ORDER BY Pop."LastPostDateTime" DESC LIMIT 5""", new { userId });
        public Task<IEnumerable<DashboardArticle>> GetNewestArticlesForDashboardAsync(int userId) => GetRecordsAsync<DashboardArticle>("""SELECT * FROM "ArticlesForListing" WHERE ("CategoryId" <> 7 AND "CategoryId" <> 8 AND "CategoryId" <> 9 AND "CategoryId" <> 10) AND "OwnerUserId" NOT IN (SELECT "UserBlocked" FROM "UserBlocking" WHERE "UserBlockedBy" = @userId) AND "OwnerUserId" NOT IN (SELECT "UserBlockedBy" FROM "UserBlocking" WHERE "UserBlocked" = @userId) ORDER BY "CreatedDateTime" DESC LIMIT 5""", new { userId });
        public Task<IEnumerable<DashboardProposal>> GetNewestProposalsForDashboardAsync(int userId) => GetRecordsAsync<DashboardProposal>("""SELECT * FROM "ProposalsWithDetails" WHERE "StatusId" = 1 AND "UserId" NOT IN (SELECT "UserBlocked" FROM "UserBlocking" WHERE "UserBlockedBy" = @userId) AND "UserId" NOT IN (SELECT "UserBlockedBy" FROM "UserBlocking" WHERE "UserBlocked" = @userId) ORDER BY "LastUpdated" DESC LIMIT 5""", new { userId });


        public async Task<IEnumerable<CharactersForListing>> GetCharactersForListingAsync(string screenStatus, int recordCount, int currentUserId)
        {
            var whereClauses = new List<string>
    {
        """u."UserId" NOT IN (SELECT "UserBlocked" FROM "UserBlocking" WHERE "UserBlockedBy" = @currentUserId)""",
        """u."UserId" NOT IN (SELECT "UserBlockedBy" FROM "UserBlocking" WHERE "UserBlocked" = @currentUserId)"""
    };

            string orderByClause = screenStatus == "OnlineCharacters"
                ? """ORDER BY u."LastAction" DESC"""
                : """ORDER BY c."DateSubmitted" DESC""";

            if (screenStatus == "OnlineCharacters")
            {
                whereClauses.Add("""u."LastAction" > (NOW() AT TIME ZONE 'UTC' - interval '15 minute')""");
            }

            string sql = $"""
        SELECT c."CharacterId", c."UserId", c."CharacterDisplayName", c."CardImageUrl", u."LastAction", u."ShowWhenOnline",
               ca."AvatarImageUrl", (SELECT B."CharacterNameClass" FROM "Badges" B JOIN "UserBadges" UB ON B."BadgeId" = UB."BadgeId" 
                                    WHERE UB."UserId" = c."UserId" AND B."CharacterNameClass" IS NOT NULL 
                                    ORDER BY B."SortOrder" LIMIT 1) AS "CharacterNameClass"
        FROM "Characters" c
        JOIN "Users" u ON c."UserId" = u."UserId"
        LEFT JOIN "CharacterAvatars" ca ON c."CharacterId" = ca."CharacterId"
        WHERE {string.Join(" AND ", whereClauses)}
        {orderByClause}
        LIMIT @recordCount
        """;

            var rawData = await GetRecordsAsync<dynamic>(sql, new { recordCount, currentUserId });

            return rawData.Select(d => new CharactersForListing
            {
                CharacterId = d.CharacterId,
                UserId = d.UserId,
                CharacterDisplayName = d.CharacterDisplayName,
                DisplayImageUrl = _imageService.GetImageUrl(d.CardImageUrl),
                AvatarImageUrl = _imageService.GetImageUrl(d.AvatarImageUrl),
                CharacterNameClass = d.CharacterNameClass,
                LastAction = d.LastAction,
                ShowWhenOnline = d.ShowWhenOnline
            }).ToList();
        }
        public async Task<CharacterWithDetails?> GetCharacterWithDetailsAsync(int characterId)
        {
            const string sql = """SELECT * FROM "
                " WHERE "CharacterId" = @characterId""";
            var character = await GetRecordAsync<CharacterWithDetails>(sql, new { characterId });

            if (character != null)
            {
                ImageProcessingHelpers.ProcessCharacterDetails(character, _imageService);
            }
            return character;
        }

        public Task<IEnumerable<Genre>> GetCharacterGenresAsync(int characterId) => GetRecordsAsync<Genre>("""SELECT G."GenreId", G."GenreName" FROM "Genres" G JOIN "CharacterGenres" CG ON G."GenreId" = CG."GenreId" WHERE CG."CharacterId" = @characterId ORDER BY G."GenreName" """, new { characterId });
        public async Task<bool> IsUserBlockedAsync(int blockedUserId, int blockerUserId) => await GetScalarAsync<int>("""SELECT COUNT(1) FROM "UserBlocking" WHERE "UserBlocked" = @blockedUserId AND "UserBlockedBy" = @blockerUserId""", new { blockedUserId, blockerUserId }) > 0;

        public async Task<PagedResult<CharactersForListing>> SearchCharactersAsync(SearchInputModel search, int currentUserId, int pageIndex, int pageSize)
        {
            var whereClauses = new List<string> { """c."IsApproved" = TRUE AND c."IsPrivate" = FALSE""" };
            var parameters = new DynamicParameters();
            parameters.Add("CurrentUserId", currentUserId);
            whereClauses.Add("""(u."UserId" NOT IN (SELECT "UserBlocked" FROM "UserBlocking" WHERE "UserBlockedBy" = @CurrentUserId) AND u."UserId" NOT IN (SELECT "UserBlockedBy" FROM "UserBlocking" WHERE "UserBlocked" = @CurrentUserId))""");

            if (!string.IsNullOrWhiteSpace(search.Name))
            {
                if (int.TryParse(search.Name, out var characterId))
                {
                    whereClauses.Add("""c."CharacterId" = @CharacterId""");
                    parameters.Add("CharacterId", characterId);
                }
                else
                {
                    whereClauses.Add("""(c."CharacterDisplayName" ILIKE @Name OR c."CharacterFirstName" ILIKE @Name)""");
                    parameters.Add("Name", $"%{search.Name}%");
                }
            }
            if (search.GenderId > 0)
            {
                whereClauses.Add("""c."CharacterGender" = @GenderId""");
                parameters.Add("GenderId", search.GenderId);
            }
            if (search.SelectedGenreIds.Any())
            {
                whereClauses.Add("""c."CharacterId" IN (SELECT "CharacterId" FROM "CharacterGenres" WHERE "GenreId" = ANY(@GenreIds))""");
                parameters.Add("GenreIds", search.SelectedGenreIds);
            }

            var whereSql = string.Join(" AND ", whereClauses);
            var fromSql = $"""
    FROM "Characters" c
    JOIN "Users" u ON c."UserId" = u."UserId"
    LEFT JOIN "CharacterAvatars" ca ON c."CharacterId" = ca."CharacterId"
    WHERE {whereSql}
    """;

            var countSql = $"SELECT COUNT(c.\"CharacterId\") {fromSql}";
            var totalCount = await GetScalarAsync<int>(countSql, parameters);
            if (totalCount == 0) return new PagedResult<CharactersForListing> { Items = Enumerable.Empty<CharactersForListing>(), PageIndex = pageIndex, PageSize = pageSize };

            string orderByClause = search.SortOrder switch { 1 => """ORDER BY c."CharacterId" ASC""", _ => """ORDER BY c."DateSubmitted" DESC NULLS LAST, c."CharacterId" DESC""", };
            var pagingSql = $"""
    SELECT c."CharacterId", c."UserId", c."CharacterDisplayName", c."CardImageUrl", u."LastAction", u."ShowWhenOnline",
           ca."AvatarImageUrl", (SELECT B."CharacterNameClass" FROM "Badges" B JOIN "UserBadges" UB ON B."BadgeId" = UB."BadgeId" WHERE UB."UserId" = c."UserId" AND B."CharacterNameClass" IS NOT NULL ORDER BY B."SortOrder" LIMIT 1) AS "CharacterNameClass"
    {fromSql}
    {orderByClause}
    LIMIT @Take OFFSET @Skip
    """;
            parameters.Add("Skip", (pageIndex - 1) * pageSize);
            parameters.Add("Take", pageSize);

            var rawData = await GetRecordsAsync<dynamic>(pagingSql, parameters);

            var items = rawData.Select(d => new CharactersForListing
            {
                CharacterId = d.CharacterId,
                UserId = d.UserId,
                CharacterDisplayName = d.CharacterDisplayName,
                DisplayImageUrl = _imageService.GetImageUrl(d.CardImageUrl),
                AvatarImageUrl = _imageService.GetImageUrl(d.AvatarImageUrl),
                CharacterNameClass = d.CharacterNameClass,
                LastAction = d.LastAction,
                ShowWhenOnline = d.ShowWhenOnline
            }).ToList();

            return new PagedResult<CharactersForListing>
            {
                Items = items,
                TotalCount = totalCount,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
        }
        public Task<IEnumerable<ToDoItemViewModel>> GetDevelopmentItemsAsync() => GetRecordsAsync<ToDoItemViewModel>("""SELECT "ItemId", "ItemName", "ItemDescription" FROM "TodoItemsWithDetails" WHERE "TypeId" = 2 AND "StatusId" = 4 AND "AssignedToUserId" = 2 ORDER BY "ItemId" """);
        public Task<IEnumerable<ToDoItemViewModel>> GetConsiderationItemsAsync() => GetRecordsAsync<ToDoItemViewModel>("""SELECT "ItemId", "ItemName", "ItemDescription", "VoteCount" FROM "TodoItemsWithDetails" WHERE "TypeId" = 1 AND ("StatusId" = 1 OR "StatusId" = 4) ORDER BY "VoteCount" DESC, "ItemId" """);
        public Task<int> AddToDoItemAsync(string name, string description, int userId) => GetScalarAsync<int>("""INSERT INTO "TodoItems" ("ItemName", "ItemDescription", "StatusId", "TypeId", "CreatedByUserId") VALUES (@ItemName, @ItemDescription, 1, 1, @CreatedByUserId) RETURNING "ItemId" """, new { ItemName = name, ItemDescription = description, CreatedByUserId = userId });
        public Task AddVoteAsync(int itemId, int userId) => ExecuteAsync("""INSERT INTO "TodoItemVotes" ("TodoItemId", "UserId") VALUES (@ItemId, @UserId)""", new { ItemId = itemId, UserId = userId });
        public Task RemoveVoteAsync(int itemId, int userId) => ExecuteAsync("""DELETE FROM "TodoItemVotes" WHERE "TodoItemId" = @ItemId AND "UserId" = @UserId""", new { ItemId = itemId, UserId = userId });
        public async Task<bool> HasUserVotedAsync(int itemId, int userId) => await GetScalarAsync<int>("""SELECT COUNT(1) FROM "TodoItemVotes" WHERE "TodoItemId" = @ItemId AND "UserId" = @UserId""", new { ItemId = itemId, UserId = userId }) > 0;

        public async Task<IEnumerable<ImageCommentViewModel>> GetImageCommentsAsync(int imageId)
        {
            var comments = await GetRecordsAsync<ImageCommentViewModel>("""SELECT * FROM "ImageCommentsWithDetails" WHERE "ImageId" = @ImageId ORDER BY "CommentTimestamp" DESC""", new { ImageId = imageId });
            ProcessImageComments(comments);
            return comments;
        }

        public Task AddImageCommentAsync(int imageId, int characterId, string commentText) => ExecuteAsync("""INSERT INTO "CharacterImageComments" ("ImageId", "CharacterId", "CommentText") VALUES (@ImageId, @CharacterId, @CommentText)""", new { ImageId = imageId, CharacterId = characterId, CommentText = commentText });
        public Task DeleteImageCommentAsync(int commentId, int userId) => ExecuteAsync("""DELETE FROM "CharacterImageComments" AS CIC USING "CharacterImages" AS CI, "Characters" AS C_OWNER, "Characters" AS C_COMMENTER WHERE CIC."ImageId" = CI."CharacterImageId" AND CI."CharacterId" = C_OWNER."CharacterId" AND CIC."CharacterId" = C_COMMENTER."CharacterId" AND CIC."ImageCommentId" = @CommentId AND (@UserId = C_OWNER."UserId" OR @UserId = C_COMMENTER."UserId")""", new { CommentId = commentId, UserId = userId });

        public async Task<PagedResult<CharacterImage>> GetCharacterImagesAsync(int characterId, int pageIndex, int pageSize)
        {
            const string baseQuery = """FROM "CharacterImages" WHERE ("CharacterId" = @CharacterId)""";
            var countSql = "SELECT COUNT(\"CharacterImageId\") " + baseQuery;
            var pagingSql = $"SELECT * {baseQuery} ORDER BY \"CharacterImageId\" LIMIT @Take OFFSET @Skip";

            var totalCount = await GetScalarAsync<int>(countSql, new { CharacterId = characterId });
            if (totalCount == 0) return new PagedResult<CharacterImage> { PageIndex = pageIndex, PageSize = pageSize };

            var items = await GetRecordsAsync<CharacterImage>(pagingSql, new { CharacterId = characterId, Skip = (pageIndex - 1) * pageSize, Take = pageSize });

            return new PagedResult<CharacterImage>
            {
                Items = items.Select(img => ImageProcessingHelpers.ProcessCharacterImage(img, _imageService)).ToList(),
                TotalCount = totalCount,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
        }

        public async Task<CharacterImageWithDetails?> GetImageDetailsAsync(int imageId)
        {
            var image = await GetRecordAsync<CharacterImageWithDetails>("""SELECT CI."CharacterImageId", CI."CharacterId", C."UserId", C."CharacterDisplayName", CI."CharacterImageUrl", CI."ImageCaption", CI."IsMature", CI."IsPrimary" FROM "CharacterImages" CI JOIN "Characters" C ON CI."CharacterId" = C."CharacterId" WHERE CI."CharacterImageId" = @imageId""", new { imageId });
            ProcessImageDetails(image);
            return image;
        }

        public Task<Character?> GetCharacterForEditAsync(int characterId, int userId) => GetRecordAsync<Character>("""SELECT * FROM "Characters" WHERE "CharacterId" = @characterId AND "UserId" = @userId""", new { characterId, userId });
        public Task UpdateCharacterCustomProfileAsync(int characterId, string? css, string? html, bool isEnabled) => ExecuteAsync("""UPDATE "Characters" SET "ProfileCss" = @CSS, "ProfileHtml" = @HTML, "CustomProfileEnabled" = @Enabled WHERE "CharacterId" = @CharID""", new { CSS = css, HTML = html, Enabled = isEnabled, CharID = characterId });
        public async Task<IEnumerable<CharacterImage>> GetCharacterImagesForGalleryAsync(int characterId)
        {
            const string sql = """
                               SELECT CI."CharacterImageId", CI."CharacterImageUrl", CI."IsMature", CI."ImageCaption", CI."UserId",
                                      (SELECT COUNT(CIC."ImageCommentId") FROM "CharacterImageComments" CIC 
                                       WHERE CIC."ImageId" = CI."CharacterImageId" AND CIC."IsRead" = FALSE) AS "UnreadCommentCount" 
                               FROM "CharacterImages" CI 
                               WHERE CI."CharacterId" = @characterId 
                               ORDER BY CI."CharacterImageId"
                               """;
            // Now it correctly returns the raw data from the database.
            return await GetRecordsAsync<CharacterImage>(sql, new { characterId });
        }

        public Task<IEnumerable<ProposalWithDetails>> GetUserProposalsAsync(int userId) => GetRecordsAsync<ProposalWithDetails>("""SELECT * FROM "ProposalsWithDetails" WHERE ("UserId" = @UserId) ORDER BY "Title" """, new { UserId = userId });
        public Task<IEnumerable<QuickLink>> GetUserQuickLinksAsync(int userId) => GetRecordsAsync<QuickLink>("""SELECT * FROM "QuickLinks" WHERE ("UserId" = @UserId) ORDER BY "OrderNumber", "LinkName" """, new { UserId = userId });
        public Task AddQuickLinkAsync(QuickLink newLink) => ExecuteAsync("""INSERT INTO "QuickLinks" ("UserId", "LinkName", "LinkAddress", "OrderNumber") VALUES (@UserId, @LinkName, @LinkAddress, @OrderNumber)""", newLink);
        public Task DeleteQuickLinkAsync(int quickLinkId, int userId) => ExecuteAsync("""DELETE FROM "QuickLinks" WHERE "QuickLinkId" = @QuickLinkId AND "UserId" = @UserId""", new { QuickLinkId = quickLinkId, UserId = userId });

        public Task UpdateUserSettingsAsync(int userId, SettingsInputModel settings)
        {
            return ExecuteAsync("""UPDATE "Users" SET "Username" = @Username, "EmailAddress" = @EmailAddress, "ShowWhenOnline" = @ShowWhenOnline, "ShowWriterLinkOnCharacters" = @ShowWriterLinkOnCharacters, "ReceivesThreadNotifications" = @ReceivesThreadNotifications, "ReceivesImageCommentNotifications" = @ReceivesImageCommentNotifications, "ReceivesWritingCommentNotifications" = @ReceivesWritingCommentNotifications, "ReceivesDevEmails" = @ReceivesDevEmails, "ReceivesErrorFixEmails" = @ReceivesErrorFixEmails, "ReceivesGeneralEmailBlasts" = @ReceivesGeneralEmailBlasts, "ShowMatureContent" = @ShowMatureContent, "UseDarkTheme" = @UseDarkTheme WHERE "UserId" = @UserId""",
                new { UserId = userId, settings.Username, settings.EmailAddress, settings.ShowWhenOnline, settings.ShowWriterLinkOnCharacters, settings.ReceivesThreadNotifications, settings.ReceivesImageCommentNotifications, settings.ReceivesWritingCommentNotifications, settings.ReceivesDevEmails, settings.ReceivesErrorFixEmails, settings.ReceivesGeneralEmailBlasts, settings.ShowMatureContent, settings.UseDarkTheme });
        }

        public async Task<bool> IsUsernameTakenAsync(string username, int currentUserId) => await GetScalarAsync<int>("""SELECT COUNT(1) FROM "Users" WHERE "Username" = @Username AND "UserId" <> @CurrentUserId""", new { Username = username, CurrentUserId = currentUserId }) > 0;
        public Task<StoryPost?> GetStoryPostForEditAsync(int storyPostId, int userId) => GetRecordAsync<StoryPost>("""SELECT SP.* FROM "StoryPosts" SP JOIN "Characters" C ON SP."CharacterId" = C."CharacterId" WHERE SP."StoryPostId" = @StoryPostId AND C."UserId" = @UserId""", new { StoryPostId = storyPostId, UserId = userId });
        public Task UpdateStoryPostAsync(int storyPostId, int characterId, string content) => ExecuteAsync("""UPDATE "StoryPosts" SET "CharacterId" = @CharacterId, "PostContent" = @PostContent WHERE "StoryPostId" = @StoryPostId""", new { StoryPostId = storyPostId, CharacterId = characterId, PostContent = content });
        public Task<IEnumerable<StoryWithDetails>> GetUserStoriesAsync(int userId) => GetRecordsAsync<StoryWithDetails>("""SELECT * FROM "StoriesWithDetails" WHERE "UserId" = @UserId ORDER BY "StoryTitle" """, new { UserId = userId });

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
            ProcessThreadMessages(messages);
            return messages;
        }

        public Task<IEnumerable<ThreadParticipantViewModel>> GetThreadParticipantsAsync(int threadId) => GetRecordsAsync<ThreadParticipantViewModel>("""SELECT "CharacterId", "CharacterDisplayName" FROM "ThreadCharacters" WHERE "ThreadId" = @ThreadId""", new { ThreadId = threadId });

        public async Task<IEnumerable<UniverseWithDetails>> GetUserUniversesAsync(int userId)
        {
            // 1. Fetch all required universe data in one query from the view
            const string sql = """
                SELECT "UniverseId", "UniverseName", "UniverseDescription", "SourceType", "ContentRating", "CharacterCount"
                FROM "UniversesForListing" 
                WHERE "UniverseOwnerId" = @UserId 
                ORDER BY "UniverseName"
                """;
            var universes = (await GetRecordsAsync<UniverseWithDetails>(sql, new { UserId = userId })).ToList();

            if (!universes.Any())
            {
                return universes; // Return early if there are no universes
            }

            // 2. Get all genres for all fetched universes in a single second query
            var universeIds = universes.Select(u => u.UniverseId).ToList();
            const string genreSql = """
                SELECT UG."UniverseId", G."GenreName" 
                FROM "UniverseGenres" UG 
                JOIN "Genres" G ON UG."GenreId" = G."GenreId" 
                WHERE UG."UniverseId" = ANY(@UniverseIds)
                """;
            var genreLookup = (await GetRecordsAsync<(int UniverseId, string GenreName)>(genreSql, new { UniverseIds = universeIds }))
                                .ToLookup(g => g.UniverseId, g => g.GenreName);

            // 3. Assign genres to each universe in memory
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
        public Task UpdateUserAboutMeAsync(int userId, string aboutMe) => ExecuteAsync("""UPDATE "Users" SET "AboutMe" = @AboutMe WHERE "UserId" = @UserId""", new { AboutMe = aboutMe, UserId = userId });

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

            ProcessStoryPosts(items);
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

        public Task UnsubscribeUserFromNotificationsAsync(int userId)
        {
            const string sql = """UPDATE "Users" SET "ReceivesThreadNotifications" = FALSE, "ReceivesImageCommentNotifications" = FALSE WHERE "UserId" = @UserId;""";
            return ExecuteAsync(sql, new { UserId = userId });
        }

        public Task<IEnumerable<UserBadgeViewModel>> GetUserBadgesAsync(int userId)
        {
            const string sql = """
            SELECT B."BadgeName", B."BadgeImageUrl", B."BadgeDescription"
            FROM "Badges" B
            INNER JOIN "UserBadges" UB ON B."BadgeId" = UB."BadgeId"
            WHERE UB."UserId" = @UserId
            ORDER BY B."SortOrder", B."BadgeName"
            """;
            return GetRecordsAsync<UserBadgeViewModel>(sql, new { UserId = userId });
        }

        public Task<IEnumerable<ArticleForListingViewModel>> GetUserPublicArticlesAsync(int userId)
        {
            const string sql = """
            SELECT "ArticleId", "ArticleTitle", "ContentRating"
            FROM "ArticlesForListing"
            WHERE "CategoryId" NOT IN (8, 9, 10)
            AND "OwnerUserId" = @UserId
            AND "IsPrivate" = FALSE
            ORDER BY "ArticleTitle"
            """;
            return GetRecordsAsync<ArticleForListingViewModel>(sql, new { UserId = userId });
        }

        public async Task<IEnumerable<CharactersForListing>> GetUserPublicCharactersAsync(int userId)
        {
            const string sql = """
                               SELECT c."CharacterId", c."UserId", c."CharacterDisplayName", c."CardImageUrl", u."LastAction", u."ShowWhenOnline",
                                      ca."AvatarImageUrl", (SELECT B."CharacterNameClass" FROM "Badges" B JOIN "UserBadges" UB ON B."BadgeId" = UB."BadgeId" WHERE UB."UserId" = c."UserId" AND B."CharacterNameClass" IS NOT NULL ORDER BY B."SortOrder" LIMIT 1) AS "CharacterNameClass"
                               FROM "Characters" c
                               JOIN "Users" u ON c."UserId" = u."UserId"
                               LEFT JOIN "CharacterAvatars" ca ON c."CharacterId" = ca."CharacterId"
                               WHERE c."UserId" = @UserId
                               AND c."IsPrivate" = FALSE
                               AND c."CharacterStatusId" = 1
                               ORDER BY c."CharacterDisplayName"
                               """;
            var rawData = await GetRecordsAsync<dynamic>(sql, new { UserId = userId });

            return rawData.Select(d => new CharactersForListing
            {
                CharacterId = d.CharacterId,
                UserId = d.UserId,
                CharacterDisplayName = d.CharacterDisplayName,
                DisplayImageUrl = _imageService.GetImageUrl(d.CardImageUrl),
                AvatarImageUrl = _imageService.GetImageUrl(d.AvatarImageUrl),
                CharacterNameClass = d.CharacterNameClass,
                LastAction = d.LastAction,
                ShowWhenOnline = d.ShowWhenOnline
            }).ToList();
        }

        public async Task<int> UpsertArticleAsync(ArticleInputModel input, int userId)
        {
            // If the ArticleId is 0, this is a new article and we INSERT it.
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
            // Otherwise, it's an existing article we need to UPDATE.
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
                    OwnerUserId = userId, // Security check to ensure users can only edit their own articles
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
            // If the ProposalId is 0, this is a new proposal.
            if (input.ProposalId == 0)
            {
                // Reuse the existing method for creating a new proposal.
                return await CreateProposalAsync(input, userId);
            }
            // Otherwise, it's an existing proposal.
            else
            {
                // Reuse the existing method for updating.
                await UpdateProposalAsync(input, userId);
                return input.ProposalId;
            }
        }

        public async Task<int> UpsertStoryAsync(StoryInputModel input, int userId)
        {
            if (input.StoryId == 0)
            {
                // This is a new story, so we INSERT it.
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
                // This is an existing story, so we UPDATE it.
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
                    UserId = userId, // Security check
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

            int currentUserId = CurrentUserId;
            parameters.Add("CurrentUserId", currentUserId);
            whereClauses.Add("""(P."UserId" NOT IN (SELECT "UserBlocked" FROM "UserBlocking" WHERE "UserBlockedBy" = @CurrentUserId) AND P."UserId" NOT IN (SELECT "UserBlockedBy" FROM "UserBlocking" WHERE "UserBlocked" = @CurrentUserId))""");

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

        public async Task<PagedResult<ChatRoomWithDetails>> SearchChatRoomsAsync(int pageIndex, int pageSize, string? searchTerm, List<int> genreIds)
        {
            var parameters = new DynamicParameters();
            var whereClauses = new List<string> { """CR."ChatRoomStatusId" = 1""" };

            int currentUserId = CurrentUserId;
            parameters.Add("CurrentUserId", currentUserId);
            whereClauses.Add("""(CR."SubmittedByUserId" NOT IN (SELECT "UserBlocked" FROM "UserBlocking" WHERE "UserBlockedBy" = @CurrentUserId) AND CR."SubmittedByUserId" NOT IN (SELECT "UserBlockedBy" FROM "UserBlocking" WHERE "UserBlocked" = @CurrentUserId))""");

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

        public async Task<PagedResult<ArticleViewModel>> SearchArticlesAsync(int pageIndex, int pageSize, string? searchTerm, List<int> genreIds)
        {
            var parameters = new DynamicParameters();
            var whereClauses = new List<string>();

            whereClauses.Add("""A."IsPrivate" = FALSE""");
            whereClauses.Add("""A."CategoryId" NOT IN (7, 8, 9, 10)""");

            int currentUserId = CurrentUserId;
            parameters.Add("CurrentUserId", currentUserId);
            whereClauses.Add("""(A."OwnerUserId" NOT IN (SELECT "UserBlocked" FROM "UserBlocking" WHERE "UserBlockedBy" = @CurrentUserId) AND A."OwnerUserId" NOT IN (SELECT "UserBlockedBy" FROM "UserBlocking" WHERE "UserBlocked" = @CurrentUserId))""");

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

        public Task<IEnumerable<BadgeSelectionViewModel>> GetUserBadgesForEditingAsync(int userId)
        {
            const string sql = """
            SELECT UB."UserBadgeId", B."BadgeName", B."BadgeImageUrl", UB."IsDisplayed"
            FROM "UserBadges" UB
            JOIN "Badges" B ON UB."BadgeId" = B."BadgeId"
            WHERE UB."UserId" = @userId
            ORDER BY B."SortOrder", B."BadgeName"
            """;
            return GetRecordsAsync<BadgeSelectionViewModel>(sql, new { userId });
        }
        public Task UpsertCharacterAvatarAsync(int characterId, string avatarUrl)
        {
            const string sql = """
                                INSERT INTO "CharacterAvatars" ("CharacterId", "AvatarImageUrl", "DateCreated")
                                VALUES (@CharacterId, @AvatarUrl, NOW())
                                ON CONFLICT ("CharacterId") DO UPDATE 
                                SET "AvatarImageUrl" = EXCLUDED."AvatarImageUrl", "DateCreated" = NOW();
                                """;
            return ExecuteAsync(sql, new { CharacterId = characterId, AvatarUrl = avatarUrl });
        }
        public Task<int> AddInlineImageAsync(string imageUrl, int characterId, int userId, string inlineName)
        {
            const string sql = """
                                INSERT INTO "CharacterInlines" ("CharacterId", "UserId", "InlineImageUrl", "InlineName", "DateCreated")
                                VALUES (@CharacterId, @UserId, @ImageUrl, @InlineName, NOW())
                                RETURNING "InlineId"; 
                                """;

            return GetScalarAsync<int>(sql, new
            {
                CharacterId = characterId,
                UserId = userId,
                ImageUrl = imageUrl,
                InlineName = inlineName
            });
        }
        public Task<CharacterInline?> GetInlineImageAsync(int inlineId)
        {
            const string sql = """SELECT * FROM "CharacterInlines" WHERE "InlineId" = @inlineId""";
            return GetRecordAsync<CharacterInline>(sql, new { inlineId });
        }

        public Task DeleteInlineImageRecordAsync(int inlineId)
        {
            const string sql = """DELETE FROM "CharacterInlines" WHERE "InlineId" = @inlineId""";
            return ExecuteAsync(sql, new { inlineId });
        }
        public async Task UpdateUserBadgeDisplayAsync(int userId, List<int> displayedUserBadgeIds)
        {
            const string hideAllSql = """UPDATE "UserBadges" SET "IsDisplayed" = FALSE WHERE "UserId" = @userId""";
            await ExecuteAsync(hideAllSql, new { userId });

            if (displayedUserBadgeIds != null && displayedUserBadgeIds.Any())
            {
                const string showSelectedSql = """UPDATE "UserBadges" SET "IsDisplayed" = TRUE WHERE "UserId" = @userId AND "UserBadgeId" = ANY(@displayedUserBadgeIds)""";
                await ExecuteAsync(showSelectedSql, new { userId, displayedUserBadgeIds });
            }
        }
        public async Task<PagedResult<User>> SearchUsersAsync(string? username, int pageIndex, int pageSize, int currentUserId)
        {
            var parameters = new DynamicParameters();
            var whereClauses = new List<string>
            {
                """ "UserId" NOT IN (SELECT "UserBlocked" FROM "UserBlocking" WHERE "UserBlockedBy" = @CurrentUserId)""",
                """ "UserId" NOT IN (SELECT "UserBlockedBy" FROM "UserBlocking" WHERE "UserBlocked" = @CurrentUserId)"""
            };
            parameters.Add("CurrentUserId", currentUserId);

            if (!string.IsNullOrWhiteSpace(username))
            {
                whereClauses.Add(""" "Username" LIKE @Username """);
                parameters.Add("Username", $"%{username}%");
            }

            var whereSql = string.Join(" AND ", whereClauses);
            var fromSql = $"FROM \"Users\" WHERE {whereSql}";

            var countSql = $"SELECT COUNT(\"UserId\") {fromSql}";
            var totalCount = await GetScalarAsync<int>(countSql, parameters);

            if (totalCount == 0)
            {
                return new PagedResult<User> { Items = Enumerable.Empty<User>(), TotalCount = 0, PageIndex = pageIndex, PageSize = pageSize };
            }

            var pagingSql = $"SELECT * {fromSql} ORDER BY \"Username\" LIMIT @Take OFFSET @Skip";
            parameters.Add("Skip", (pageIndex - 1) * pageSize);
            parameters.Add("Take", pageSize);

            var items = await GetRecordsAsync<User>(pagingSql, parameters);
            return new PagedResult<User> { Items = items, TotalCount = totalCount, PageIndex = pageIndex, PageSize = pageSize };
        }

        #endregion

        #region --- Private Image URL Processing Helpers ---

        private void ProcessCharacterDetails(CharacterWithDetails? character)
        {
            if (character is null) return;

            character.AvatarImageUrl = _imageService.GetImageUrl(character.AvatarImageUrl) ?? string.Empty;
            character.DisplayImageUrl = _imageService.GetImageUrl(character.DisplayImageUrl) ?? string.Empty;
        }

        private void ProcessCharacterImages(IEnumerable<CharacterImage> images)
        {
            foreach (var image in images)
            {
                ProcessCharacterImage(image);
            }
        }

        private void ProcessCharacterImage(CharacterImage? image)
        {
            if (image is null) return;
            image.CharacterImageUrl = _imageService.GetImageUrl(image.CharacterImageUrl) ?? string.Empty;
        }

        private void ProcessImageDetails(CharacterImageWithDetails? image)
        {
            if (image is null) return;
            image.CharacterImageUrl = _imageService.GetImageUrl(image.CharacterImageUrl) ?? string.Empty;
        }

        private void ProcessImageComments(IEnumerable<ImageCommentViewModel> comments)
        {
            foreach (var comment in comments)
            {
                comment.CharacterImageUrl = _imageService.GetImageUrl(comment.CharacterImageUrl);
            }
        }

        private void ProcessStoryPosts(IEnumerable<StoryPostViewModel> posts)
        {
            foreach (var post in posts)
            {
                post.DisplayImageUrl = _imageService.GetImageUrl(post.DisplayImageUrl);
            }
        }

        private void ProcessThreadMessages(IEnumerable<ThreadMessageViewModel> messages)
        {
            foreach (var message in messages)
            {
                message.DisplayImageUrl = _imageService.GetImageUrl(message.DisplayImageUrl);
            }
        }

        // UPDATED METHOD IMPLEMENTATION
        public async Task<IEnumerable<CharactersForListing>> GetUserCharactersForListingAsync(int userId)
        {
            const string sql = """
                               SELECT
                                   c."CharacterId",
                                   c."CharacterDisplayName",
                                   'NormalCharacter' AS "CharacterNameClass",
                                   c."CardImageUrl",
                                   ca."AvatarImageUrl"
                               FROM "Characters" c
                               LEFT JOIN "CharacterAvatars" ca ON c."CharacterId" = ca."CharacterId"
                               WHERE c."UserId" = @userId
                               ORDER BY c."CharacterDisplayName";
                               """;
            var rawData = await GetRecordsAsync<dynamic>(sql, new { userId });

            return rawData.Select(d => new CharactersForListing
            {
                CharacterId = d.CharacterId,
                CharacterDisplayName = d.CharacterDisplayName,
                CharacterNameClass = d.CharacterNameClass,
                DisplayImageUrl = _imageService.GetImageUrl(d.CardImageUrl),
                AvatarImageUrl = _imageService.GetImageUrl(d.AvatarImageUrl)
            }).ToList();
        }
        public async Task<IEnumerable<DashboardItemViewModel>> GetDashboardItemsAsync(string itemType, string filter, int userId)
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
                    // FIX: S3923 - Redundant logic removed.
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
                    return Enumerable.Empty<DashboardItemViewModel>();
            }

            return await GetRecordsAsync<DashboardItemViewModel>(sql, new { userId });
        }
        public Task<IEnumerable<DashboardChatRoom>> GetDashboardChatRoomsAsync(int userId)
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
WHERE r."ChatRoomStatusId" = 2 -- <<< FIX IS HERE
AND r."SubmittedByUserId" NOT IN (SELECT "UserBlocked" FROM "UserBlocking" WHERE "UserBlockedBy" = @userId)
AND r."SubmittedByUserId" NOT IN (SELECT "UserBlockedBy" FROM "UserBlocking" WHERE "UserBlocked" = @userId)
ORDER BY lp."LastPostTime" DESC NULLS LAST, r."ChatRoomId" DESC
LIMIT 6
""";

            return GetRecordsAsync<DashboardChatRoom>(sql, new { userId });
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
        internal static class ImageProcessingHelpers
        {
            internal static void ProcessCharacterDetails(CharacterWithDetails? character, IImageService imageService)
            {
                if (character is null) return;
                character.AvatarImageUrl = imageService.GetImageUrl(character.AvatarImageUrl);
                character.DisplayImageUrl = imageService.GetImageUrl(character.CardImageUrl);
            }

            internal static CharacterImage ProcessCharacterImage(CharacterImage image, IImageService imageService)
            {
                image.CharacterImageUrl = imageService.GetImageUrl(image.CharacterImageUrl);
                return image;
            }
        }
        #endregion
    }
}