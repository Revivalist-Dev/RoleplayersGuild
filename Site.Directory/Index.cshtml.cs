// In F:\Visual Studio\RoleplayersGuild\Site.Directory\Index.cshtml.cs

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using RoleplayersGuild.Site.Services.DataServices;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory
{
    public class IndexModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly IContentDataService _contentDataService;

        [BindProperty]
        public LoginInputModel Input { get; set; } = new();

        [TempData]
        public string? Message { get; set; }

        public IEnumerable<ArticleWithDetails> RecentArticles { get; private set; } = Enumerable.Empty<ArticleWithDetails>();

        public IndexModel(IUserService userService, IContentDataService contentDataService)
        {
            _userService = userService;
            _contentDataService = contentDataService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (await _userService.IsUserAuthenticatedAsync())
            {
                return RedirectToPage("/Dashboard/Index");
            }

            RecentArticles = await _contentDataService.GetRecentArticlesAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            var result = await _userService.LoginAsync(Input.Email, Input.Password);

            // --- UPDATED LOGIC ---
            if (result.IsSuccess && result.User != null)
            {
                await _userService.SignInUserAsync(result.User);

                if (result.PasswordNeedsUpgrade)
                {
                    // Legacy password was correct. Flag user to update it.
                    // The "alert-warning|" prefix sets the CSS class for the alert.
                    Message = "alert-warning|For your security, please update your password to our new, more secure format.";
                    return RedirectToPage("/Account-Panel/Password/Change-Password");
                }

                // Modern password was correct.
                return RedirectToPage("/Dashboard/Index");
            }
            else
            {
                // Login failed.
                Message = "alert-danger|" + result.ErrorMessage;
                // We must use RedirectToPage to ensure TempData is handled correctly
                return RedirectToPage();
            }
        }

        public IActionResult OnGetCharacterList(string screenStatus)
        {
            var currentUserId = _userService.GetUserId(User);
            var status = currentUserId == 0 ? $"{screenStatus}NoAuth" : screenStatus;

            return new ViewComponentResult
            {
                ViewComponentName = "CharacterListing",
                Arguments = new { screenStatus = status, recordCount = 8, displaySize = "profile-card-horizontal" }
            };
        }
    }
}
