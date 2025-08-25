using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using RoleplayersGuild.Site.Services.DataServices;
using System.Security.Claims;

namespace RoleplayersGuild.Site.Services
{
    public class UserService : IUserService
    {
        private readonly IUserDataService _userDataService;
        private readonly ICharacterDataService _characterDataService;
        private readonly ICommunityDataService _communityDataService;
        private readonly ICookieService _cookieService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _config;
        private readonly IPassCryptService _passCryptService;
        private readonly INotificationService _notificationService;
        private readonly ImageSettings _imageSettings;

        public UserService(
            IUserDataService userDataService,
            ICharacterDataService characterDataService,
            ICommunityDataService communityDataService,
            ICookieService cookieService,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration config,
            IPassCryptService passCryptService,
            INotificationService notificationService,
            IOptions<ImageSettings> imageSettings)
        {
            _userDataService = userDataService;
            _characterDataService = characterDataService;
            _communityDataService = communityDataService;
            _cookieService = cookieService;
            _httpContextAccessor = httpContextAccessor;
            _config = config;
            _passCryptService = passCryptService;
            _notificationService = notificationService;
            _imageSettings = imageSettings.Value;
        }

        public async Task<ImageLimitResults> GetImageLimitsAsync()
        {
            var userId = GetUserId(_httpContextAccessor.HttpContext?.User ?? new ClaimsPrincipal());
            if (userId == 0)
            {
                return new ImageLimitResults { MaxSlots = _imageSettings.MaxPerCharacter, MaxSizeMb = _imageSettings.MaxFileSizeMb };
            }

            var membershipTypeId = await _userDataService.GetMembershipTypeIdAsync(userId);
            int maxSlots = membershipTypeId switch { 1 => _imageSettings.BronzeMemberMax, 2 => _imageSettings.SilverMemberMax, 3 => _imageSettings.GoldMemberMax, 4 => _imageSettings.PlatinumMemberMax, _ => _imageSettings.MaxPerCharacter };
            int maxSizeMb = membershipTypeId switch { 1 => _imageSettings.BronzeMaxFileSizeMb, 2 => _imageSettings.SilverMaxFileSizeMb, 3 => _imageSettings.GoldMaxFileSizeMb, 4 => _imageSettings.PlatinumMaxFileSizeMb, _ => _imageSettings.MaxFileSizeMb };
            return new ImageLimitResults { MaxSlots = maxSlots, MaxSizeMb = maxSizeMb };
        }

        public int GetUserId(ClaimsPrincipal principal)
        {
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId)) { return userId; }
            return 0;
        }

        public async Task<bool> GetUserPrefersMatureAsync(ClaimsPrincipal principal)
        {
            var claim = principal.FindFirst("PrefersMature");
            if (claim != null && bool.TryParse(claim.Value, out var prefersMature))
            {
                return prefersMature;
            }
            var user = await GetCurrentUserAsync();
            return user?.ShowMatureContent ?? true;
        }

        public async Task<LoginResult> LoginAsync(string email, string password)
        {
            var ipAddress = GetCurrentIpAddress();
            var user = await _userDataService.GetUserByEmailAsync(email);
            if (user is null)
            {
                await LogFailedLoginAttempt(email, ipAddress);
                return new LoginResult(false, "The email or password you entered is incorrect.");
            }
            var verificationResult = _passCryptService.VerifyPassword(user.Password, password);
            switch (verificationResult)
            {
                case PasswordVerificationResult.Success:
                    await SignInUserAsync(user);
                    return new LoginResult(true, user: user);
                case PasswordVerificationResult.SuccessRehashNeeded:
                    await SignInUserAsync(user);
                    return new LoginResult(true, passwordNeedsUpgrade: true, user: user);
                // FIX: Removed redundant 'Failed' case that falls through to default.
                default:
                    await LogFailedLoginAttempt(email, ipAddress);
                    return new LoginResult(false, "The email or password you entered is incorrect.");
            }
        }

        public async Task SignInUserAsync(User user)
        {
            var context = _httpContextAccessor.HttpContext;
            if (context is null) return;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username ?? string.Empty),
                new Claim("UserTypeId", user.UserTypeId.ToString())
            };

            switch (user.UserTypeId)
            {
                case 4: // SuperAdmin
                    claims.Add(new Claim(ClaimTypes.Role, "SuperAdmin"));
                    claims.Add(new Claim(ClaimTypes.Role, "Admin"));
                    claims.Add(new Claim(ClaimTypes.Role, "Staff"));
                    break;
                case 3: // Admin
                    claims.Add(new Claim(ClaimTypes.Role, "Admin"));
                    claims.Add(new Claim(ClaimTypes.Role, "Staff"));
                    break;
                case 2: // Staff
                    claims.Add(new Claim(ClaimTypes.Role, "Staff"));
                    break;
            }

            var claimsIdentity = new ClaimsIdentity(claims, "rpg_auth_scheme", ClaimTypes.Name, ClaimTypes.Role);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(14)
            };

            await context.SignInAsync("rpg_auth_scheme", new ClaimsPrincipal(claimsIdentity), authProperties);

            var ipAddress = GetCurrentIpAddress();
            await _userDataService.LogSuccessfulLoginAsync(user.UserId, user.EmailAddress ?? string.Empty, ipAddress);
            _cookieService.SetCookie("UseDarkTheme", user.UseDarkTheme.ToString(), 14);

            // Set the subscriber status cookie for Google Funding Choices
            var membershipTypeId = await _userDataService.GetMembershipTypeIdAsync(user.UserId);
            if (membershipTypeId > 0)
            {
                _cookieService.SetPublicCookie("subscriber-status", "true", 365);
            }
        }

        public async Task SendPasswordResetEmailAsync(string email)
        {
            var user = await _userDataService.GetUserByEmailAsync(email);
            if (user is not null) { await _notificationService.SendPasswordResetEmailAsync(user); }
        }

        public async Task<IdentityResult> ResetPasswordAsync(string email, string token, string newPassword)
        {
            if (!Guid.TryParse(token, out var recoveryKey)) { return IdentityResult.Failed(new IdentityError { Description = "Invalid token format." }); }
            var recoveryAttempt = await _userDataService.GetRecoveryAttemptAsync(recoveryKey);
            if (recoveryAttempt is null || recoveryAttempt.RecoveryKeyUsed) { return IdentityResult.Failed(new IdentityError { Description = "This recovery link is invalid or has already been used." }); }
            if (recoveryAttempt.AttemptTimestamp < DateTime.Now.AddMinutes(-30)) { return IdentityResult.Failed(new IdentityError { Description = "This recovery link has expired. Please request a new one." }); }
            var user = await _userDataService.GetUserAsync(recoveryAttempt.UserId);
            if (user is null || user.EmailAddress != email) { return IdentityResult.Failed(new IdentityError { Description = "Invalid user for this token." }); }
            var newPasswordHash = _passCryptService.HashPassword(newPassword);
            await _userDataService.UpdatePasswordAndInvalidateTokenAsync(user.UserId, newPasswordHash, recoveryAttempt.RecoveryAttemptId);
            return IdentityResult.Success;
        }

        private Task LogFailedLoginAttempt(string email, string? ip)
        {
            return _userDataService.LogFailedLoginAttemptAsync(email, ip);
        }

        public async Task<bool> CurrentUserHasActiveMembershipAsync()
        {
            var principal = _httpContextAccessor.HttpContext?.User;
            if (principal is null) return false;
            var userId = GetUserId(principal);
            if (userId == 0) return false;
            var membershipTypeId = await _userDataService.GetMembershipTypeIdAsync(userId);
            return membershipTypeId > 0;
        }

        public async Task MarkCharacterForReviewAsync(int characterId, int adminUserId)
        {
            var character = await _characterDataService.GetCharacterAsync(characterId);
            if (character is null) return;
            var ownerUserId = character.UserId;
            await _characterDataService.SetCharacterStatusAsync(characterId, 2); // 2 = Under Review
            var threadId = await _communityDataService.CreateNewThreadAsync("[RPG] - Character Locked", 1);
            var systemCharacterId = _config.GetValue<int>("SystemSettings:SystemCharacterId");
            var messageContent = "<div class=\"ThreadAlert alert-danger\"><p>This character been locked and placed under review...</p></div>";
            await _communityDataService.InsertMessageAsync(threadId, systemCharacterId, messageContent);
            await _communityDataService.InsertThreadUserAsync(character.UserId, threadId, 2, characterId, 1);
            var noteContent = $"AUTOMATED NOTE: Character {characterId} was locked out and marked for review.";
            await _userDataService.AddUserNoteAsync(character.UserId, adminUserId, noteContent);
        }

        public string? GetCurrentIpAddress()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context is null) return null;
            var ipAddress = context.Connection.RemoteIpAddress?.ToString();
            if (context.Request.Headers.TryGetValue("X-Forwarded-for", out var forwardedFor)) { return forwardedFor.ToString().Split(',').FirstOrDefault(); }
            return ipAddress;
        }

        public Task<bool> IsUserAuthenticatedAsync()
        {
            var principal = _httpContextAccessor.HttpContext?.User;
            return Task.FromResult(principal?.Identity?.IsAuthenticated ?? false);
        }

        public Task<bool> IsCurrentUserStaffAsync()
        {
            var principal = _httpContextAccessor.HttpContext?.User;
            if (principal is null) return Task.FromResult(false);
            var userTypeClaim = principal.FindFirst("UserTypeId");
            if (userTypeClaim is null || !int.TryParse(userTypeClaim.Value, out int userTypeId)) return Task.FromResult(false);
            return Task.FromResult(userTypeId >= 2);
        }

        public async Task<User?> CreateUserAsync(string username, string email, string password)
        {
            var existingByEmail = await _userDataService.GetUserIdByEmailAsync(email);
            if (existingByEmail > 0) { return null; }
            var existingByUsername = await _userDataService.GetUserIdByUsernameAsync(username);
            if (existingByUsername > 0) { return null; }
            var hashedPassword = _passCryptService.HashPassword(password);
            int newUserId = await _userDataService.CreateNewUserAsync(email, hashedPassword, username);
            if (newUserId > 0) { return await _userDataService.GetUserAsync(newUserId); }
            return null;
        }

        public async Task<User?> GetCurrentUserAsync()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context?.User?.Identity?.IsAuthenticated ?? false)
            {
                var userId = GetUserId(context.User);
                if (userId > 0) { return await _userDataService.GetUserAsync(userId); }
            }
            return null;
        }

        public async Task<IdentityResult> ChangePasswordAsync(User user, string currentPassword, string newPassword)
        {
            var verificationResult = _passCryptService.VerifyPassword(user.Password, currentPassword);
            if (verificationResult == PasswordVerificationResult.Failed) { return IdentityResult.Failed(new IdentityError { Description = "Incorrect current password." }); }
            var newPasswordHash = _passCryptService.HashPassword(newPassword);
            await _userDataService.UpdateUserPasswordAsync(user.UserId, newPasswordHash);
            return IdentityResult.Success;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _userDataService.GetUserByEmailAsync(email);
        }

        public async Task UpdateUserLastActionAsync(int userId)
        {
            await _userDataService.UpdateUserLastActionAsync(userId);
        }
    }
}
