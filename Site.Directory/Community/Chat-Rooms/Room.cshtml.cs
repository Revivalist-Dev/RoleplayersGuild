using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using RoleplayersGuild.Site.Services.DataServices;
using RoleplayersGuild.Site.Services.Models;

namespace RoleplayersGuild.Site.Directory.Community.Chat_Rooms
{
    public class RoomModel : PageModel
    {
        private readonly ICommunityDataService _communityDataService;
        private readonly IUserService _userService;
        private readonly IUrlProcessingService _urlProcessingService;

        public RoomModel(ICommunityDataService communityDataService, IUserService userService, IUrlProcessingService urlProcessingService)
        {
            _communityDataService = communityDataService;
            _userService = userService;
            _urlProcessingService = urlProcessingService;
        }

        public ChatRoomWithDetails ChatRoom { get; set; } = new();
        public List<CharactersForListing> UserCharacters { get; set; } = new();
        public int CurrentSendAsCharacterId { get; set; }
        public List<ChatParticipantViewModel> Participants { get; set; } = new(); // Add this property

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var room = await _communityDataService.GetChatRoomWithDetailsAsync(id);
            if (room is null)
            {
                return NotFound();
            }
            ChatRoom = room;

            // Fetch participants for the left column
            var participants = await _communityDataService.GetChatRoomParticipantsAsync(id);
            foreach (var p in participants)
            {
                p.AvatarImageUrl = _urlProcessingService.GetCharacterImageUrl((ImageUploadPath)p.AvatarImageUrl);
            }
            Participants = participants.ToList();

            var userId = _userService.GetUserId(User);
            if (userId != 0)
            {
                var characters = await _communityDataService.GetRecordsAsync<CharactersForListing>(
                    """
                    SELECT "CharacterId", "CharacterDisplayName"
                    FROM "CharactersForListing"
                    WHERE "UserId" = @userId AND "CharacterStatusId" = 1
                    """,
                    new { userId });
                UserCharacters = characters.ToList();
                CurrentSendAsCharacterId = await _communityDataService.GetScalarAsync<int>("""SELECT "CurrentSendAsCharacter" FROM "Users" WHERE "UserId" = @userId""", new { userId });
            }

            return Page();
        }

        // The OnGetCharacterList handler is no longer needed and has been removed.
    }
}
