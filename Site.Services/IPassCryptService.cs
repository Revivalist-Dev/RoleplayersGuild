using Microsoft.AspNetCore.Identity; // Required for PasswordVerificationResult

namespace RoleplayersGuild.Site.Services
{
    /// <summary>
    /// Defines a service for hashing and verifying passwords using the
    /// modern ASP.NET Core Identity standards, with support for legacy hashes.
    /// </summary>
    public interface IPassCryptService
    {
        /// <summary>
        /// Creates a secure hash from a user's password using the modern format.
        /// </summary>
        /// <param name="password">The plain-text password to hash.</param>
        /// <returns>A securely hashed password string ready for database storage.</returns>
        string HashPassword(string password);

        /// <summary>
        /// Verifies that a provided password matches a stored hash (either legacy or modern).
        /// </summary>
        /// <param name="hashedPassword">The password hash stored in the database.</param>
        /// <param name="providedPassword">The plain-text password provided by the user during login.</param>
        /// <returns>A PasswordVerificationResult indicating success, failure, or success with a rehash needed.</returns>
        PasswordVerificationResult VerifyPassword(string hashedPassword, string providedPassword);
    }
}