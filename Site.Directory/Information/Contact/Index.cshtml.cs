using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Services;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.Contact
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IDataService _dataService;
        private readonly INotificationService _notificationService;
        private readonly ICookieService _cookieService;

        public IndexModel(IDataService dataService, INotificationService notificationService, ICookieService cookieService)
        {
            _dataService = dataService;
            _notificationService = notificationService;
            _cookieService = cookieService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public bool MessageSent { get; set; } = false;

        public class InputModel
        {
            [Required]
            [StringLength(200)]
            public string Title { get; set; } = string.Empty;

            [Required]
            public string Description { get; set; } = string.Empty;
        }

        public void OnGet()
        {
            // Just displays the form
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var userId = _cookieService.GetUserId();
            if (userId == 0)
            {
                return RedirectToPage("/Account/Login");
            }

            await _dataService.AddToDoItemAsync(Input.Title, Input.Description, userId);

            // CORRECTED: Using the existing SendMessageToStaffAsync method
            await _notificationService.SendMessageToStaffAsync(
                "[Staff] - " + Input.Title,
                "A new issue has been reported via the contact form: <br/><br/>" + Input.Description
            );

            MessageSent = true;
            return Page();
        }
    }
}