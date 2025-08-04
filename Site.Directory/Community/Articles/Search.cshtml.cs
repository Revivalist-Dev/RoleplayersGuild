using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.Community.Articles
{
    public class SearchArticlesModel : PageModel
    {
        private readonly IDataService _dataService;

        public SearchArticlesModel(IDataService dataService)
        {
            _dataService = dataService;
        }

        [BindProperty(SupportsGet = true)]
        public ArticleSearchInputModel SearchInput { get; set; } = new();

        [BindProperty(SupportsGet = true, Name = "p")]
        public int CurrentPage { get; set; } = 1;

        public PagedResult<ArticleViewModel>? SearchResults { get; set; }

        public async Task OnGetAsync()
        {
            const int pageSize = 20;
            // Corrected: Property is SelectedGenreIds
            SearchResults = await _dataService.SearchArticlesAsync(
                CurrentPage,
                pageSize,
                SearchInput.SearchTerm,
                SearchInput.SelectedGenreIds);
        }
    }
}