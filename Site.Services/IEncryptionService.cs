namespace RoleplayersGuild.Site.Services
{
    /// <summary>
    /// Defines a service for encrypting and decrypting application data.
    /// This is the modern replacement for the old TextEncrypt/TextDecrypt and Protect/Unprotect methods.
    /// </summary>
    public interface IEncryptionService
    {
        /// <summary>
        /// Encrypts a plain-text string using a purpose-specific protector.
        /// </summary>
        /// <param name="plainText">The string to encrypt.</param>
        /// <param name="purpose">A unique string that isolates the encryption context. Must be the same for encryption and decryption.</param>
        /// <returns>A protected, URL-safe string.</returns>
        string Protect(string plainText, string purpose = "DefaultPurpose");

        /// <summary>
        /// Decrypts a string that was protected with the same purpose.
        /// </summary>
        /// <param name="protectedText">The protected string to decrypt.</param>
        /// <param name="purpose">The unique string that was used to protect the data.</param>
        /// <returns>The original plain-text string, or null if decryption fails.</returns>
        string? Unprotect(string protectedText, string purpose = "DefaultPurpose");
    }
}