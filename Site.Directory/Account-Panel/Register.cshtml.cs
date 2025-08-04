// In F:\...\Site.Directory\Account-Panel\Register.cshtml.cs

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging; // Added for logging
using RoleplayersGuild.Project.Configuration; // Added for settings
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using System;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.Account_Panel
{
    [ValidateAntiForgeryToken]
    public class RegisterModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly IReCaptchaService _reCaptchaService;
        private readonly IConfiguration _configuration;
        private readonly IDataService _dataService;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(
            IUserService userService,
            IReCaptchaService reCaptchaService,
            IConfiguration configuration,
            IDataService dataService,
            ILogger<RegisterModel> logger)
        {
            _userService = userService;
            _reCaptchaService = reCaptchaService;
            _configuration = configuration;
            _dataService = dataService;
            _logger = logger;
        }

        [BindProperty]
        public RegisterInputModel Input { get; set; } = new();

        [TempData]
        public string? StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(string? rsn = null, int? referralId = null)
        {
            if (await _userService.IsUserAuthenticatedAsync())
            {
                return RedirectToPage("/Dashboard/Index");
            }

            if (referralId.HasValue)
            {
                HttpContext.Session.SetInt32("ReferralId", referralId.Value);
            }

            if (rsn == "NoAccess")
            {
                StatusMessage = "alert-warning|Thank you for your interest in RPG! You've found a section that requires membership. Please use the form below to join!";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (await _userService.IsUserAuthenticatedAsync())
            {
                return RedirectToPage("/Dashboard/Index");
            }

            var reCaptchaToken = Request.Form["g-recaptcha-response"].ToString();
            var reCaptchaResult = await _reCaptchaService.ValidateAsync(reCaptchaToken);

            _logger.LogInformation("reCAPTCHA raw result - Success: {Success}, Score: {Score}, Action: {Action}, Hostname: {Hostname}",
                reCaptchaResult.Success, reCaptchaResult.Score, reCaptchaResult.Action, reCaptchaResult.Hostname);

            var expectedHostname = new Uri(_configuration["SiteBaseUrl"] ?? "").Host;
            // Validate the reCAPTCHA result using the helper method.
            if (!reCaptchaResult.IsSuccess(expectedAction: "register", expectedHostname: expectedHostname))
            {
                ModelState.AddModelError(string.Empty, "The reCAPTCHA validation failed. Please try again.");
                _logger.LogWarning("reCAPTCHA validation FAILED. Errors: {Errors}", string.Join(",", reCaptchaResult.ErrorCodes));
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userService.CreateUserAsync(Input.Username, Input.Email, Input.Password);

            if (user is null)
            {
                ModelState.AddModelError(string.Empty, "A user with that username or email already exists.");
                return Page();
            }

            // Handle referral logic
            var referralId = HttpContext.Session.GetInt32("ReferralId");
            if (referralId.HasValue && referralId.Value != user.UserId)
            {
                await _dataService.ExecuteAsync("""UPDATE "Users" SET "ReferredBy" = @ReferrerUserId WHERE "UserId" = @NewUserId""", new { ReferrerUserId = referralId.Value, NewUserId = user.UserId });

                var badgeId = _configuration.GetValue<int>("BadgeIds:Referral"); // Corrected config path
                if (badgeId > 0)
                {
                    await _dataService.ExecuteAsync("""INSERT INTO "UserBadges" ("UserId", "BadgeId", "ReasonEarned") VALUES (@UserId, @BadgeId, @Reason)""", new { UserId = referralId.Value, BadgeId = badgeId, Reason = $"Referred User {user.UserId} to the site." });
                }
                HttpContext.Session.Remove("ReferralId");
            }

            await _userService.SignInUserAsync(user);
            HttpContext.Session.SetString("NewMember", "true");
            return RedirectToPage("/Information/Welcome/Index");
        }
    }
}