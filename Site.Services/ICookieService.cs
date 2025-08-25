namespace RoleplayersGuild.Site.Services
{
    public interface ICookieService
    {
        void SetEncryptedCookie(string name, string value, int expiryDays = 2, string purpose = "General-Cookie-Encryption");
        string? GetEncryptedCookie(string name, string purpose = "General-Cookie-Encryption");
        void RemoveCookie(string name);
        string? GetCookie(string name);
        void SetCookie(string name, string value, int expiryDays = 365);
        void SetPublicCookie(string name, string value, int expiryDays = 365);

        bool GetUserPrefersMature();
    }
}