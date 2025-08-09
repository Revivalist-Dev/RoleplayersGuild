using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using RoleplayersGuild.Site.Services.DataServices;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.Community.Users
{
    public class ViewUserModel : PageModel
    {
        private readonly IUserDataService _userDataService;
        private readonly IContentDataService _contentDataService;
        private readonly ICharacterDataService _characterDataService;
        private readonly IUserService _userService;

        public ViewUserModel(IUserDataService userDataService, IContentDataService contentDataService, ICharacterDataService characterDataService, IUserService userService)
        {
            _userDataService = userDataService;
            _contentDataService = contentDataService;
            _characterDataService = characterDataService;
            _userService = userService;
        }

        public User? ProfileUser { get; set; }
        public IEnumerable<UserBadgeViewModel> Badges { get; set; } = Enumerable.Empty<UserBadgeViewModel>();
        public List<ArticleForListingViewModel> Articles { get; set; } = new();
        public IEnumerable<CharactersForListing> Characters { get; set; } = Enumerable.Empty<CharactersForListing>();

        public bool IsViewingOwnProfile { get; set; }
        public bool IsBlockedByCurrentUser { get; set; }
        public bool IsCurrentUserBlocked { get; set; }
        public int CurrentUserId { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            CurrentUserId = _userService.GetUserId(User);
            IsViewingOwnProfile = id == CurrentUserId;

            ProfileUser = await _userDataService.GetUserAsync(id);
            if (ProfileUser is null)
            {
                return NotFound();
            }

            if (CurrentUserId > 0 && !IsViewingOwnProfile)
            {
                IsBlockedByCurrentUser = await _userDataService.IsUserBlockedAsync(id, CurrentUserId);
                IsCurrentUserBlocked = await _userDataService.IsUserBlockedAsync(CurrentUserId, id);
            }

            if (!IsBlockedByCurrentUser && !IsCurrentUserBlocked)
            {
                var articlesData = (await _userDataService.GetUserPublicArticlesAsync(id)).ToList();
                var articleIds = articlesData.Select(a => a.ArticleId);
                if (articleIds.Any())
                {
                    var genresLookup = await _contentDataService.GetGenresForArticleListAsync(articleIds);
                    Articles = articlesData.Select(a => new ArticleForListingViewModel
                    {
                        ArticleId = a.ArticleId,
                        ArticleTitle = a.ArticleTitle,
                        ContentRating = a.ContentRating,
                        Genres = genresLookup.Contains(a.ArticleId) ? genresLookup[a.ArticleId].ToList() : new List<string>()
                    }).ToList();
                }

                Badges = await _userDataService.GetUserBadgesAsync(id);
                Characters = await _userDataService.GetUserCharactersForListingAsync(id);
            }

            ViewData["Title"] = ProfileUser.Username;
            return Page();
        }

        public async Task<IActionResult> OnPostBlockAsync(int id)
        {
            var currentUserId = _userService.GetUserId(User);
            if (currentUserId > 0 && currentUserId != id)
            {
                await _userDataService.BlockUserAsync(currentUserId, id);
                TempData["Message"] = "User has been blocked.";
            }
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostUnblockAsync(int id)
        {
            var currentUserId = _userService.GetUserId(User);
            if (currentUserId > 0)
            {
                await _userDataService.UnblockUserAsync(currentUserId, id);
                TempData["Message"] = "User has been unblocked.";
            }
            return RedirectToPage(new { id });
        }
    }
}
