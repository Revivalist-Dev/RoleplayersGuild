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
    public class ViewThreadModel : PageModel
    {
        private readonly ICommunityDataService _communityDataService;
        private readonly ICharacterDataService _characterDataService;
        private readonly IUserService _userService;

        public ViewThreadModel(
            ICommunityDataService communityDataService,
            ICharacterDataService characterDataService,
            IUserService userService)
        {
            _communityDataService = communityDataService;
            _characterDataService = characterDataService;
            _userService = userService;
        }

        public ThreadDetails Thread { get; set; } = new();
        public List<ThreadMessageViewModel> Messages { get; set; } = new();
        public List<ThreadParticipantViewModel> Participants { get; set; } = new();
        public SelectList UserCharacters { get; set; } = new(Enumerable.Empty<SelectListItem>());

        [BindProperty]
        public NewMessageInput NewMessage { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userId = _userService.GetUserId(User);
            if (userId == 0) return Forbid();

            var thread = await _communityDataService.GetThreadDetailsForUserAsync(id, userId);
            if (thread is null) return NotFound();
            Thread = thread;

            await LoadPageData(id, userId);
            await _communityDataService.MarkReadForCurrentUserAsync(id, userId);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = _userService.GetUserId(User);
            if (userId == 0) return Forbid();

            var thread = await _communityDataService.GetThreadDetailsForUserAsync(NewMessage.ThreadId, userId);
            if (thread is null) return NotFound();
            Thread = thread;

            if (!ModelState.IsValid)
            {
                await LoadPageData(NewMessage.ThreadId, userId);
                return Page();
            }

            await _communityDataService.InsertMessageAsync(NewMessage.ThreadId, NewMessage.CharacterId, NewMessage.Content);
            await _communityDataService.MarkUnreadForOthersOnThreadAsync(NewMessage.ThreadId, userId);

            return RedirectToPage(new { id = NewMessage.ThreadId });
        }

        private async Task LoadPageData(int threadId, int userId)
        {
            Messages = (await _communityDataService.GetThreadMessagesAsync(threadId)).ToList();
            Participants = (await _communityDataService.GetThreadParticipantsAsync(threadId)).ToList();
            var characters = await _characterDataService.GetActiveCharactersForUserAsync(userId);
            UserCharacters = new SelectList(characters, "CharacterId", "CharacterDisplayName");
        }
    }

    public class NewMessageInput
    {
        public int ThreadId { get; set; }
        [Required]
        public int CharacterId { get; set; }
        [Required]
        public string Content { get; set; } = string.Empty;
    }
}
