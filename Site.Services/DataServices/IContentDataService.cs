using RoleplayersGuild.Site.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Services.DataServices
{
    public interface IContentDataService : IBaseDataService
    {
        Task<ArticleWithDetails?> GetArticleWithDetailsAsync(int articleId);
        Task<int> CreateNewArticleAsync(int userId);
        Task DeleteArticleAsync(int articleId);
        Task<IEnumerable<int>> GetArticleGenresAsync(int articleId);
        Task UpdateArticleAsync(int articleId, ArticleInputModel model);
        Task UpdateArticleGenresAsync(int articleId, List<int> genreIds);
        Task<StoryWithDetails?> GetStoryWithDetailsAsync(int storyId);
        Task<int> CreateNewStoryAsync(int userId);
        Task DeleteStoryAsync(int storyId);
        Task<IEnumerable<int>> GetStoryGenresAsync(int storyId);
        Task UpdateStoryAsync(StoryInputModel model);
        Task UpdateStoryGenresAsync(int storyId, List<int> genreIds);
        Task<Proposal?> GetProposalAsync(int proposalId);
        Task<IEnumerable<ProposalStatus>> GetProposalStatusesAsync();
        Task<IEnumerable<int>> GetProposalGenresAsync(int proposalId);
        Task<int> CreateProposalAsync(ProposalInputModel model, int userId);
        Task UpdateProposalAsync(ProposalInputModel model, int userId);
        Task UpdateProposalGenresAsync(int proposalId, List<int> genreIds);
        Task DeleteProposalAsync(int proposalId, int userId);
        Task<IEnumerable<ArticleForListingViewModel>> GetUserArticlesAsync(int userId);
        Task<ILookup<int, string>> GetGenresForArticleListAsync(IEnumerable<int> articleIds);
        Task<PagedResult<ArticleForListingViewModel>> GetPublicArticlesAsync(int pageIndex, int pageSize);
        Task<IEnumerable<DashboardStory>> GetPopularStoriesForDashboardAsync(int userId);
        Task<IEnumerable<DashboardArticle>> GetNewestArticlesForDashboardAsync(int userId);
        Task<IEnumerable<DashboardProposal>> GetNewestProposalsForDashboardAsync(int userId);
        Task<IEnumerable<ProposalWithDetails>> GetUserProposalsAsync(int userId);
        Task<StoryPost?> GetStoryPostForEditAsync(int storyPostId, int userId);
        Task UpdateStoryPostAsync(int storyPostId, int characterId, string content);
        Task<IEnumerable<StoryWithDetails>> GetUserStoriesAsync(int userId);
        Task<ILookup<int, string>> GetGenresForStoryListAsync(IEnumerable<int> storyIds);
        Task<PagedResult<StoryPostViewModel>> GetStoryPostsPagedAsync(int storyId, int pageIndex, int pageSize);
        Task AddStoryPostAsync(int storyId, int characterId, string content);
        Task DeleteStoryPostAsync(int storyPostId, int currentUserId, int storyOwnerId);
        Task<int> UpsertArticleAsync(ArticleInputModel input, int userId);
        Task<int> UpsertProposalAsync(ProposalInputModel input, int userId);
        Task<int> UpsertStoryAsync(StoryInputModel input, int userId);
        Task<PagedResult<ProposalWithDetails>> SearchProposalsAsync(int pageIndex, int pageSize, List<int> genreIds);
        Task<PagedResult<StoryForListingViewModel>> SearchStoriesAsync(int pageIndex, int pageSize, string? searchTerm, List<int> genreIds, bool includeAdult, int? universeId, int currentUserId);
        Task<PagedResult<ArticleViewModel>> SearchArticlesAsync(int pageIndex, int pageSize, string? searchTerm, List<int> genreIds);
        Task<IEnumerable<ArticleWithDetails>> GetRecentArticlesAsync();
    }
}
