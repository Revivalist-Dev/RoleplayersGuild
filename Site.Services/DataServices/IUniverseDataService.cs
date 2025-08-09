using RoleplayersGuild.Site.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Services.DataServices
{
    public interface IUniverseDataService : IBaseDataService
    {
        Task<Universe?> GetUniverseAsync(int universeId);
        Task<UniverseWithDetails?> GetUniverseWithDetailsAsync(int universeId);
        Task<int> CreateNewUniverseAsync(int userId);
        Task DeleteUniverseAsync(int universeId);
        Task<IEnumerable<UniverseWithDetails>> GetUserUniversesAsync(int userId);
        Task<Universe?> GetUniverseForEditAsync(int universeId, int userId);
        Task UpdateUniverseAsync(UniverseInputModel model, int userId);
        Task UpdateUniverseGenresAsync(int universeId, List<int> genreIds);
        Task<IEnumerable<int>> GetUniverseGenresAsync(int universeId);
        Task<PagedResult<UniverseWithDetails>> SearchUniversesAsync(int pageIndex, int pageSize, string? searchTerm, List<int> genreIds, int currentUserId);
        Task<IEnumerable<ArticleForListingViewModel>> GetUniverseArticlesByCategoryAsync(int universeId, int categoryId);
        Task<IEnumerable<Character>> GetUserCharactersNotInUniverseAsync(int userId, int universeId);
        Task<IEnumerable<Character>> GetUserCharactersInUniverseAsync(int userId, int universeId);
        Task AddCharactersToUniverseAsync(int universeId, List<int> characterIds);
        Task RemoveCharactersFromUniverseAsync(int universeId, List<int> characterIds);
    }
}
