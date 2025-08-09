// In F:\...\Site.Services\CustomUserStore.cs

using Microsoft.AspNetCore.Identity;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services.DataServices;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Services
{
    public class CustomUserStore : IUserStore<User>, IUserClaimStore<User>, IUserRoleStore<User>
    {
        private readonly IUserDataService _userDataService;

        public CustomUserStore(IUserDataService userDataService)
        {
            _userDataService = userDataService;
        }

        public Task<IList<System.Security.Claims.Claim>> GetClaimsAsync(User user, CancellationToken cancellationToken)
        {
            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, user.Username ?? string.Empty),
                new System.Security.Claims.Claim("UserTypeId", user.UserTypeId.ToString())
            };
            return Task.FromResult<IList<System.Security.Claims.Claim>>(claims);
        }

        public Task AddClaimsAsync(User user, IEnumerable<System.Security.Claims.Claim> claims, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task ReplaceClaimAsync(User user, System.Security.Claims.Claim claim, System.Security.Claims.Claim newClaim, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task RemoveClaimsAsync(User user, IEnumerable<System.Security.Claims.Claim> claims, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<IList<User>> GetUsersForClaimAsync(System.Security.Claims.Claim claim, CancellationToken cancellationToken) => throw new NotImplementedException();

        public Task AddToRoleAsync(User user, string roleName, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task RemoveFromRoleAsync(User user, string roleName, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<IList<string>> GetRolesAsync(User user, CancellationToken cancellationToken)
        {
            var roles = new List<string>();
            if (user.UserTypeId >= 2) roles.Add("Staff");
            if (user.UserTypeId >= 3) roles.Add("Admin");
            if (user.UserTypeId == 4) roles.Add("SuperAdmin");
            return Task.FromResult<IList<string>>(roles);
        }
        public Task<bool> IsInRoleAsync(User user, string roleName, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<IList<User>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken) => throw new NotImplementedException();

        public void Dispose()
        {
            // Nothing to dispose.
        }

        public Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.UserId.ToString());
        }

        public Task<string?> GetUserNameAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Username);
        }

        public Task SetUserNameAsync(User user, string? userName, CancellationToken cancellationToken)
        {
            user.Username = userName;
            return Task.CompletedTask;
        }

        public async Task<User?> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            if (int.TryParse(userId, out int id))
            {
                return await _userDataService.GetUserAsync(id);
            }
            return null;
        }

        public async Task<User?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            // Using GetUserByEmailAsync as the primary lookup, which matches your login logic.
            return await _userDataService.GetUserByEmailAsync(normalizedUserName);
        }

        // The methods below are not needed for your app to start, but are required by the interface.
        public Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<string?> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task SetNormalizedUserNameAsync(User user, string? normalizedName, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken) => throw new NotImplementedException();
    }
}
