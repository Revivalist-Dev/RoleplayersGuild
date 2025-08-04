// In F:\...\Site.Services\CustomUserStore.cs

using Microsoft.AspNetCore.Identity;
using RoleplayersGuild.Site.Model;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Services
{
    public class CustomUserStore : IUserStore<User>
    {
        private readonly IDataService _dataService;

        public CustomUserStore(IDataService dataService)
        {
            _dataService = dataService;
        }

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
                return await _dataService.GetUserAsync(id);
            }
            return null;
        }

        public async Task<User?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            // Using GetUserByEmailAsync as the primary lookup, which matches your login logic.
            return await _dataService.GetUserByEmailAsync(normalizedUserName);
        }

        // The methods below are not needed for your app to start, but are required by the interface.
        public Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<string?> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task SetNormalizedUserNameAsync(User user, string? normalizedName, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken) => throw new NotImplementedException();
    }
}