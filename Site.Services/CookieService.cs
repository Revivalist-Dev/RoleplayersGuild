using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using System;
using System.Security.Cryptography;

namespace RoleplayersGuild.Site.Services
{
    public class CookieService : ICookieService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDataProtectionProvider _dataProtectionProvider;

        public CookieService(IHttpContextAccessor httpContextAccessor, IDataProtectionProvider dataProtectionProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            _dataProtectionProvider = dataProtectionProvider;
        }

        private HttpContext? CurrentContext => _httpContextAccessor.HttpContext;

        public string? GetCookie(string name)
        {
            return CurrentContext?.Request.Cookies[name];
        }

        public void SetCookie(string name, string value, int expiryDays = 365)
        {
            if (CurrentContext is null) return;
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Expires = DateTime.UtcNow.AddDays(expiryDays),
                SameSite = SameSiteMode.Lax
            };
            CurrentContext.Response.Cookies.Append(name, value, cookieOptions);
        }

        public void SetEncryptedCookie(string name, string value, int expiryDays = 2, string purpose = "General-Cookie-Encryption")
        {
            if (CurrentContext is null || string.IsNullOrEmpty(value)) return;
            var protector = _dataProtectionProvider.CreateProtector(purpose);
            var protectedValue = protector.Protect(value);
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Expires = DateTime.UtcNow.AddDays(expiryDays),
                SameSite = SameSiteMode.Lax
            };
            CurrentContext.Response.Cookies.Append(name, protectedValue, cookieOptions);
        }

        public string? GetEncryptedCookie(string name, string purpose = "General-Cookie-Encryption")
        {
            if (CurrentContext is null) return null;
            var cookieValue = CurrentContext.Request.Cookies[name];
            if (string.IsNullOrEmpty(cookieValue)) return null;

            try
            {
                var protector = _dataProtectionProvider.CreateProtector(purpose);
                return protector.Unprotect(cookieValue);
            }
            catch (CryptographicException)
            {
                return null;
            }
        }

        public void RemoveCookie(string name)
        {
            CurrentContext?.Response.Cookies.Delete(name);
        }

        #region --- Specific Implementations for User Properties ---

        public int GetUserId()
        {
            var cookieValue = GetEncryptedCookie("UserId");
            return int.TryParse(cookieValue, out int userId) ? userId : 0;
        }

        public void SetUserId(int userId)
        {
            SetEncryptedCookie("UserId", userId.ToString());
        }

        public int GetMembershipTypeId()
        {
            var cookieValue = GetEncryptedCookie("MembershipTypeId");
            return int.TryParse(cookieValue, out int id) ? id : 0;
        }

        public void SetMembershipTypeId(int membershipTypeId)
        {
            SetEncryptedCookie("MembershipTypeId", membershipTypeId.ToString());
        }

        public int GetUserTypeId()
        {
            var cookieValue = GetEncryptedCookie("UserTypeId");
            return int.TryParse(cookieValue, out int id) ? id : 0;
        }

        public void SetUserTypeId(int userTypeId)
        {
            SetEncryptedCookie("UserTypeId", userTypeId.ToString());
        }

        public bool GetHideStream()
        {
            var cookieValue = GetEncryptedCookie("HideStream");
            return bool.TryParse(cookieValue, out bool hideStream) && hideStream;
        }

        public void SetHideStream(bool hideStream)
        {
            SetEncryptedCookie("HideStream", hideStream.ToString());
        }

        public bool GetUserPrefersMature()
        {
            var cookieValue = GetEncryptedCookie("ShowMatureContent");
            return bool.TryParse(cookieValue, out bool prefersMature) && prefersMature;
        }

        // FIX: Implemented IsStaff() based on UserTypeId and removed old methods.
        public bool IsStaff()
        {
            var userTypeId = GetUserTypeId();
            // UserTypeIds 2 (Staff), 3 (Admin), and 4 (SuperAdmin) are staff.
            return userTypeId >= 2 && userTypeId <= 4;
        }

        public bool IsAdmin()
        {
            var userTypeId = GetUserTypeId();
            // UserTypeIds 3 (Admin) and 4 (SuperAdmin) are admins.
            return userTypeId == 3 || userTypeId == 4;
        }
        #endregion
    }
}