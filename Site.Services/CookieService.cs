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

        public void SetPublicCookie(string name, string value, int expiryDays = 365)
        {
            if (CurrentContext is null) return;
            var cookieOptions = new CookieOptions
            {
                HttpOnly = false, // Accessible by client-side script
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

        public bool GetUserPrefersMature()
        {
            var cookieValue = GetEncryptedCookie("ShowMatureContent");
            return bool.TryParse(cookieValue, out bool prefersMature) && prefersMature;
        }
        #endregion
    }
}