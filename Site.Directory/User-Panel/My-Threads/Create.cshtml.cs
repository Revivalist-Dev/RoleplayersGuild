using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;

namespace RoleplayersGuild.Site.Directory.User_Panel.My_Threads
{
    public class CreateNewThreadModel : UserPanelBaseModel
    {
        [BindProperty]
        public NewThreadInput Input { get; set; } = new();
        public SelectList UserCharacters { get; set; } = new(Enumerable.Empty<SelectListItem>());

        // CORRECTED: Constructor now uses the base class and IUserService
        public CreateNewThreadModel(IDataService dataService, IUserService userService)
            : base(dataService, userService) { }

        public async Task<IActionResult> OnGetAsync([FromQuery] int toCharacterId)
        {
            var userId = UserService.GetUserId(User);
            if (userId == 0) return Forbid();

            if (toCharacterId > 0)
            {
                var character = await DataService.GetCharacterAsync(toCharacterId);
                if (character is not null)
                {
                    Input.ToCharacterIds = character.CharacterDisplayName ?? "";
                }
            }

            await PopulateCharactersAsync(userId);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = UserService.GetUserId(User);
            if (userId == 0) return Forbid();

            // Split and clean the recipient names from the input string
            var recipientNames = Input.ToCharacterIds.Split(',')
                .Select(name => name.Trim())
                .Where(name => !string.IsNullOrEmpty(name))
                .ToList();

            if (!recipientNames.Any())
            {
                ModelState.AddModelError("Input.ToCharacterIds", "You must specify at least one recipient.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateCharactersAsync(userId);
                return Page();
            }

            // Find the character and user IDs for the recipients
            var recipients = (await DataService.GetCharacterAndUserIdsByDisplayNamesAsync(recipientNames)).ToList();

            // Check if all names were found
            if (recipients.Count != recipientNames.Count)
            {
                var foundNames = recipients.Select(r => r.CharacterId.ToString()).ToHashSet();
                var notFoundNames = recipientNames.Where(name => !recipients.Any(r =>
                    DataService.GetCharacterAsync(r.CharacterId).Result?.CharacterDisplayName?.Equals(name, System.StringComparison.OrdinalIgnoreCase) ?? false));
                ModelState.AddModelError("Input.ToCharacterIds", $"Could not find character(s): {string.Join(", ", notFoundNames)}");
                await PopulateCharactersAsync(userId);
                return Page();
            }

            // Create the thread and the first message
            var threadId = await DataService.CreateNewThreadAsync(Input.Title, userId);
            await DataService.InsertMessageAsync(threadId, Input.CharacterId, Input.Content);

            // Add the sender to the thread
            await DataService.InsertThreadUserAsync(userId, threadId, 1, Input.CharacterId, 1); // 1 = Read, 1 = Owner

            // Add all recipients to the thread
            foreach (var recipient in recipients)
            {
                // 2 = Unread, 2 = Participant
                await DataService.InsertThreadUserAsync(recipient.UserId, threadId, 2, recipient.CharacterId, 2);
            }

            return RedirectToPage("./View", new { id = threadId });
        }

        private async Task PopulateCharactersAsync(int userId)
        {
            var characters = await DataService.GetActiveCharactersForUserAsync(userId);
            UserCharacters = new SelectList(characters, "CharacterId", "CharacterDisplayName");
        }
    }

    public class NewThreadInput
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        [Required]
        [Display(Name = "To (Character Names)")]
        public string ToCharacterIds { get; set; } = string.Empty;
        [Required(ErrorMessage = "You must select a character to send as.")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a character to send as.")]
        [Display(Name = "Send As")]
        public int CharacterId { get; set; }
        [Required]
        public string Content { get; set; } = string.Empty;
    }
}