using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using RoleplayersGuild.Site.Services.DataServices;
using RoleplayersGuild.Site.Services.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Hubs
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CodexHub : Hub
    {
        private readonly ICommunityDataService _communityDataService;
        private readonly ICharacterDataService _characterDataService;
        private readonly IChatTrackerService _chatTracker;
        private readonly IUrlProcessingService _urlProcessingService;

        // THE FIX IS HERE: Add IImageService imageService to the constructor parameters
        public CodexHub(ICommunityDataService communityDataService, ICharacterDataService characterDataService, IChatTrackerService chatTracker, IUrlProcessingService urlProcessingService)
        {
            _communityDataService = communityDataService;
            _characterDataService = characterDataService;
            _chatTracker = chatTracker;
            _urlProcessingService = urlProcessingService;
        }

        public override async Task OnConnectedAsync()
        {
            try
            {
                await _chatTracker.UserConnected(Context.ConnectionId);
                var channels = await _communityDataService.GetPublicChannelsAsync();
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
            if (Context.User is null) return;
            var userIdClaim = Context.User.FindFirst(ClaimTypes.NameIdentifier);
            var userNameClaim = Context.User.FindFirst(ClaimTypes.Name);

            if (userIdClaim is null || userNameClaim is null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                // Cannot identify user, handle appropriately
                return;
            }
            var userName = userNameClaim.Value;

            var character = await _characterDataService.GetCharacterForChatAsync(characterId);

            if (character == null || character.UserId != userId) return;

            character.AvatarImageUrl = _urlProcessingService.GetCharacterImageUrl((ImageUploadPath)character.AvatarImageUrl);

            if (character.UserName is null)
            {
                character.UserName = userName;
            }
            await _chatTracker.SetCharacter(userName, Context.ConnectionId, character);
        }

        public async Task JoinChannel(string channelName)
        {
            try // <-- ADD THIS TRY BLOCK
            {
                var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
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
            var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
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
