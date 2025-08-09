using RoleplayersGuild.Site.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Services.DataServices
{
    public interface ICommunityDataService : IBaseDataService
    {
        Task<ChatRoom?> GetChatRoomAsync(int chatRoomId);
        Task<ChatRoomWithDetails?> GetChatRoomWithDetailsAsync(int chatRoomId);
        Task<int> CreateNewChatRoomAsync(int userId);
        Task DeleteChatRoomAsync(int chatRoomId);
        Task<IEnumerable<ChatRoomPostsWithDetails>> GetChatRoomPostsAsync(int chatRoomId, int lastPostId);
        Task AddChatRoomPostAsync(int chatRoomId, int userId, int characterId, string postContent, string? characterThumbnail, string? characterNameClass, string? characterDisplayName);
        Task UpdateChatRoomAsync(ChatRoomInputModel model);
        Task<int> CreateNewThreadAsync(string threadTitle, int creatorUserId);
        Task InsertThreadUserAsync(int userId, int threadId, int readStatusId, int characterId, int permissionId);
        Task InsertMessageAsync(int threadId, int creatorId, string messageContent);
        Task RemoveThreadCharacterAsync(int characterId, int threadId);
        Task RemoveThreadUserAsync(int userId, int threadId);
        Task NukeThreadAsync(int threadId);
        Task MarkUnreadForOthersOnThreadAsync(int threadId, int currentUserId);
        Task MarkReadForCurrentUserAsync(int threadId, int currentUserId);
        Task ChangeReadStatusForCurrentUserAsync(int threadId, int readStatus, int currentUserId);
        Task<ThreadDetails?> GetThreadDetailsForUserAsync(int threadId, int currentUserId);
        Task<IEnumerable<DashboardChatRoom>> GetActiveChatRoomsForDashboardAsync(int userId);
        Task<IEnumerable<ThreadDetails>> GetUserThreadsAsync(int userId, string filter);
        Task<IEnumerable<ThreadMessageViewModel>> GetThreadMessagesAsync(int threadId);
        Task<IEnumerable<ThreadParticipantViewModel>> GetThreadParticipantsAsync(int threadId);
        Task<PagedResult<ChatRoomWithDetails>> SearchChatRoomsAsync(int pageIndex, int pageSize, string? searchTerm, List<int> genreIds);
        Task<IEnumerable<DashboardChatRoom>> GetDashboardChatRoomsAsync(int userId);
        Task<IEnumerable<ChatParticipantViewModel>> GetChatRoomParticipantsAsync(int chatRoomId);
        Task<IEnumerable<ChatRoomWithDetails>> GetMyChatRoomsAsync(int userId);
        Task<int> GetUnreadThreadCountAsync(int userId);
        Task<IEnumerable<User>> GetThreadRecipientsAsync(int threadId);
        Task<IEnumerable<ChannelViewModel>> GetPublicChannelsAsync();
        Task UpdateThreadTitleAsync(int threadId, string title);
    }
}
