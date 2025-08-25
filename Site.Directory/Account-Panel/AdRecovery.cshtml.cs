using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Services;

namespace RoleplayersGuild.Site.Directory.Account_Panel
{
    public class AdRecoveryModel : PageModel
    {
        private readonly IUserService _userService;

        public AdRecoveryModel(IUserService userService)
        {
            _userService = userService;
        }

        public bool IsLoggedIn { get; private set; }

        public void OnGet()
        {
            IsLoggedIn = _userService.GetUserId(User) != 0;
        }
    }
}