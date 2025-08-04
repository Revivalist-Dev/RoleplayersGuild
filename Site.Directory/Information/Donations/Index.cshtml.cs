using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Services;

namespace RoleplayersGuild.Site.Directory.Donations
{
    public class IndexModel : PageModel
    {
        private readonly ICookieService _cookieService;

        public int CurrentUserId { get; private set; }
        public bool IsUserLoggedIn { get; private set; }

        public IndexModel(ICookieService cookieService)
        {
            _cookieService = cookieService;
        }

        public void OnGet()
        {
            CurrentUserId = _cookieService.GetUserId();
            IsUserLoggedIn = CurrentUserId != 0;
        }
    }
}