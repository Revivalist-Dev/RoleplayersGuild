using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Services;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

// Corrected: Namespace now matches folder structure
namespace RoleplayersGuild.Site.Directory.Admin_Panel.Site_Funding_Goal
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
        [DataType(DataType.Currency)]
        public decimal CurrentFundingAmount { get; set; }

        [TempData]
        public string? Message { get; set; }
        [TempData]
        public bool IsSuccess { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Corrected: SQL query uses PascalCase
            CurrentFundingAmount = await _dataService.GetScalarAsync<decimal>("""SELECT "CurrentFundingAmount" FROM "GeneralSettings" """);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Corrected: SQL query uses PascalCase
            await _dataService.ExecuteAsync("""UPDATE "GeneralSettings" SET "CurrentFundingAmount" = @Amount""", new { Amount = CurrentFundingAmount });

            IsSuccess = true;
            Message = "The funding goal amount has been updated successfully.";

            return RedirectToPage();
        }
    }
}