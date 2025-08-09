using Microsoft.AspNetCore.Mvc;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using RoleplayersGuild.Site.Services.DataServices;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.User_Panel.My_Articles
{
    public class IndexMyArticlesModel : UserPanelBaseModel
    {
        public List<ArticleViewModel> Articles { get; set; } = new();

        // UPDATED: Constructor now injects IUserService
        public IndexMyArticlesModel(ICharacterDataService characterDataService, ICommunityDataService communityDataService, IMiscDataService miscDataService, IUserService userService, IContentDataService contentDataService)
            : base(characterDataService, communityDataService, miscDataService, userService)
        {
            _contentDataService = contentDataService;
        }
        private readonly IContentDataService _contentDataService;

        public async Task<IActionResult> OnGetAsync()
        {
            var userArticles = (await _contentDataService.GetUserArticlesAsync(LoggedInUserId)).ToList();
            if (!userArticles.Any())
            {
                return Page();
            }

            var articleIds = userArticles.Select(a => a.ArticleId);
            var genresLookup = await _contentDataService.GetGenresForArticleListAsync(articleIds);

            Articles = userArticles.Select(article => new ArticleViewModel
            {
                ArticleId = article.ArticleId,
                ArticleTitle = article.ArticleTitle,
                CategoryName = article.CategoryName,
                ContentRating = article.ContentRating,
                Genres = genresLookup.Contains(article.ArticleId) ? genresLookup[article.ArticleId].ToList() : new List<string>()
            }).ToList();

            return Page();
        }
    }
}
