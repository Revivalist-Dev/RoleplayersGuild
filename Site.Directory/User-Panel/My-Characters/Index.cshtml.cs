using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.User_Panel.My_Characters
{
    public class IndexModel : PageModel
    {
        private readonly IDataService _dataService;
        private readonly IUserService _userService;

        public IndexModel(IDataService dataService, IUserService userService)
        {
            _dataService = dataService;
            _userService = userService;
        }

        [BindProperty(SupportsGet = true)]
        public SearchInputModel Search { get; set; } = new();

        [BindProperty(SupportsGet = true, Name = "p")]
        public int CurrentPage { get; set; } = 1;

        public PagedResult<CharactersForListing>? PagedResults { get; private set; }
        public SelectList Genders { get; private set; } = new(Enumerable.Empty<SelectListItem>());
        public SelectList SortOrders { get; private set; } = new(Enumerable.Empty<SelectListItem>());

        public async Task<IActionResult> OnGetAsync()
        {
            // CORRECTION 1: GetUserId returns an 'int'. 0 means not logged in.
            var userId = _userService.GetUserId(User);
            if (userId == 0)
            {
                return Challenge();
            }

            await PopulateSelectListsAsync();

            const int pageSize = 24;

            // CORRECTION 2: Call the newly created method and pass 'userId' directly.
            PagedResults = await _dataService.SearchUserCharactersAsync(userId, Search, CurrentPage, pageSize);

            return Page();
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