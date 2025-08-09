using RoleplayersGuild.Site.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Services.DataServices
{
    public interface IMiscDataService : IBaseDataService
    {
        Task LogErrorAsync(string errorDetails);
        Task<Ad?> GetRandomAdAsync(int adType);
        Task<IEnumerable<Category>> GetCategoriesAsync();
        Task<IEnumerable<ContentRating>> GetContentRatingsAsync();
        Task<IEnumerable<Genre>> GetGenresAsync();
        Task<IEnumerable<Gender>> GetGendersAsync();
        Task<IEnumerable<SexualOrientation>> GetSexualOrientationsAsync();
        Task<IEnumerable<CharacterSource>> GetCharacterSourcesAsync();
        Task<IEnumerable<PostLength>> GetPostLengthsAsync();
        Task<IEnumerable<LiteracyLevel>> GetLiteracyLevelsAsync();
        Task<IEnumerable<LfrpStatus>> GetLfrpStatusesAsync();
        Task<IEnumerable<EroticaPreference>> GetEroticaPreferencesAsync();
        Task<DashboardFunding> GetDashboardFundingAsync();
        Task<int> GetOpenAdminItemCountAsync(int userId);
        Task<IEnumerable<ToDoItemViewModel>> GetDevelopmentItemsAsync();
        Task<IEnumerable<ToDoItemViewModel>> GetConsiderationItemsAsync();
        Task<int> AddToDoItemAsync(string name, string description, int userId);
        Task AddVoteAsync(int itemId, int userId);
        Task RemoveVoteAsync(int itemId, int userId);
        Task<bool> HasUserVotedAsync(int itemId, int userId);
        Task<IEnumerable<QuickLink>> GetUserQuickLinksAsync(int userId);
        Task AddQuickLinkAsync(QuickLink newLink);
        Task DeleteQuickLinkAsync(int quickLinkId, int userId);
        Task<IEnumerable<DashboardItemViewModel>> GetDashboardItemsAsync(string itemType, string filter, int userId);
    }
}
