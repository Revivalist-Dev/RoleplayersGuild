using Microsoft.AspNetCore.Mvc;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.User_Panel.My_Profile
{
    public class IndexMyProfileModel : UserPanelBaseModel
    {
        public User? ProfileUser { get; set; }
        public IEnumerable<UserBadgeViewModel> Badges { get; set; } = Enumerable.Empty<UserBadgeViewModel>();
        public List<ArticleForListingViewModel> Articles { get; set; } = new();
        public IEnumerable<CharactersForListing> Characters { get; set; } = Enumerable.Empty<CharactersForListing>();

        // UPDATED: Constructor to match the new base class signature.
        public IndexMyProfileModel(IDataService dataService, IUserService userService)
            : base(dataService, userService) { }

        public async Task<IActionResult> OnGetAsync()
        {
            // LoggedInUserId is provided by the UserPanelBaseModel
            ProfileUser = await DataService.GetUserAsync(LoggedInUserId);
            if (ProfileUser is null)
            {
                return NotFound("User profile could not be found.");
            }

            ViewData["Title"] = "My Profile Preview";

            Badges = await DataService.GetUserBadgesAsync(LoggedInUserId);

            var articlesData = await DataService.GetUserPublicArticlesAsync(LoggedInUserId);
            var articleIds = articlesData.Select(a => a.ArticleId);
            if (articleIds.Any())
            {
                var genresLookup = await DataService.GetGenresForArticleListAsync(articleIds);
                Articles = articlesData.Select(a => new ArticleForListingViewModel
                {
                    ArticleId = a.ArticleId,
                    ArticleTitle = a.ArticleTitle,
                    ContentRating = a.ContentRating,
                    CategoryName = a.CategoryName,
                    Username = a.Username,
                    OwnerUserId = a.OwnerUserId,
                    CreatedDateTime = a.CreatedDateTime,
                    DateSubmitted = a.DateSubmitted,
                    IsPrivate = a.IsPrivate,
                    IsPublished = a.IsPublished,
                    CategoryId = a.CategoryId,
                    UniverseId = a.UniverseId,
                    Genres = genresLookup[a.ArticleId].ToList()
                }).ToList();
            }

            Characters = await DataService.GetUserPublicCharactersAsync(LoggedInUserId);

            return Page();
        }
    }
}