using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Threading.Tasks;
using RoleplayersGuild.Site.Services;
using RoleplayersGuild.Site.Model;
using System.Linq;
using System;

namespace RoleplayersGuild.Site.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IDataService _dataService;
        private readonly IImageService _imageService;

        public ChatHub(IDataService dataService, IImageService imageService)
        {
            _dataService = dataService;
            _imageService = imageService;
        }

        public async Task SendMessage(int chatRoomId, int characterId, string messageContent)
        {
            var currentUserId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(currentUserId, out int userId) || userId == 0)
            {
                await Clients.Caller.SendAsync("ReceiveError", "You must be logged in to chat.");
                return;
            }

            if (string.IsNullOrWhiteSpace(messageContent) || chatRoomId <= 0 || characterId <= 0)
            {
                await Clients.Caller.SendAsync("ReceiveError", "Message content or character/room ID is invalid.");
                return;
            }

            // CORRECTED: The SQL query now provides a default value for "CharacterNameClass"
            // because this column does not exist on the base "Characters" table.
            var character = (await _dataService.GetRecordsAsync<ChatCharacterViewModel>(
                """
                SELECT
                    c."CharacterId", c."UserId", c."CharacterDisplayName",
                    'NormalCharacter' AS "CharacterNameClass",
                    ca."AvatarImageUrl"
                FROM "Characters" c
                LEFT JOIN "CharacterAvatars" ca ON c."CharacterId" = ca."CharacterId"
                WHERE c."CharacterId" = @CharacterId
                """,
                new { CharacterId = characterId }
            )).FirstOrDefault();

            if (character == null || character.UserId != userId)
            {
                await Clients.Caller.SendAsync("ReceiveError", "Invalid character or character does not belong to you.");
                return;
            }

            var finalAvatarUrl = _imageService.GetImageUrl(character.AvatarImageUrl, "avatar")
                                 ?? "/images/UserFiles/CharacterAvatars/NewAvatar.png";

            await _dataService.AddChatRoomPostAsync(
                chatRoomId,
                userId,
                character.CharacterId,
                messageContent,
                finalAvatarUrl,
                character.CharacterNameClass,
                character.CharacterDisplayName
            );

            var newPost = new ChatRoomPostsWithDetails
            {
                ChatRoomId = chatRoomId,
                CharacterId = character.CharacterId,
                PostContent = messageContent,
                CharacterDisplayName = character.CharacterDisplayName,
                CharacterThumbnail = finalAvatarUrl,
                CharacterNameClass = character.CharacterNameClass,
                PostDateTime = DateTime.Now
            };

            await Clients.Group(chatRoomId.ToString()).SendAsync("ReceiveMessage", newPost);
        }

        public async Task JoinRoom(int chatRoomId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, chatRoomId.ToString());
            await Clients.Caller.SendAsync("RoomJoined", chatRoomId);
        }

        public async Task LeaveRoom(int chatRoomId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatRoomId.ToString());
            await Clients.Caller.SendAsync("RoomLeft", chatRoomId);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}