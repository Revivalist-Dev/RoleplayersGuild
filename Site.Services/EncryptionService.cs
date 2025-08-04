using Microsoft.AspNetCore.DataProtection;
using System.Security.Cryptography;

namespace RoleplayersGuild.Site.Services
{
    /// <summary>
    /// Implements the IEncryptionService using ASP.NET Core's Data Protection APIs.
    /// </summary>
    public class EncryptionService : IEncryptionService
    {
        private readonly IDataProtectionProvider _dataProtectionProvider;

        public EncryptionService(IDataProtectionProvider dataProtectionProvider)
        {
            _dataProtectionProvider = dataProtectionProvider;
        }

        public string Protect(string plainText, string purpose = "DefaultPurpose")
        {
            // Create a protector for a specific purpose. This prevents a token
            // generated for one feature (e.g., "password-reset") from being used for another.
            var protector = _dataProtectionProvider.CreateProtector(purpose);
            return protector.Protect(plainText);
        }

        public string? Unprotect(string protectedText, string purpose = "DefaultPurpose")
        {
            var protector = _dataProtectionProvider.CreateProtector(purpose);
            try
            {
                return protector.Unprotect(protectedText);
            }
            catch (CryptographicException)
            {
                // This will happen if the token is tampered with, expired, or invalid.
                // Returning null is a safe way to handle decryption failures.
                return null;
            }
        }
    }
}