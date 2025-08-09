using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using RoleplayersGuild.Site.Services.DataServices;

namespace RoleplayersGuild.Site.Directory.User_Panel.My_Threads
{
    public class CreateNewThreadModel : UserPanelBaseModel
    {
        [BindProperty]
        public NewThreadInput Input { get; set; } = new();
        public SelectList UserCharacters { get; set; } = new(Enumerable.Empty<SelectListItem>());

        public CreateNewThreadModel(
            ICharacterDataService characterDataService,
            ICommunityDataService communityDataService,
            IMiscDataService miscDataService,
            IUserService userService)
            : base(characterDataService, communityDataService, miscDataService, userService)
        {
        }

        public async Task<IActionResult> OnGetAsync([FromQuery] int toCharacterId)
        {
            if (toCharacterId > 0)
            {
                var character = await _characterDataService.GetCharacterAsync(toCharacterId);
                if (character is not null)
                {
                    Input.ToCharacterIds = character.CharacterDisplayName ?? "";
                }
            }

            await PopulateCharactersAsync(LoggedInUserId);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
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
                await PopulateCharactersAsync(LoggedInUserId);
                return Page();
            }

            var recipients = (await _characterDataService.GetCharacterAndUserIdsByDisplayNamesAsync(recipientNames)).ToList();

            var recipientDisplayNames = recipients.Select(r => r.CharacterDisplayName).ToList();
            var notFoundNames = recipientNames.Except(recipientDisplayNames, System.StringComparer.OrdinalIgnoreCase).ToList();

            if (notFoundNames.Any())
            {
                ModelState.AddModelError("Input.ToCharacterIds", $"Could not find character(s): {string.Join(", ", notFoundNames)}");
                await PopulateCharactersAsync(LoggedInUserId);
                return Page();
            }

            var threadId = await _communityDataService.CreateNewThreadAsync(Input.Title, LoggedInUserId);
            await _communityDataService.InsertMessageAsync(threadId, Input.CharacterId, Input.Content);

            await _communityDataService.InsertThreadUserAsync(LoggedInUserId, threadId, 1, Input.CharacterId, 1);

            foreach (var recipient in recipients)
            {
                await _communityDataService.InsertThreadUserAsync(recipient.UserId, threadId, 2, recipient.CharacterId, 2);
            }

            return RedirectToPage("./View", new { id = threadId });
        }

        private async Task PopulateCharactersAsync(int userId)
        {
            var characters = await _characterDataService.GetActiveCharactersForUserAsync(userId);
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
