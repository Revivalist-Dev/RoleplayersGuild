using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.Account_Panel.Password
{
    public class ResetPasswordModel : PageModel
    {
        private readonly IUserService _userService;

        [BindProperty]
        public ResetPasswordInputModel Input { get; set; } = new();

        public bool ResetSuccessful { get; private set; }

        public ResetPasswordModel(IUserService userService)
        {
            _userService = userService;
        }

        public IActionResult OnGet(string? email = null, string? token = null)
        {
            if (email is null || token is null)
            {
                return RedirectToPage("/Index");
            }

            Input = new ResetPasswordInputModel
            {
                Email = email,
                Token = token
            };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var result = await _userService.ResetPasswordAsync(Input.Email, Input.Token, Input.Password);

            if (result.Succeeded)
            {
                ResetSuccessful = true;
                return Page();
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return Page();
        }
    }
}