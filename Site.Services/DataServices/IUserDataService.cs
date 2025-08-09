using RoleplayersGuild.Site.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Services.DataServices
{
    public interface IUserDataService : IBaseDataService
    {
        Task<User?> GetUserAsync(string emailAddress, string password);
        Task<User?> GetUserAsync(int userId);
        Task<int> CreateNewUserAsync(string emailAddress, string password, string username);
        Task<User?> GetUserByEmailAsync(string emailAddress);
        Task<int> GetUserIdFromCharacterAsync(int characterId);
        Task<int> GetUserIdByEmailAsync(string emailAddress);
        Task<int> GetUserIdByUsernameAsync(string username);
        Task BanUserAsync(int userId);
        Task<int> GetCurrentSendAsCharacterIdAsync();
        Task SetCurrentSendAsCharacterIdAsync(int characterId);
        Task<int> GetMembershipTypeIdAsync(int userId);
        Task<int> GetBlockRecordIdAsync(int blockeeUserId, int blockerUserId);
        Task BlockUserAsync(int blockerUserId, int blockeeUserId);
        Task UnblockUserAsync(int blockerUserId, int blockedUserId);
        Task AwardHolidayBadgeAsync(string badgeType, int badgeId, int userId);
        Task AwardBadgeIfNotExistingAsync(int badgeId, int userId);
        Task<IEnumerable<AssignableBadge>> GetAssignableBadgesAsync(int userId, int characterId);
        Task<IEnumerable<UserBadgeViewModel>> GetAvailableBadgesAsync();
        Task UpdateUserSettingsAsync(int userId, SettingsInputModel settings);
        Task<bool> IsUsernameTakenAsync(string username, int currentUserId);
        Task UpdateUserAboutMeAsync(int userId, string aboutMe);
        Task UnsubscribeUserFromNotificationsAsync(int userId);
        Task<IEnumerable<UserBadgeViewModel>> GetUserBadgesAsync(int userId);
        Task<IEnumerable<ArticleForListingViewModel>> GetUserPublicArticlesAsync(int userId);
        Task<IEnumerable<CharactersForListing>> GetUserCharactersForListingAsync(int userId);
        Task<IEnumerable<BadgeSelectionViewModel>> GetUserBadgesForEditingAsync(int userId);
        Task UpdateUserBadgeDisplayAsync(int userId, List<int> displayedUserBadgeIds);
        Task<PagedResult<User>> SearchUsersAsync(string? username, int pageIndex, int pageSize, int currentUserId);
        Task<int> GetUnreadImageCommentCountAsync(int userId);
        Task<bool> IsUserBlockedAsync(int blockedUserId, int blockerUserId);
        Task<Guid> CreatePasswordRecoveryTokenAsync(int userId);
        Task<IEnumerable<User>> GetStaffUsersAsync();
        Task<int> GetSendAsCharacterIdForUserAsync(int userId);
        Task LogSuccessfulLoginAsync(int userId, string email, string? ipAddress);
        Task<RecoveryAttempt?> GetRecoveryAttemptAsync(Guid recoveryKey);
        Task UpdatePasswordAndInvalidateTokenAsync(int userId, string newPasswordHash, int attemptId);
        Task LogFailedLoginAttemptAsync(string email, string? ipAddress);
        Task AddUserNoteAsync(int userId, int createdByUserId, string content);
        Task UpdateUserPasswordAsync(int userId, string newPasswordHash);
        Task<IEnumerable<UserRecipient>> GetAllUserRecipientsAsync();
        Task UpdateUserLastActionAsync(int userId);
        Task<int> GetOnlineUserCountAsync();
        Task<int> GetTotalUserCountAsync();
        Task ClearUserLastActionAsync(int userId);
        Task SetUserReferrerAsync(int userId, int referrerUserId);
        Task AddUserBadgeAsync(int userId, int badgeId, string reason);
    }
}
