namespace RoleplayersGuild.Site.Services
{
    public interface ICookieService
    {
        void SetEncryptedCookie(string name, string value, int expiryDays = 2, string purpose = "General-Cookie-Encryption");
        string? GetEncryptedCookie(string name, string purpose = "General-Cookie-Encryption");
        void RemoveCookie(string name);
        string? GetCookie(string name);
        void SetCookie(string name, string value, int expiryDays = 365);

        int GetUserId();
        void SetUserId(int userId);
        int GetMembershipTypeId();
        void SetMembershipTypeId(int membershipTypeId);
        int GetUserTypeId();
        void SetUserTypeId(int userTypeId);
        bool GetHideStream();
        void SetHideStream(bool hideStream);
        bool GetUserPrefersMature();
        bool IsStaff();
        bool IsAdmin();
    }
}