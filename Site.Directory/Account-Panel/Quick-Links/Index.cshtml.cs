using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;

// FIXED: Replaced hyphens with underscores for a valid C# namespace.
namespace RoleplayersGuild.Site.Directory.Account_Panel.Quick_Links
{
    public class IndexQuickLinksModel : PageModel
    {
        private readonly IDataService _dataService;
        private readonly ICookieService _cookieService;

        public IndexQuickLinksModel(IDataService dataService, ICookieService cookieService)
        {
            _dataService = dataService;
            _cookieService = cookieService;
        }

        public List<QuickLink> QuickLinks { get; set; } = new();

        [BindProperty]
        public NewLinkInputModel NewLink { get; set; } = new();

        [TempData]
        public string? Message { get; set; }
        [TempData]
        public string MessageType { get; set; } = "info";

        public async Task OnGetAsync()
        {
            var userId = _cookieService.GetUserId();
            if (userId != 0)
            {
                var links = await _dataService.GetUserQuickLinksAsync(userId);
                QuickLinks = links.ToList();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = _cookieService.GetUserId();
            if (userId == 0) return Forbid();

            if (!ModelState.IsValid)
            {
                await OnGetAsync(); // Reload links
                return Page();
            }

            var newLink = new QuickLink
            {
                UserId = userId,
                LinkName = NewLink.LinkName,
                LinkAddress = NewLink.LinkAddress,
                OrderNumber = NewLink.OrderNumber
            };

            await _dataService.AddQuickLinkAsync(newLink);
            Message = "Your new quick link has been added!";
            MessageType = "success";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int linkId)
        {
            var userId = _cookieService.GetUserId();
            if (userId == 0) return Forbid();

            await _dataService.DeleteQuickLinkAsync(linkId, userId);
            Message = "The quick link has been removed.";
            MessageType = "success";
            return RedirectToPage();
        }
    }

    public class NewLinkInputModel
    {
        [Required]
        [Display(Name = "Link Name")]
        [StringLength(50)]
        public string LinkName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Link Address")]
        [StringLength(255)]
        [Url]
        public string LinkAddress { get; set; } = string.Empty;

        [Display(Name = "Order")]
        public int OrderNumber { get; set; }
    }
}