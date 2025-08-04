using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.User_Panel.My_Articles
{
    public class EditMyArticlesModel : UserPanelBaseModel
    {
        // NOTE: Redundant fields were removed. 'DataService' and 'UserService' from the base class will be used.

        [BindProperty]
        public ArticleInputModel Input { get; set; } = new();

        public SelectList Categories { get; set; } = new(Enumerable.Empty<SelectListItem>());
        public SelectList Universes { get; set; } = new(Enumerable.Empty<SelectListItem>());
        public SelectList Ratings { get; set; } = new(Enumerable.Empty<SelectListItem>());
        public List<GenreSelectionViewModel> GenreSelection { get; set; } = new();

        public bool IsNewArticle => Input.ArticleId == 0;

        // UPDATED: Constructor to match the new base class signature.
        public EditMyArticlesModel(IDataService dataService, IUserService userService)
            : base(dataService, userService)
        {
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            var userId = UserService.GetUserId(User);
            if (userId == 0) return RedirectToPage("/");

            if (id.HasValue)
            {
                var article = await DataService.GetArticleWithDetailsAsync(id.Value);
                if (article is null || article.OwnerUserId != userId)
                {
                    return Forbid();
                }
                Input = new ArticleInputModel(article)
                {
                    SelectedGenreIds = (await DataService.GetArticleGenresAsync(id.Value)).ToList()
                };
            }

            await PopulateSelectListsAsync(userId);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = UserService.GetUserId(User);
            if (userId == 0) return Forbid();

            if (!ModelState.IsValid)
            {
                await PopulateSelectListsAsync(userId);
                return Page();
            }

            int articleId = await DataService.UpsertArticleAsync(Input, userId);
            await DataService.UpdateArticleGenresAsync(articleId, Input.SelectedGenreIds);

            TempData["Message"] = "Article saved successfully!";
            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var userId = UserService.GetUserId(User);
            var article = await DataService.GetArticleWithDetailsAsync(id);

            if (userId == 0 || article is null || article.OwnerUserId != userId)
            {
                return Forbid();
            }

            await DataService.DeleteArticleAsync(id);
            TempData["Message"] = "Article deleted successfully.";
            return RedirectToPage("./Index");
        }

        private async Task PopulateSelectListsAsync(int userId)
        {
            Categories = new SelectList(await DataService.GetCategoriesAsync(), "CategoryId", "CategoryName", Input.CategoryId);
            Universes = new SelectList(await DataService.GetUserUniversesAsync(userId), "UniverseId", "UniverseName", Input.UniverseId);
            Ratings = new SelectList(await DataService.GetContentRatingsAsync(), "ContentRatingId", "ContentRatingName", Input.ContentRatingId);

            var allGenres = await DataService.GetGenresAsync();
            GenreSelection = allGenres.Select(g => new GenreSelectionViewModel
            {
                GenreId = g.GenreId,
                GenreName = g.GenreName,
                IsSelected = Input.SelectedGenreIds.Contains(g.GenreId)
            }).ToList();
        }
    }
}