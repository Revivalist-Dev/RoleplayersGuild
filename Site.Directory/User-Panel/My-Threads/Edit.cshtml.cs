using Microsoft.AspNetCore.Mvc;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using RoleplayersGuild.Site.Services.DataServices;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.User_Panel.My_Threads
{
    public class EditModel : UserPanelBaseModel
    {
        [BindProperty]
        public EditThreadInput Input { get; set; } = new();
        public ThreadDetails Thread { get; set; } = new();
        public List<ThreadParticipantViewModel> Participants { get; set; } = new();

        public EditModel(
            ICharacterDataService characterDataService,
            ICommunityDataService communityDataService,
            IMiscDataService miscDataService,
            IUserService userService)
            : base(characterDataService, communityDataService, miscDataService, userService)
        {
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var thread = await _communityDataService.GetThreadDetailsForUserAsync(id, LoggedInUserId);
            if (thread is null) return NotFound();
            if (thread.CreatorUserId != LoggedInUserId) return Forbid();

            Thread = thread;
            Participants = (await _communityDataService.GetThreadParticipantsAsync(id)).ToList();

            Input.ThreadId = thread.ThreadId;
            Input.Title = thread.ThreadTitle;
            Input.ToCharacterIds = string.Join(", ", Participants.Select(p => p.CharacterDisplayName));

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var thread = await _communityDataService.GetThreadDetailsForUserAsync(Input.ThreadId, LoggedInUserId);
            if (thread is null) return NotFound();
            if (thread.CreatorUserId != LoggedInUserId) return Forbid();

            if (!ModelState.IsValid)
            {
                Thread = thread;
                Participants = (await _communityDataService.GetThreadParticipantsAsync(Input.ThreadId)).ToList();
                return Page();
            }

            await _communityDataService.UpdateThreadTitleAsync(Input.ThreadId, Input.Title);

            var newRecipientNames = Input.ToCharacterIds.Split(',').Select(name => name.Trim()).Where(name => !string.IsNullOrEmpty(name)).ToList();
            var newRecipients = (await _characterDataService.GetCharacterAndUserIdsByDisplayNamesAsync(newRecipientNames)).ToList();

            var currentParticipants = (await _communityDataService.GetThreadParticipantsAsync(Input.ThreadId)).ToList();

            var recipientsToAdd = newRecipients.Where(r => !currentParticipants.Any(p => p.CharacterId == r.CharacterId)).ToList();
            var participantsToRemove = currentParticipants.Where(p => !newRecipients.Any(r => r.CharacterId == p.CharacterId)).ToList();

            foreach (var recipient in recipientsToAdd)
            {
                await _communityDataService.InsertThreadUserAsync(recipient.UserId, Input.ThreadId, 2, recipient.CharacterId, 2);
            }

            foreach (var participant in participantsToRemove)
            {
                await _communityDataService.RemoveThreadCharacterAsync(participant.CharacterId, Input.ThreadId);
            }

            return RedirectToPage("./View", new { id = Input.ThreadId });
        }
    }

    public class EditThreadInput
    {
        public int ThreadId { get; set; }
        [Required]
        public string Title { get; set; } = string.Empty;
        [Display(Name = "To (Character Names)")]
        public string ToCharacterIds { get; set; } = string.Empty;
    }
}
