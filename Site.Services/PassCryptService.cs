using Microsoft.AspNetCore.Identity;

namespace RoleplayersGuild.Site.Services
{
    /// <summary>
    /// Implements password hashing and verification using the standard ASP.NET Core Identity hasher.
    /// </summary>
    public class PassCryptService : IPassCryptService
    {
        private readonly IPasswordHasher<User> _passwordHasher;

        public PassCryptService(IPasswordHasher<User> passwordHasher)
        {
            // Inject the standard ASP.NET Core Identity password hasher.
            _passwordHasher = passwordHasher;
        }

        /// <summary>
        /// Hashes a password using the configured ASP.NET Core Identity hashing format.
        /// </summary>
        public string HashPassword(string password)
        {
            // The first argument can be null or a new User() because the default hasher
            // doesn't typically use user-specific data to generate the hash, though it's
            // good practice to pass a User instance if context allows.
            return _passwordHasher.HashPassword(new User(), password);
        }

        /// <summary>
        /// Verifies a password against a hash.
        /// </summary>
        public PasswordVerificationResult VerifyPassword(string hashedPassword, string providedPassword)
        {
            return _passwordHasher.VerifyHashedPassword(new User(), hashedPassword, providedPassword);
        }
    }
}