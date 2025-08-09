using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using RoleplayersGuild.Site.Services.DataServices;

namespace RoleplayersGuild.Site.Directory.Admin_Panel.Articles
{
    public class IndexModel : PageModel
    {
        private readonly IContentDataService _contentDataService;
        private readonly IMiscDataService _miscDataService;

        public IndexModel(IContentDataService contentDataService, IMiscDataService miscDataService)
        {
            _contentDataService = contentDataService;
            _miscDataService = miscDataService;
        }

        [TempData]
        public string? Message { get; set; }
        [TempData]
        public bool IsSuccess { get; set; }

        public IEnumerable<ArticleForListingViewModel> AllArticles { get; set; } = new List<ArticleForListingViewModel>();
        public SelectList? Categories { get; set; }
        [BindProperty]
        public List<GenreSelectionViewModel> Genres { get; set; } = new();
        [BindProperty]
        public ArticleEditModel? Article { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            AllArticles = await _contentDataService.GetRecordsAsync<ArticleForListingViewModel>("""SELECT "ArticleId", "ArticleTitle" FROM "Articles" ORDER BY "ArticleTitle" """);
            if (id.HasValue)
            {
                var articleToEdit = await _contentDataService.GetArticleWithDetailsAsync(id.Value);
                if (articleToEdit == null) return NotFound();

                Article = new ArticleEditModel
                {
                    ArticleId = articleToEdit.ArticleId,
                    ArticleTitle = articleToEdit.ArticleTitle,
                    ArticleContent = articleToEdit.ArticleContent,
                    CategoryId = articleToEdit.CategoryId ?? 0,
                    ContentRatingId = articleToEdit.ContentRatingId ?? 1,
                    DisableLinkify = articleToEdit.DisableLinkify,
                    OwnerUserId = articleToEdit.OwnerUserId,
                    OwnerUserName = articleToEdit.AuthorUsername // Corrected this line
                };

                var articleGenres = await _contentDataService.GetArticleGenresAsync(id.Value);
                var allGenres = await _miscDataService.GetGenresAsync();
                Genres = allGenres.Select(g => new GenreSelectionViewModel
                {
                    GenreId = g.GenreId,
                    GenreName = g.GenreName,
                    IsSelected = articleGenres.Contains(g.GenreId)
                }).ToList();

                var categories = await _contentDataService.GetRecordsAsync<Category>("""SELECT * FROM "ArticleCategories" ORDER BY "CategoryName" """);
                Categories = new SelectList(categories, "CategoryId", "CategoryName", Article.CategoryId);
            }
            return Page();
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            if (Article == null || !ModelState.IsValid)
            {
                await OnGetAsync(Article?.ArticleId);
                return Page();
            }

            var updateModel = new ArticleInputModel
            {
                ArticleId = Article.ArticleId,
                ArticleTitle = Article.ArticleTitle,
                ArticleContent = Article.ArticleContent,
                ContentRatingId = Article.ContentRatingId,
                CategoryId = Article.CategoryId,
                DisableLinkify = Article.DisableLinkify,
                IsPrivate = false,
                UniverseId = null
            };

            await _contentDataService.UpdateArticleAsync(Article.ArticleId, updateModel);
            var selectedGenreIds = Genres.Where(g => g.IsSelected).Select(g => g.GenreId).ToList();
            await _contentDataService.UpdateArticleGenresAsync(Article.ArticleId, selectedGenreIds);

            IsSuccess = true;
            Message = "The article has been saved.";
            return RedirectToPage(new { id = Article.ArticleId });
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            await _contentDataService.DeleteArticleAsync(id);
            TempData["IsSuccess"] = true;
            TempData["Message"] = "The article has been deleted.";
            return RedirectToPage("./Index");
        }
    }
}
