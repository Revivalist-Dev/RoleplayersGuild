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
    public class ViewThreadModel : PageModel
    {
        private readonly IDataService _dataService;
        // ADDED: Inject the modern user service
        private readonly IUserService _userService;

        // UPDATED: The constructor now accepts IUserService
        public ViewThreadModel(IDataService dataService, IUserService userService)
        {
            _dataService = dataService;
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
            // UPDATED: Use the modern service to get the user ID from the login session
            var userId = _userService.GetUserId(User);
            if (userId == 0) return Forbid();

            var thread = await _dataService.GetThreadDetailsForUserAsync(id, userId);
            if (thread is null) return NotFound();
            Thread = thread;

            await LoadPageData(id, userId);
            await _dataService.MarkReadForCurrentUserAsync(id, userId);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // UPDATED: Use the modern service here as well for consistency
            var userId = _userService.GetUserId(User);
            if (userId == 0) return Forbid();

            // The 'Thread' property is not automatically repopulated on POST.
            // We need to fetch it again before calling LoadPageData.
            var thread = await _dataService.GetThreadDetailsForUserAsync(NewMessage.ThreadId, userId);
            if (thread is null) return NotFound();
            Thread = thread;

            if (!ModelState.IsValid)
            {
                await LoadPageData(NewMessage.ThreadId, userId);
                return Page();
            }

            await _dataService.InsertMessageAsync(NewMessage.ThreadId, NewMessage.CharacterId, NewMessage.Content);
            await _dataService.MarkUnreadForOthersOnThreadAsync(NewMessage.ThreadId, userId);

            return RedirectToPage(new { id = NewMessage.ThreadId });
        }

        private async Task LoadPageData(int threadId, int userId)
        {
            Messages = (await _dataService.GetThreadMessagesAsync(threadId)).ToList();
            Participants = (await _dataService.GetThreadParticipantsAsync(threadId)).ToList();
            var characters = await _dataService.GetActiveCharactersForUserAsync(userId);
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