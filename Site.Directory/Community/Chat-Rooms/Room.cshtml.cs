using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;

namespace RoleplayersGuild.Site.Directory.Community.Chat_Rooms
{
    public class RoomModel : PageModel
    {
        private readonly IDataService _dataService;
        private readonly IUserService _userService;
        private readonly IImageService _imageService; // Add ImageService

        public RoomModel(IDataService dataService, IUserService userService, IImageService imageService) // Update constructor
        {
            _dataService = dataService;
            _userService = userService;
            _imageService = imageService; // Add ImageService
        }

        public ChatRoomWithDetails ChatRoom { get; set; } = new();
        public List<CharactersForListing> UserCharacters { get; set; } = new();
        public int CurrentSendAsCharacterId { get; set; }
        public List<ChatParticipantViewModel> Participants { get; set; } = new(); // Add this property

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var room = await _dataService.GetChatRoomWithDetailsAsync(id);
            if (room is null)
            {
                return NotFound();
            }
            ChatRoom = room;

            // Fetch participants for the left column
            var participants = await _dataService.GetChatRoomParticipantsAsync(id);
            foreach (var p in participants)
            {
                p.AvatarImageUrl = _imageService.GetImageUrl(p.AvatarImageUrl);
            }
            Participants = participants.ToList();

            var userId = _userService.GetUserId(User);
            if (userId != 0)
            {
                var characters = await _dataService.GetRecordsAsync<CharactersForListing>(
                    """
                    SELECT "CharacterId", "CharacterDisplayName"
                    FROM "CharactersForListing" 
                    WHERE "UserId" = @userId AND "CharacterStatusId" = 1
                    """,
                    new { userId });
                UserCharacters = characters.ToList();
                CurrentSendAsCharacterId = await _dataService.GetScalarAsync<int>("""SELECT "CurrentSendAsCharacter" FROM "Users" WHERE "UserId" = @userId""", new { userId });
            }

            return Page();
        }

        // The OnGetCharacterList handler is no longer needed and has been removed.
    }
}