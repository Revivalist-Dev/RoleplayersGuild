using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace RoleplayersGuild.Site.Services
{
    public class LoginResult
    {
        public bool IsSuccess { get; }
        public string? ErrorMessage { get; }
        public bool PasswordNeedsUpgrade { get; }
        public User? User { get; }

        public LoginResult(bool isSuccess, string? errorMessage = null, bool passwordNeedsUpgrade = false, User? user = null)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
            PasswordNeedsUpgrade = passwordNeedsUpgrade;
            User = user;
        }
    }

    public interface IUserService
    {
        int GetUserId(ClaimsPrincipal principal);
        Task<bool> GetUserPrefersMatureAsync(ClaimsPrincipal principal);
        Task<LoginResult> LoginAsync(string email, string password);
        Task MarkCharacterForReviewAsync(int characterId, int adminUserId);
        Task SignInUserAsync(User user);
        string? GetCurrentIpAddress();
        Task<bool> CurrentUserHasActiveMembershipAsync();
        Task SendPasswordResetEmailAsync(string email);
        Task<IdentityResult> ResetPasswordAsync(string email, string token, string newPassword);
        Task<bool> IsUserAuthenticatedAsync();
        Task<bool> IsCurrentUserStaffAsync();
        Task<User?> CreateUserAsync(string username, string email, string password);
        Task<User?> GetCurrentUserAsync();
        Task<IdentityResult> ChangePasswordAsync(User user, string currentPassword, string newPassword);
        Task<ImageLimitResults> GetImageLimitsAsync();
        Task<User?> GetUserByEmailAsync(string email);
        Task UpdateUserLastActionAsync(int userId);
    }
}
