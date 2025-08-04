using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Services;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

// Corrected: Namespace now matches folder structure
namespace RoleplayersGuild.Site.Directory.Admin_Panel.Site_Privacy_Policy
{
    [Authorize(Policy = "IsAdmin")]
    public class IndexModel : PageModel
    {
        private readonly IDataService _dataService;

        public IndexModel(IDataService dataService)
        {
            _dataService = dataService;
        }

        [BindProperty]
        [Required(ErrorMessage = "Stop trying to save a blank page...")]
        public string PageContent { get; set; } = "";

        [TempData]
        public string? Message { get; set; }
        [TempData]
        public bool IsSuccess { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Corrected: SQL query uses PascalCase
            PageContent = await _dataService.GetScalarAsync<string>("""SELECT "PrivacyPolicyContent" FROM "GeneralSettings" """) ?? "";
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Corrected: SQL query uses PascalCase
            await _dataService.ExecuteAsync("""UPDATE "GeneralSettings" SET "PrivacyPolicyContent" = @Content""", new { Content = PageContent });

            IsSuccess = true;
            Message = "Yay, you did it. Want a cookie? Too bad, <a href=\"/Information/Legal/Terms-Of-Use\" target=\"_blank\">go make sure you did it right</a>.";

            return Page();
        }
    }
}