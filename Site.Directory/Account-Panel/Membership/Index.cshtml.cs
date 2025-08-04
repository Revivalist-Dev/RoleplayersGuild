using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Services;

namespace RoleplayersGuild.Site.Directory.Account_Panel.Membership
{
    public class IndexMembershipModel : PageModel
    {
        private readonly ICookieService _cookieService;

        public int CurrentUserId { get; private set; }
        public bool IsUserLoggedIn { get; private set; }

        public IndexMembershipModel(ICookieService cookieService)
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