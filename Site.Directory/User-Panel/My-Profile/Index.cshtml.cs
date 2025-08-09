using Microsoft.AspNetCore.Mvc;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using RoleplayersGuild.Site.Services.DataServices;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.User_Panel.My_Profile
{
    public class IndexMyProfileModel : UserPanelBaseModel
    {
        private readonly IUserDataService _userDataService;
        private readonly IContentDataService _contentDataService;
        public User? ProfileUser { get; set; }
        public IEnumerable<UserBadgeViewModel> Badges { get; set; } = Enumerable.Empty<UserBadgeViewModel>();
        public List<ArticleForListingViewModel> Articles { get; set; } = new();
        public IEnumerable<CharactersForListing> Characters { get; set; } = Enumerable.Empty<CharactersForListing>();

        public IndexMyProfileModel(
            ICharacterDataService characterDataService,
            ICommunityDataService communityDataService,
            IMiscDataService miscDataService,
            IUserService userService,
            IUserDataService userDataService,
            IContentDataService contentDataService)
            : base(characterDataService, communityDataService, miscDataService, userService)
        {
            _userDataService = userDataService;
            _contentDataService = contentDataService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            ProfileUser = await _userDataService.GetUserAsync(LoggedInUserId);
            if (ProfileUser is null)
            {
                return NotFound("User profile could not be found.");
            }

            ViewData["Title"] = "My Profile Preview";

            Badges = await _userDataService.GetUserBadgesAsync(LoggedInUserId);

            var articlesData = await _contentDataService.GetUserArticlesAsync(LoggedInUserId);
            var articleIds = articlesData.Select(a => a.ArticleId);
            if (articleIds.Any())
            {
                var genresLookup = await _contentDataService.GetGenresForArticleListAsync(articleIds);
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

            Characters = await _userDataService.GetUserCharactersForListingAsync(LoggedInUserId);

            return Page();
        }
    }
}
