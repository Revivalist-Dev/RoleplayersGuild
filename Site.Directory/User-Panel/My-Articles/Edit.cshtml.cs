using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using RoleplayersGuild.Site.Services.DataServices;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.User_Panel.My_Articles
{
    public class EditMyArticlesModel : UserPanelBaseModel
    {
        private readonly IContentDataService _contentDataService;
        private readonly IUniverseDataService _universeDataService;

        [BindProperty]
        public ArticleInputModel Input { get; set; } = new();

        public SelectList Categories { get; set; } = new(Enumerable.Empty<SelectListItem>());
        public SelectList Universes { get; set; } = new(Enumerable.Empty<SelectListItem>());
        public SelectList Ratings { get; set; } = new(Enumerable.Empty<SelectListItem>());
        public List<GenreSelectionViewModel> GenreSelection { get; set; } = new();

        public bool IsNewArticle => Input.ArticleId == 0;

        public EditMyArticlesModel(ICharacterDataService characterDataService, ICommunityDataService communityDataService, IMiscDataService miscDataService, IUserService userService, IContentDataService contentDataService, IUniverseDataService universeDataService)
            : base(characterDataService, communityDataService, miscDataService, userService)
        {
            _contentDataService = contentDataService;
            _universeDataService = universeDataService;
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            var userId = _userService.GetUserId(User);
            if (userId == 0) return RedirectToPage("/");

            if (id.HasValue)
            {
                var article = await _contentDataService.GetArticleWithDetailsAsync(id.Value);
                if (article is null || article.OwnerUserId != userId)
                {
                    return Forbid();
                }
                Input = new ArticleInputModel(article)
                {
                    SelectedGenreIds = (await _contentDataService.GetArticleGenresAsync(id.Value)).ToList()
                };
            }

            await PopulateSelectListsAsync(userId);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = _userService.GetUserId(User);
            if (userId == 0) return Forbid();

            if (!ModelState.IsValid)
            {
                await PopulateSelectListsAsync(userId);
                return Page();
            }

            int articleId = await _contentDataService.UpsertArticleAsync(Input, userId);
            await _contentDataService.UpdateArticleGenresAsync(articleId, Input.SelectedGenreIds);

            TempData["Message"] = "Article saved successfully!";
            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var userId = _userService.GetUserId(User);
            var article = await _contentDataService.GetArticleWithDetailsAsync(id);

            if (userId == 0 || article is null || article.OwnerUserId != userId)
            {
                return Forbid();
            }

            await _contentDataService.DeleteArticleAsync(id);
            TempData["Message"] = "Article deleted successfully.";
            return RedirectToPage("./Index");
        }

        private async Task PopulateSelectListsAsync(int userId)
        {
            Categories = new SelectList(await _miscDataService.GetCategoriesAsync(), "CategoryId", "CategoryName", Input.CategoryId);
            Universes = new SelectList(await _universeDataService.GetUserUniversesAsync(userId), "UniverseId", "UniverseName", Input.UniverseId);
            Ratings = new SelectList(await _miscDataService.GetContentRatingsAsync(), "ContentRatingId", "ContentRatingName", Input.ContentRatingId);

            var allGenres = await _miscDataService.GetGenresAsync();
            GenreSelection = allGenres.Select(g => new GenreSelectionViewModel
            {
                GenreId = g.GenreId,
                GenreName = g.GenreName,
                IsSelected = Input.SelectedGenreIds.Contains(g.GenreId)
            }).ToList();
        }
    }
}
