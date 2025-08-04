using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.Experimental.Chat
{
    public class ChatModel : PageModel
    {
        private readonly IDataService _dataService;
        private readonly IUserService _userService;

        // The ID of the chat room, captured from the URL route.
        [FromRoute]
        public int Id { get; set; }

        public ChatRoomWithDetails? ChatRoom { get; private set; }
        public bool HideAds { get; private set; }

        public ChatModel(IDataService dataService, IUserService userService)
        {
            _dataService = dataService;
            _userService = userService;
        }

        /// <summary>
        /// OnGetAsync runs when the page is first loaded. It fetches the initial
        /// details for the chat room.
        /// </summary>
        public async Task<IActionResult> OnGetAsync()
        {
            ChatRoom = await _dataService.GetChatRoomWithDetailsAsync(Id);

            if (ChatRoom is null)
            {
                // If the chat room doesn't exist, redirect to a not-found page or the main list.
                return NotFound();
            }

            // Check if the user is a paying member to determine if ads should be hidden.
            HideAds = await _userService.CurrentUserHasActiveMembershipAsync();

            return Page();
        }
    }
}