using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Threading.Tasks;
using RoleplayersGuild.Site.Services;
using RoleplayersGuild.Site.Services.DataServices;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services.Models;
using System.Linq;
using System;

namespace RoleplayersGuild.Site.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ICharacterDataService _characterDataService;
        private readonly ICommunityDataService _communityDataService;
        private readonly IUrlProcessingService _urlProcessingService;

        public ChatHub(ICharacterDataService characterDataService, ICommunityDataService communityDataService, IUrlProcessingService urlProcessingService)
        {
            _characterDataService = characterDataService;
            _communityDataService = communityDataService;
            _urlProcessingService = urlProcessingService;
        }

        public async Task SendMessage(int chatRoomId, int characterId, string messageContent)
        {
            if (Context.User is null)
            {
                await Clients.Caller.SendAsync("ReceiveError", "You must be logged in to chat.");
                return;
            }
            var currentUserId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId is null || !int.TryParse(currentUserId, out int userId) || userId == 0)
            {
                await Clients.Caller.SendAsync("ReceiveError", "You must be logged in to chat.");
                return;
            }

            if (string.IsNullOrWhiteSpace(messageContent) || chatRoomId <= 0 || characterId <= 0)
            {
                await Clients.Caller.SendAsync("ReceiveError", "Message content or character/room ID is invalid.");
                return;
            }

            var character = await _characterDataService.GetCharacterForChatAsync(characterId);

            if (character == null || character.UserId != userId)
            {
                await Clients.Caller.SendAsync("ReceiveError", "Invalid character or character does not belong to you.");
                return;
            }

            var finalAvatarUrl = _urlProcessingService.GetCharacterImageUrl((ImageUploadPath)character.AvatarImageUrl);

            await _communityDataService.AddChatRoomPostAsync(
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
