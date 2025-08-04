using System.Collections.Generic;
using System.Threading.Tasks;
using RoleplayersGuild.Site.Model;

namespace RoleplayersGuild.Site.Services
{
    public interface IChatTrackerService
    {
        Task UserConnected(string connectionId);
        Task<(string UserName, IEnumerable<string> Channels)> UserDisconnected(string connectionId);
        Task SetCharacter(string userName, string connectionId, ChatCharacterViewModel character);
        Task<ChatCharacterViewModel> GetCharacterForUser(string userName);
        Task UserJoinedChannel(string userName, string channelName);
        Task UserLeftChannel(string userName, string channelName);
        Task<IEnumerable<ChatCharacterViewModel>> GetChattersInChannel(string channelName);
    }
}