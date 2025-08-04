using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.Account_Panel.Password
{
    public class ChangePasswordModel : PageModel
    {
        private readonly IUserService _userService;

        public ChangePasswordModel(IUserService userService)
        {
            _userService = userService;
        }

        [BindProperty]
        public ChangePasswordInputModel Input { get; set; } = new();

        [TempData]
        public string? Message { get; set; }

        [TempData]
        public string MessageType { get; set; } = "info";

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userService.GetCurrentUserAsync();
            if (user == null)
            {
                return NotFound($"Unable to load user.");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userService.GetCurrentUserAsync();
            if (user == null)
            {
                return NotFound($"Unable to load user.");
            }

            var changePasswordResult = await _userService.ChangePasswordAsync(user, Input.CurrentPassword, Input.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                MessageType = "danger";
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return Page();
            }

            MessageType = "success";
            Message = "Your password has been changed.";
            return RedirectToPage();
        }
    }
}