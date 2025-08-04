using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using RoleplayersGuild.Project.Configuration;
using RoleplayersGuild.Site.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Services
{
    public class UserService : IUserService
    {
        private readonly IDataService _dataService;
        private readonly ICookieService _cookieService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _config;
        private readonly IPassCryptService _passCryptService;
        private readonly INotificationService _notificationService;
        private readonly ImageSettings _imageSettings;

        public UserService(
            IDataService dataService,
            ICookieService cookieService,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration config,
            IPassCryptService passCryptService,
            INotificationService notificationService,
            IOptions<ImageSettings> imageSettings)
        {
            _dataService = dataService;
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

            var membershipTypeId = await _dataService.GetMembershipTypeIdAsync(userId);
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

        public bool GetUserPrefersMature(ClaimsPrincipal principal)
        {
            var claim = principal.FindFirst("PrefersMature");
            if (claim != null && bool.TryParse(claim.Value, out var prefersMature)) { return prefersMature; }
            var user = GetCurrentUserAsync().GetAwaiter().GetResult();
            return user?.ShowMatureContent ?? true;
        }

        public async Task<LoginResult> LoginAsync(string email, string password)
        {
            var ipAddress = GetCurrentIpAddress();
            var user = await _dataService.GetUserByEmailAsync(email);
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
                    return new LoginResult(true);
                case PasswordVerificationResult.SuccessRehashNeeded:
                    await SignInUserAsync(user);
                    return new LoginResult(true, passwordNeedsUpgrade: true);
                case PasswordVerificationResult.Failed:
                default:
                    await LogFailedLoginAttempt(email, ipAddress);
                    return new LoginResult(false, "The email or password you entered is incorrect.");
            }
        }

        public async Task SignInUserAsync(User user)
        {
            var context = _httpContextAccessor.HttpContext;
            if (context is null) return;
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()), new Claim(ClaimTypes.Name, user.Username ?? string.Empty), new Claim(ClaimTypes.Email, user.EmailAddress ?? string.Empty), new Claim("UserTypeId", user.UserTypeId.ToString()), new Claim("PrefersMature", user.ShowMatureContent.ToString()) };
            if (user.UserTypeId >= 2) { claims.Add(new Claim(ClaimTypes.Role, "Staff")); }
            if (user.UserTypeId >= 3) { claims.Add(new Claim(ClaimTypes.Role, "Admin")); }
            if (user.UserTypeId == 4) { claims.Add(new Claim(ClaimTypes.Role, "SuperAdmin")); }
            var claimsIdentity = new ClaimsIdentity(claims, "rpg_auth_scheme");
            var authProperties = new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddDays(14) };
            await context.SignInAsync("rpg_auth_scheme", new ClaimsPrincipal(claimsIdentity), authProperties);
            var ipAddress = GetCurrentIpAddress();
            const string logSql = """ INSERT INTO "LoginAttempts" ("AttemptedUsername", "AttemptedPassword", "IpAddress", "AttemptWasSuccessful") VALUES (@Email, '--', @Ip, TRUE); UPDATE "Users" SET "LastLogin" = NOW() WHERE "UserId" = @UserId; """;
            await _dataService.ExecuteAsync(logSql, new { Email = user.EmailAddress, Ip = ipAddress, UserId = user.UserId });
            var cookieOptions = new CookieOptions { Expires = DateTime.Now.AddDays(14), HttpOnly = true, SameSite = SameSiteMode.Lax, Secure = true };
            context.Response.Cookies.Append("UseDarkTheme", user.UseDarkTheme.ToString(), cookieOptions);
        }

        public async Task SendPasswordResetEmailAsync(string email)
        {
            var user = await _dataService.GetUserByEmailAsync(email);
            if (user is not null) { await _notificationService.SendPasswordResetEmailAsync(user); }
        }

        public async Task<IdentityResult> ResetPasswordAsync(string email, string token, string newPassword)
        {
            if (!Guid.TryParse(token, out var recoveryKey)) { return IdentityResult.Failed(new IdentityError { Description = "Invalid token format." }); }
            const string sql = """SELECT * FROM "RecoveryAttempts" WHERE "RecoveryKey" = @RecoveryKey""";
            var recoveryAttempt = await _dataService.GetRecordAsync<RecoveryAttempt>(sql, new { RecoveryKey = recoveryKey });
            if (recoveryAttempt is null || recoveryAttempt.RecoveryKeyUsed) { return IdentityResult.Failed(new IdentityError { Description = "This recovery link is invalid or has already been used." }); }
            if (recoveryAttempt.AttemptTimestamp < DateTime.Now.AddMinutes(-30)) { return IdentityResult.Failed(new IdentityError { Description = "This recovery link has expired. Please request a new one." }); }
            var user = await _dataService.GetUserAsync(recoveryAttempt.UserId);
            if (user is null || user.EmailAddress != email) { return IdentityResult.Failed(new IdentityError { Description = "Invalid user for this token." }); }
            var newPasswordHash = _passCryptService.HashPassword(newPassword);
            const string updateSql = """ UPDATE "Users" SET "Password" = @Password WHERE "UserId" = @UserId; UPDATE "RecoveryAttempts" SET "RecoveryKeyUsed" = TRUE WHERE "RecoveryAttemptId" = @AttemptId; """;
            await _dataService.ExecuteAsync(updateSql, new { Password = newPasswordHash, UserId = user.UserId, AttemptId = recoveryAttempt.RecoveryAttemptId });
            return IdentityResult.Success;
        }

        private Task LogFailedLoginAttempt(string email, string? ip)
        {
            const string sql = """INSERT INTO "LoginAttempts" ("AttemptedUsername", "AttemptedPassword", "IpAddress", "AttemptWasSuccessful") VALUES (@User, @Pass, @IP, FALSE)""";
            return _dataService.ExecuteAsync(sql, new { User = email, Pass = "REDACTED", IP = ip });
        }

        public Task<bool> CurrentUserHasActiveMembershipAsync()
        {
            var principal = _httpContextAccessor.HttpContext?.User;
            if (principal is null) return Task.FromResult(false);
            var userId = GetUserId(principal);
            if (userId == 0) return Task.FromResult(false);
            var membershipTypeId = _dataService.GetMembershipTypeIdAsync(userId).GetAwaiter().GetResult();
            return Task.FromResult(membershipTypeId > 0);
        }

        public async Task MarkCharacterForReviewAsync(int characterId, int adminUserId)
        {
            var ownerUserId = await _dataService.GetUserIdFromCharacterAsync(characterId);
            if (ownerUserId == 0) return;
            await _dataService.ExecuteAsync("""UPDATE "Characters" SET "CharacterStatusId" = 2 WHERE "CharacterId" = @CharacterId""", new { CharacterId = characterId });
            var threadId = await _dataService.CreateNewThreadAsync("[RPG] - Character Locked", 1);
            var systemCharacterId = _config.GetValue<int>("SystemSettings:SystemCharacterId");
            var messageContent = "<div class=\"ThreadAlert alert-danger\"><p>This character been locked and placed under review...</p></div>";
            await _dataService.InsertMessageAsync(threadId, systemCharacterId, messageContent);
            await _dataService.InsertThreadUserAsync(ownerUserId, threadId, 2, characterId, 1);
            var noteContent = $"AUTOMATED NOTE: Character {characterId} was locked out and marked for review.";
            await _dataService.ExecuteAsync("""INSERT INTO "UserNotes" ("UserId", "CreatedByUserId", "NoteContent") VALUES (@UserId, @CreatedBy, @Content)""", new { UserId = ownerUserId, CreatedBy = adminUserId, Content = noteContent });
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
            return Task.FromResult(principal?.IsInRole("Staff") ?? false);
        }

        public async Task<User?> CreateUserAsync(string username, string email, string password)
        {
            var existingByEmail = await _dataService.GetUserIdByEmailAsync(email);
            if (existingByEmail > 0) { return null; }
            var existingByUsername = await _dataService.GetUserIdByUsernameAsync(username);
            if (existingByUsername > 0) { return null; }
            var hashedPassword = _passCryptService.HashPassword(password);
            int newUserId = await _dataService.CreateNewUserAsync(email, hashedPassword, username);
            if (newUserId > 0) { return await _dataService.GetUserAsync(newUserId); }
            return null;
        }

        public async Task<User?> GetCurrentUserAsync()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context?.User?.Identity?.IsAuthenticated ?? false)
            {
                var userId = GetUserId(context.User);
                if (userId > 0) { return await _dataService.GetUserAsync(userId); }
            }
            return null;
        }

        public async Task<IdentityResult> ChangePasswordAsync(User user, string currentPassword, string newPassword)
        {
            var verificationResult = _passCryptService.VerifyPassword(user.Password, currentPassword);
            if (verificationResult == PasswordVerificationResult.Failed) { return IdentityResult.Failed(new IdentityError { Description = "Incorrect current password." }); }
            var newPasswordHash = _passCryptService.HashPassword(newPassword);
            await _dataService.ExecuteAsync("""UPDATE "Users" SET "Password" = @Password WHERE "UserId" = @UserId""", new { Password = newPasswordHash, UserId = user.UserId });
            return IdentityResult.Success;
        }

        // ADD THIS METHOD TO IMPLEMENT THE INTERFACE
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _dataService.GetUserByEmailAsync(email);
        }
    }
}