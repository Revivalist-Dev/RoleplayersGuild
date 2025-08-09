using Microsoft.AspNetCore.Identity;

namespace RoleplayersGuild.Site.Services
{
    /// <summary>
    /// A custom password hasher that can validate both modern ASP.NET Core Identity hashes
    /// and legacy plain-text passwords. This acts as a bridge during migration.
    /// </summary>
    public class LegacyPasswordHasher : IPasswordHasher<User>
    {
        private readonly PasswordHasher<User> _modernHasher = new();

        /// <summary>
        /// Hashes a password using the modern ASP.NET Core Identity format.
        /// </summary>
        public string HashPassword(User user, string password)
        {
            return _modernHasher.HashPassword(user, password);
        }

        /// <summary>
        /// Verifies a password against a stored hash. It first attempts to verify using the
        /// modern hashing algorithm. If that fails (e.g., because the stored hash is not
        /// in the correct format), it falls back to a simple plain-text comparison.
        /// </summary>
        /// <param name="user">The user object (not used in this implementation but required by the interface).</param>
        /// <param name="hashedPassword">The password hash stored in the database.</param>
        /// <param name="providedPassword">The password provided by the user during login.</param>
        /// <returns>A PasswordVerificationResult indicating the outcome.</returns>
        public PasswordVerificationResult VerifyHashedPassword(User user, string hashedPassword, string providedPassword)
        {
            // First, try to treat the stored password as a modern, hashed password.
            try
            {
                var modernResult = _modernHasher.VerifyHashedPassword(user, hashedPassword, providedPassword);

                // If the modern hasher succeeds, it means the password is valid and already in the new format.
                // If it fails, it could be because it's an old password, so we continue.
                if (modernResult != PasswordVerificationResult.Failed)
                {
                    return modernResult;
                }
            }
            catch (FormatException)
            {
                // This exception occurs if the `hashedPassword` from the database is not a valid
                // Base-64 string, which strongly implies it's a legacy plain-text password.
                // We can ignore this exception and proceed to the legacy check below.
            }

            // If the modern check failed or threw an exception, try a simple string comparison for the legacy password.
            if (hashedPassword == providedPassword)
            {
                // The legacy password is correct. We return SuccessRehashNeeded to signal
                // that the application should update the user's password to the new, secure hash format.
                return PasswordVerificationResult.SuccessRehashNeeded;
            }

            // If neither the modern check nor the legacy check passed, verification fails.
            return PasswordVerificationResult.Failed;
        }
    }
}
