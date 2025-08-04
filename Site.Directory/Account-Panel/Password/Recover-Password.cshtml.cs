using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.Account_Panel.Password
{
    public class RecoverPasswordModel : PageModel
    {
        private readonly IUserService _userService;

        [BindProperty]
        public RecoverPasswordInputModel Input { get; set; } = new();

        [TempData]
        public string? Message { get; set; }

        public RecoverPasswordModel(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            await _userService.SendPasswordResetEmailAsync(Input.Email);

            Message = "If your email address is found in our system, you will receive a password recovery link. Please check your inbox.";

            return RedirectToPage();
        }
    }
}