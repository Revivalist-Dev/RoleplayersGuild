using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using RoleplayersGuild.Site.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Services.DataServices
{
    public class UserDataService : BaseDataService, IUserDataService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUrlProcessingService _urlProcessingService;

        public UserDataService(IConfiguration config, IHttpContextAccessor httpContextAccessor, IUrlProcessingService urlProcessingService) : base(config)
        {
            _httpContextAccessor = httpContextAccessor;
            _urlProcessingService = urlProcessingService;
        }

        private HttpContext? CurrentContext => _httpContextAccessor.HttpContext;
        private int CurrentUserId
        {
            get
            {
                var userIdClaim = CurrentContext?.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    return userId;
                }
                return 0;
            }
        }

        public Task<User?> GetUserAsync(string emailAddress, string password) => GetRecordAsync<User>("""SELECT * FROM "Users" WHERE "EmailAddress" = @EmailAddress AND "Password" = @Password""", new { EmailAddress = emailAddress, Password = password });
        public Task<User?> GetUserAsync(int userId) => GetRecordAsync<User>("""SELECT * FROM "Users" WHERE "UserId" = @UserId""", new { UserId = userId });
        public Task<int> CreateNewUserAsync(string emailAddress, string password, string username) => GetScalarAsync<int>("""INSERT INTO "Users" ("EmailAddress", "Password", "Username") VALUES (@EmailAddress, @Password, @Username) RETURNING "UserId";""", new { EmailAddress = emailAddress, Password = password, Username = username });
        public Task<User?> GetUserByEmailAsync(string emailAddress) => GetRecordAsync<User>("""SELECT * FROM "Users" WHERE "EmailAddress" = @EmailAddress""", new { EmailAddress = emailAddress });
        public Task<int> GetUserIdFromCharacterAsync(int characterId) => GetScalarAsync<int>("""SELECT "UserId" FROM "Characters" WHERE "CharacterId" = @CharacterId""", new { CharacterId = characterId });
        public Task<int> GetUserIdByEmailAsync(string emailAddress) => GetScalarAsync<int>("""SELECT "UserId" FROM "Users" WHERE "EmailAddress" = @EmailAddress""", new { EmailAddress = emailAddress });
        public Task<int> GetUserIdByUsernameAsync(string username) => GetScalarAsync<int>("""SELECT "UserId" FROM "Users" WHERE "Username" = @Username""", new { Username = username });
        public Task BanUserAsync(int userId) => ExecuteAsync("""UPDATE "Users" SET "Password" = 'UserBanned' || TO_CHAR(NOW(), 'YYYYMMDDHH12MISS'), "Username" = 'UserBanned' || TO_CHAR(NOW(), 'YYYYMMDDHH12MISS') WHERE "UserId" = @UserId""", new { UserId = userId });
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
        
        public Task UpdateUserSettingsAsync(int userId, SettingsInputModel settings)
        {
            return ExecuteAsync("""UPDATE "Users" SET "Username" = @Username, "EmailAddress" = @EmailAddress, "ShowWhenOnline" = @ShowWhenOnline, "ShowWriterLinkOnCharacters" = @ShowWriterLinkOnCharacters, "ReceivesThreadNotifications" = @ReceivesThreadNotifications, "ReceivesImageCommentNotifications" = @ReceivesImageCommentNotifications, "ReceivesWritingCommentNotifications" = @ReceivesWritingCommentNotifications, "ReceivesDevEmails" = @ReceivesDevEmails, "ReceivesErrorFixEmails" = @ReceivesErrorFixEmails, "ReceivesGeneralEmailBlasts" = @ReceivesGeneralEmailBlasts, "ShowMatureContent" = @ShowMatureContent, "UseDarkTheme" = @UseDarkTheme WHERE "UserId" = @UserId""",
                new { UserId = userId, settings.Username, settings.EmailAddress, settings.ShowWhenOnline, settings.ShowWriterLinkOnCharacters, settings.ReceivesThreadNotifications, settings.ReceivesImageCommentNotifications, settings.ReceivesWritingCommentNotifications, settings.ReceivesDevEmails, settings.ReceivesErrorFixEmails, settings.ReceivesGeneralEmailBlasts, settings.ShowMatureContent, settings.UseDarkTheme });
        }

        public async Task<bool> IsUsernameTakenAsync(string username, int currentUserId) => await GetScalarAsync<int>("""SELECT COUNT(1) FROM "Users" WHERE "Username" = @Username AND "UserId" <> @CurrentUserId""", new { Username = username, CurrentUserId = currentUserId }) > 0;
        public Task UpdateUserAboutMeAsync(int userId, string aboutMe) => ExecuteAsync("""UPDATE "Users" SET "AboutMe" = @AboutMe WHERE "UserId" = @UserId""", new { AboutMe = aboutMe, UserId = userId });
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
                CardImageUrl = _urlProcessingService.GetCharacterImageUrl((Models.ImageUploadPath)d.CardImageUrl),
                AvatarImageUrl = _urlProcessingService.GetCharacterImageUrl((Models.ImageUploadPath)d.AvatarImageUrl)
            }).ToList();
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

        public Task<int> GetUnreadImageCommentCountAsync(int userId) => GetScalarAsync<int>("""SELECT COUNT(CIC."ImageCommentId") FROM "CharacterImageComments" CIC JOIN "CharacterImages" CI ON CIC."ImageId" = CI."CharacterImageId" JOIN "Characters" C ON CI."CharacterId" = C."CharacterId" WHERE C."UserId" = @UserId AND CIC."IsRead" = FALSE""", new { UserId = userId });

        public async Task<bool> IsUserBlockedAsync(int blockedUserId, int blockerUserId) => await GetScalarAsync<int>("""SELECT COUNT(1) FROM "UserBlocking" WHERE "UserBlocked" = @blockedUserId AND "UserBlockedBy" = @blockerUserId""", new { blockedUserId, blockerUserId }) > 0;

        public Task<Guid> CreatePasswordRecoveryTokenAsync(int userId)
        {
            const string tokenSql = """INSERT INTO "RecoveryAttempts" ("UserId") VALUES (@UserId) RETURNING "RecoveryKey";""";
            return GetScalarAsync<Guid>(tokenSql, new { UserId = userId });
        }

        public Task<IEnumerable<User>> GetStaffUsersAsync() => GetRecordsAsync<User>("""SELECT * FROM "Users" WHERE "UserTypeId" IN (2, 3, 4)""");

        public Task<int> GetSendAsCharacterIdForUserAsync(int userId) => GetScalarAsync<int>("""SELECT "CurrentSendAsCharacter" FROM "Users" WHERE "UserId" = @UserId""", new { UserId = userId });

        public Task LogSuccessfulLoginAsync(int userId, string email, string? ipAddress)
        {
            const string logSql = """
                INSERT INTO "LoginAttempts" ("AttemptedUsername", "AttemptedPassword", "IpAddress", "AttemptWasSuccessful") 
                VALUES (@Email, '--', @Ip, TRUE); 
                UPDATE "Users" SET "LastLogin" = NOW() WHERE "UserId" = @UserId;
                """;
            return ExecuteAsync(logSql, new { Email = email, Ip = ipAddress, UserId = userId });
        }

        public Task<RecoveryAttempt?> GetRecoveryAttemptAsync(Guid recoveryKey)
        {
            const string sql = """SELECT * FROM "RecoveryAttempts" WHERE "RecoveryKey" = @RecoveryKey""";
            return GetRecordAsync<RecoveryAttempt>(sql, new { RecoveryKey = recoveryKey });
        }

        public Task UpdatePasswordAndInvalidateTokenAsync(int userId, string newPasswordHash, int attemptId)
        {
            const string updateSql = """
                UPDATE "Users" SET "Password" = @Password WHERE "UserId" = @UserId; 
                UPDATE "RecoveryAttempts" SET "RecoveryKeyUsed" = TRUE WHERE "RecoveryAttemptId" = @AttemptId;
                """;
            return ExecuteAsync(updateSql, new { Password = newPasswordHash, UserId = userId, AttemptId = attemptId });
        }

        public Task LogFailedLoginAttemptAsync(string email, string? ipAddress)
        {
            const string sql = """
                INSERT INTO "LoginAttempts" ("AttemptedUsername", "AttemptedPassword", "IpAddress", "AttemptWasSuccessful") 
                VALUES (@User, @Pass, @IP, FALSE)
                """;
            return ExecuteAsync(sql, new { User = email, Pass = "REDACTED", IP = ipAddress });
        }

        public Task AddUserNoteAsync(int userId, int createdByUserId, string content)
        {
            return ExecuteAsync("""INSERT INTO "UserNotes" ("UserId", "CreatedByUserId", "NoteContent") VALUES (@UserId, @CreatedBy, @Content)""", 
                new { UserId = userId, CreatedBy = createdByUserId, Content = content });
        }

        public Task UpdateUserPasswordAsync(int userId, string newPasswordHash)
        {
            return ExecuteAsync("""UPDATE "Users" SET "Password" = @Password WHERE "UserId" = @UserId""", 
                new { Password = newPasswordHash, UserId = userId });
        }

        public Task<IEnumerable<UserRecipient>> GetAllUserRecipientsAsync()
        {
            const string sql = """
                SELECT "UserId", "Username", "CurrentSendAsCharacter"
                FROM "Users"
                WHERE "IsActive" = TRUE
                """;
            return GetRecordsAsync<UserRecipient>(sql);
        }

        public Task UpdateUserLastActionAsync(int userId)
        {
            return ExecuteAsync("""UPDATE "Users" SET "LastAction" = NOW() WHERE "UserId" = @UserId""", new { UserId = userId });
        }

        public Task<int> GetOnlineUserCountAsync()
        {
            return GetScalarAsync<int>("""SELECT COUNT("UserId") FROM "Users" WHERE "LastAction" >= NOW() - interval '15 minutes'""");
        }

        public Task<int> GetTotalUserCountAsync()
        {
            return GetScalarAsync<int>("""SELECT COUNT(*) FROM "Users" """);
        }

        public Task ClearUserLastActionAsync(int userId)
        {
            return ExecuteAsync("""UPDATE "Users" SET "LastAction" = NULL WHERE "UserId" = @UserId""", new { UserId = userId });
        }

        public Task SetUserReferrerAsync(int userId, int referrerUserId)
        {
            return ExecuteAsync("""UPDATE "Users" SET "ReferredBy" = @ReferrerUserId WHERE "UserId" = @NewUserId""", new { ReferrerUserId = referrerUserId, NewUserId = userId });
        }

        public Task AddUserBadgeAsync(int userId, int badgeId, string reason)
        {
            return ExecuteAsync("""INSERT INTO "UserBadges" ("UserId", "BadgeId", "ReasonEarned") VALUES (@UserId, @BadgeId, @Reason)""", new { UserId = userId, BadgeId = badgeId, Reason = reason });
        }
    }
}
