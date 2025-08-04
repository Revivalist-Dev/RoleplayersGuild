using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.Community.Users
{
    public class ViewUserModel : PageModel
    {
        private readonly IDataService _dataService;
        private readonly IUserService _userService;

        public ViewUserModel(IDataService dataService, IUserService userService)
        {
            _dataService = dataService;
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

            ProfileUser = await _dataService.GetUserAsync(id);
            if (ProfileUser is null)
            {
                return NotFound();
            }

            if (CurrentUserId > 0 && !IsViewingOwnProfile)
            {
                IsBlockedByCurrentUser = await _dataService.IsUserBlockedAsync(id, CurrentUserId);
                IsCurrentUserBlocked = await _dataService.IsUserBlockedAsync(CurrentUserId, id);
            }

            if (!IsBlockedByCurrentUser && !IsCurrentUserBlocked)
            {
                var articlesData = (await _dataService.GetUserPublicArticlesAsync(id)).ToList();
                var articleIds = articlesData.Select(a => a.ArticleId);
                if (articleIds.Any())
                {
                    var genresLookup = await _dataService.GetGenresForArticleListAsync(articleIds);
                    Articles = articlesData.Select(a => new ArticleForListingViewModel
                    {
                        ArticleId = a.ArticleId,
                        ArticleTitle = a.ArticleTitle,
                        ContentRating = a.ContentRating,
                        Genres = genresLookup.Contains(a.ArticleId) ? genresLookup[a.ArticleId].ToList() : new List<string>()
                    }).ToList();
                }

                Badges = await _dataService.GetUserBadgesAsync(id);
                Characters = await _dataService.GetUserPublicCharactersAsync(id);
            }

            ViewData["Title"] = ProfileUser.Username;
            return Page();
        }

        public async Task<IActionResult> OnPostBlockAsync(int id)
        {
            var currentUserId = _userService.GetUserId(User);
            if (currentUserId > 0 && currentUserId != id)
            {
                await _dataService.BlockUserAsync(currentUserId, id);
                TempData["Message"] = "User has been blocked.";
            }
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostUnblockAsync(int id)
        {
            var currentUserId = _userService.GetUserId(User);
            if (currentUserId > 0)
            {
                await _dataService.UnblockUserAsync(currentUserId, id);
                TempData["Message"] = "User has been unblocked.";
            }
            return RedirectToPage(new { id });
        }
    }
}