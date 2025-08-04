using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RoleplayersGuild.Site.Model;

namespace RoleplayersGuild.Site.Services
{
    public class ChatTrackerService : IChatTrackerService
    {
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, ChatCharacterViewModel>> _channels = new(System.StringComparer.OrdinalIgnoreCase);
        private static readonly ConcurrentDictionary<string, ChatCharacterViewModel> _userCharacters = new(System.StringComparer.OrdinalIgnoreCase);
        private static readonly ConcurrentDictionary<string, string> _connectionUsers = new();

        public Task UserConnected(string connectionId)
        {
            // We can add logic here if needed when a raw connection is made
            return Task.CompletedTask;
        }

        public async Task<(string UserName, IEnumerable<string> Channels)> UserDisconnected(string connectionId)
        {
            _connectionUsers.TryRemove(connectionId, out var userName);
            if (string.IsNullOrEmpty(userName)) return (null, Enumerable.Empty<string>());

            var channelsLeft = new List<string>();
            foreach (var channelName in _channels.Keys)
            {
                if (_channels.TryGetValue(channelName, out var users) && users.ContainsKey(userName))
                {
                    channelsLeft.Add(channelName);
                    await UserLeftChannel(userName, channelName);
                }
            }
            _userCharacters.TryRemove(userName, out _);
            return (userName, channelsLeft);
        }

        public Task SetCharacter(string userName, string connectionId, ChatCharacterViewModel character)
        {
            _userCharacters[userName] = character;
            _connectionUsers[connectionId] = userName;
            return Task.CompletedTask;
        }

        public Task<ChatCharacterViewModel> GetCharacterForUser(string userName)
        {
            _userCharacters.TryGetValue(userName, out var character);
            return Task.FromResult(character);
        }

        public async Task UserJoinedChannel(string userName, string channelName)
        {
            var character = await GetCharacterForUser(userName);
            if (character == null) return;

            var channelUsers = _channels.GetOrAdd(channelName, new ConcurrentDictionary<string, ChatCharacterViewModel>(System.StringComparer.OrdinalIgnoreCase));
            channelUsers.TryAdd(userName, character);
        }

        public Task UserLeftChannel(string userName, string channelName)
        {
            if (_channels.TryGetValue(channelName, out var channelUsers))
            {
                channelUsers.TryRemove(userName, out _);
            }
            return Task.CompletedTask;
        }

        public Task<IEnumerable<ChatCharacterViewModel>> GetChattersInChannel(string channelName)
        {
            if (_channels.TryGetValue(channelName, out var channelUsers))
            {
                return Task.FromResult(channelUsers.Values.AsEnumerable());
            }
            return Task.FromResult(Enumerable.Empty<ChatCharacterViewModel>());
        }
    }
}