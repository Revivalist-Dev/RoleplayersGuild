using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Services;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.Account_Panel.Logout
{
    public class IndexLogoutModel : PageModel
    {
        private readonly ICookieService _cookieService;
        private readonly IDataService _dataService;

        public IndexLogoutModel(ICookieService cookieService, IDataService dataService)
        {
            _cookieService = cookieService;
            _dataService = dataService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = _cookieService.GetUserId();
            if (userId != 0)
            {
                // FIXED: Added double quotes for PostgreSQL case-sensitivity
                await _dataService.ExecuteAsync(
                    """UPDATE "Users" SET "LastAction" = NULL WHERE "UserId" = @UserId""",
                    new { UserId = userId });
            }

            await HttpContext.SignOutAsync("rpg_auth_scheme");

            HttpContext.Session.Clear();

            _cookieService.RemoveCookie("UserId");
            _cookieService.RemoveCookie("UserTypeId");
            _cookieService.RemoveCookie("HideStream");
            _cookieService.RemoveCookie("IsStaff");
            _cookieService.RemoveCookie("MembershipTypeId");

            return RedirectToPage("/Index");
        }
    }
}