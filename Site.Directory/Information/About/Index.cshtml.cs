using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Services;

namespace RoleplayersGuild.Site.Directory.About
{
    public class IndexModel : PageModel
    {
        private readonly ICookieService _cookieService;

        public bool IsUserLoggedIn { get; private set; }
        public int CurrentUserId { get; private set; }
        public string? ReferralQuery { get; private set; }

        public IndexModel(ICookieService cookieService)
        {
            _cookieService = cookieService;
        }

        public void OnGet()
        {
            CurrentUserId = _cookieService.GetUserId();
            IsUserLoggedIn = CurrentUserId != 0;

            if (IsUserLoggedIn)
            {
                ReferralQuery = $"?ReferralID={CurrentUserId}";
            }
        }
    }
}