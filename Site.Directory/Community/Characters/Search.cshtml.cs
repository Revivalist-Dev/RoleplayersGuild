using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.Community.Characters
{
    public class SearchModel : PageModel
    {
        private readonly IDataService _dataService;
        private readonly IUserService _userService;

        public SearchModel(IDataService dataService, IUserService userService)
        {
            _dataService = dataService;
            _userService = userService;
        }

        [BindProperty(SupportsGet = true)]
        public SearchInputModel Search { get; set; } = new();

        [BindProperty(SupportsGet = true, Name = "p")]
        public int CurrentPage { get; set; } = 1;

        public PagedResult<CharactersForListing>? PagedResults { get; private set; }
        public string PageTitle { get; private set; } = "Character Search";
        public SelectList Genders { get; private set; } = new(Enumerable.Empty<SelectListItem>());
        public SelectList SortOrders { get; private set; } = new(Enumerable.Empty<SelectListItem>());

        public async Task OnGetAsync()
        {
            await PopulateSelectListsAsync();

            var userId = _userService.GetUserId(User);
            const int pageSize = 36;

            // CORRECTED: The DataService now handles all image URL processing.
            // The manual processing loop that was here has been removed.
            PagedResults = await _dataService.SearchCharactersAsync(Search, userId, CurrentPage, pageSize);
        }

        private async Task PopulateSelectListsAsync()
        {
            var genders = await _dataService.GetGendersAsync();
            Genders = new SelectList(genders, "GenderId", "GenderName", Search.GenderId);

            var sortOptions = new List<SelectListItem>
            {
                new() { Value = "0", Text = "Newest First" },
                new() { Value = "1", Text = "Oldest First" }
            };
            SortOrders = new SelectList(sortOptions, "Value", "Text", Search.SortOrder);
        }

        public Dictionary<string, string> GetRouteDataForPaging()
        {
            var routeData = new Dictionary<string, string>
            {
                { "Search.Name", Search.Name ?? "" },
                { "Search.GenderId", Search.GenderId?.ToString() ?? "" },
                { "Search.SortOrder", Search.SortOrder.ToString() }
            };

            for (int i = 0; i < Search.SelectedGenreIds.Count; i++)
            {
                routeData.Add($"Search.SelectedGenreIds[{i}]", Search.SelectedGenreIds[i].ToString());
            }

            return routeData;
        }
    }
}