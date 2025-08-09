using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using RoleplayersGuild.Site.Services.DataServices;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

// Corrected: Namespace now matches folder structure
namespace RoleplayersGuild.Site.Directory.Admin_Panel.Mass_Message
{
    [Authorize(Policy = "IsAdmin")]
    public class IndexModel : PageModel
    {
        private readonly IUserDataService _userDataService;
        private readonly ICommunityDataService _communityDataService;
        private readonly IUserService _userService; // Using IUserService is better for getting current user info

        public IndexModel(IUserDataService userDataService, ICommunityDataService communityDataService, IUserService userService)
        {
            _userDataService = userDataService;
            _communityDataService = communityDataService;
            _userService = userService;
        }

        [BindProperty]
        public MessageInputModel Input { get; set; } = new();

        [TempData]
        public string? Message { get; set; }
        [TempData]
        public bool IsSuccess { get; set; }

        public void OnGet()
        {
            // The [Authorize] attribute handles authorization.
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var adminUser = await _userService.GetCurrentUserAsync();

            if (adminUser == null || adminUser.CurrentSendAsCharacter == 0)
            {
                IsSuccess = false;
                Message = "Could not find your user record or a character to send from.";
                return Page();
            }

            // Corrected: SQL query uses PascalCase
            var recipients = await _userDataService.GetAllUserRecipientsAsync();

            string messageContent = $"{Input.MessageContent}<br><br><div class=\"alert alert-info\">If you have any questions or concerns about this message, please <a href='/Information/Contact'>contact the staff</a> right away.</div>";
            
            if (string.IsNullOrEmpty(Input.Title))
            {
                ModelState.AddModelError("Input.Title", "Title cannot be empty.");
                return Page();
            }
            string threadTitle = $"[RPG] - Mass Message - {Input.Title}";

            foreach (var user in recipients)
            {
                // Each user gets their own thread
                var userId = _userService.GetUserId(User); // Make sure you have the user's ID
                var threadId = await _communityDataService.CreateNewThreadAsync(Input.Title, userId);

                // Post the message from the admin's character
                await _communityDataService.InsertMessageAsync(threadId, adminUser.CurrentSendAsCharacter, messageContent);

                // Add the recipient user to the thread
                await _communityDataService.InsertThreadUserAsync(user.UserId, threadId, 2, user.CurrentSendAsCharacter, 1);
            }

            TempData["IsSuccess"] = true;
            TempData["Message"] = "The mass message has been queued for sending.";
            return RedirectToPage("/User-Panel/My-Threads/Index");
        }
    }

    // Local ViewModels for this page
    public class MessageInputModel
    {
        [Required]
        public string? Title { get; set; }
        [Required]
        public string? MessageContent { get; set; }
    }
}
