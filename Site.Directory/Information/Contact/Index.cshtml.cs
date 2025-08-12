using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Services;
using RoleplayersGuild.Site.Services.DataServices;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.Information.Contact
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IMiscDataService _miscDataService;
        private readonly INotificationService _notificationService;
        private readonly IUserService _userService;

        public IndexModel(IMiscDataService miscDataService, INotificationService notificationService, IUserService userService)
        {
            _miscDataService = miscDataService;
            _notificationService = notificationService;
            _userService = userService;
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

            var userId = _userService.GetUserId(User);
            if (userId == 0)
            {
                return RedirectToPage("/Account/Login");
            }

            await _miscDataService.AddToDoItemAsync(Input.Title, Input.Description, userId);

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
