using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;

namespace RoleplayersGuild.Site.Directory.Account_Panel.Settings
{
    public class IndexModel : PageModel
    {
        private readonly IDataService _dataService;
        private readonly ICookieService _cookieService;
        // ADDED: Inject the modern user service
        private readonly IUserService _userService;

        [BindProperty]
        public SettingsInputModel Input { get; set; } = new();

        [TempData]
        public string? Message { get; set; }
        public bool IsSubscribed { get; private set; }

        // UPDATED: The constructor now accepts IUserService
        public IndexModel(IDataService dataService, ICookieService cookieService, IUserService userService)
        {
            _dataService = dataService;
            _cookieService = cookieService;
            _userService = userService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // UPDATED: Use the modern service to get the user ID from the login session (ClaimsPrincipal)
            var userId = _userService.GetUserId(User);
            if (userId == 0) return RedirectToPage("/Index");

            var user = await _dataService.GetUserAsync(userId);
            if (user == null) return Forbid();

            Input = new SettingsInputModel
            {
                Username = user.Username ?? "",
                EmailAddress = user.EmailAddress ?? "",
                ShowWhenOnline = user.ShowWhenOnline,
                ShowWriterLinkOnCharacters = user.ShowWriterLinkOnCharacters,
                ReceivesThreadNotifications = user.ReceivesThreadNotifications,
                ReceivesImageCommentNotifications = user.ReceivesImageCommentNotifications,
                ReceivesWritingCommentNotifications = user.ReceivesWritingCommentNotifications,
                ReceivesDevEmails = user.ReceivesDevEmails,
                ReceivesErrorFixEmails = user.ReceivesErrorFixEmails,
                ReceivesGeneralEmailBlasts = user.ReceivesGeneralEmailBlasts,
                ShowMatureContent = user.ShowMatureContent,
                UseDarkTheme = user.UseDarkTheme
            };

            IsSubscribed = await _dataService.GetMembershipTypeIdAsync(userId) > 0;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // UPDATED: Use the modern service here as well for consistency and security
            var userId = _userService.GetUserId(User);
            if (userId == 0) return Forbid();

            if (!ModelState.IsValid)
            {
                IsSubscribed = await _dataService.GetMembershipTypeIdAsync(userId) > 0;
                return Page();
            }

            if (await _dataService.IsUsernameTakenAsync(Input.Username, userId))
            {
                ModelState.AddModelError("Input.Username", "This username is already taken.");
                IsSubscribed = await _dataService.GetMembershipTypeIdAsync(userId) > 0;
                return Page();
            }

            await _dataService.UpdateUserSettingsAsync(userId, Input);

            // Set cookies to reflect choices immediately across the site
            _cookieService.SetCookie("UseDarkTheme", Input.UseDarkTheme.ToString(), 365);
            _cookieService.SetCookie("ShowMatureContent", Input.ShowMatureContent.ToString(), 365);

            Message = "Your settings have been successfully saved!";
            return RedirectToPage();
        }
    }
}