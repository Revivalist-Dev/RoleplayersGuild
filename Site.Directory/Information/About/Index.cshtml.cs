using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Services;

namespace RoleplayersGuild.Site.Directory.Information.About
{
    public class IndexModel : PageModel
    {
        private readonly IUserService _userService;

        public bool IsUserLoggedIn { get; private set; }
        public int CurrentUserId { get; private set; }
        public string? ReferralQuery { get; private set; }

        public IndexModel(IUserService userService)
        {
            _userService = userService;
        }

        public void OnGet()
        {
            CurrentUserId = _userService.GetUserId(User);
            IsUserLoggedIn = CurrentUserId != 0;

            if (IsUserLoggedIn)
            {
                ReferralQuery = $"?ReferralID={CurrentUserId}";
            }
        }
    }
}