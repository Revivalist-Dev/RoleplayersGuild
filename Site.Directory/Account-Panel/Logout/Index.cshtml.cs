using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Services;
using RoleplayersGuild.Site.Services.DataServices;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.Account_Panel.Logout
{
    public class IndexLogoutModel : PageModel
    {
        private readonly IUserService _userService;

        public IndexLogoutModel(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = _userService.GetUserId(User);
            if (userId != 0)
            {
                await _userService.UpdateUserLastActionAsync(userId);
            }

            await HttpContext.SignOutAsync("rpg_auth_scheme");

            HttpContext.Session.Clear();

            return RedirectToPage("/Index");
        }
    }
}
