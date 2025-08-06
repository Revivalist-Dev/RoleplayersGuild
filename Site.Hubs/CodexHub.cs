using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Hubs
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CodexHub : Hub
    {
        private readonly IDataService _dataService;
        private readonly IChatTrackerService _chatTracker;
        private readonly IImageService _imageService;

        // THE FIX IS HERE: Add IImageService imageService to the constructor parameters
        public CodexHub(IDataService dataService, IChatTrackerService chatTracker, IImageService imageService)
        {
            _dataService = dataService;
            _chatTracker = chatTracker;
            _imageService = imageService; // This line will now work
        }

        public override async Task OnConnectedAsync()
        {
            try
            {
                await _chatTracker.UserConnected(Context.ConnectionId);
                var channels = await _dataService.GetRecordsAsync<ChannelViewModel>(
                    @"SELECT ""ChatRoomId"" AS ""Id"", ""ChatRoomName"" as ""Title"", 0 AS ""UserCount"" FROM ""ChatRooms"" WHERE ""IsPublic"" = TRUE ORDER BY ""ChatRoomName""");
                await Clients.Caller.SendAsync("ReceiveChannelList", channels);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CodexHub OnConnectedAsync ERROR]: {ex.Message}");
                Context.Abort();
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var (userName, channels) = await _chatTracker.UserDisconnected(Context.ConnectionId);
            if (!string.IsNullOrEmpty(userName))
            {
                foreach (var channelName in channels)
                {
                    await Clients.Group(channelName).SendAsync("UserLeft", channelName, userName);
                }
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SelectCharacter(int characterId)
        {
            var userId = int.Parse(Context.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var userName = Context.User.FindFirst(ClaimTypes.Name).Value;

            var character = (await _dataService.GetRecordsAsync<ChatCharacterViewModel>(
                @"SELECT c.""CharacterId"", c.""UserId"", c.""CharacterDisplayName"", c.""CharacterFirstName"", c.""CharacterMiddleName"", c.""CharacterLastName"", ca.""AvatarImageUrl""
                  FROM ""Characters"" c
                  LEFT JOIN ""CharacterAvatars"" ca ON c.""CharacterId"" = ca.""CharacterId""
                  WHERE c.""CharacterId"" = @characterId AND c.""UserId"" = @userId",
                new { characterId, userId })).FirstOrDefault();

            if (character == null) return;

            character.AvatarImageUrl = _imageService.GetImageUrl(character.AvatarImageUrl);

            character.UserName = userName;
            await _chatTracker.SetCharacter(userName, Context.ConnectionId, character);
        }

        public async Task JoinChannel(string channelName)
        {
            try // <-- ADD THIS TRY BLOCK
            {
                var userName = Context.User.FindFirst(ClaimTypes.Name)?.Value;
                if (string.IsNullOrEmpty(userName)) return;

                await _chatTracker.UserJoinedChannel(userName, channelName);
                await Groups.AddToGroupAsync(Context.ConnectionId, channelName);

                var usersInChannel = await _chatTracker.GetChattersInChannel(channelName);
                await Clients.Caller.SendAsync("ReceiveUserList", channelName, usersInChannel);

                var chatter = await _chatTracker.GetCharacterForUser(userName);
                if (chatter != null)
                {
                    await Clients.OthersInGroup(channelName).SendAsync("UserJoined", channelName, chatter);
                }

                await Clients.Caller.SendAsync("JoinedChannel", channelName, "Welcome! You have joined the channel.");
            }
            catch (Exception ex) // <-- ADD THIS CATCH BLOCK
            {
                Console.WriteLine($"[CodexHub JoinChannel ERROR]: {ex.ToString()}");
            }
        }

        public async Task SendChannelMessage(string channelName, string message)
        {
            var userName = Context.User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userName)) return;

            var chatter = await _chatTracker.GetCharacterForUser(userName);
            if (chatter == null) return;

            var messageViewModel = new ChatMessageViewModel
            {
                Sender = chatter,
                Text = message,
                Timestamp = DateTime.UtcNow
            };

            await Clients.Group(channelName).SendAsync("ReceiveChannelMessage", channelName, messageViewModel);
        }
    }
}