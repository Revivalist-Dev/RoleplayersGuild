using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.Community.Universes
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
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public List<int> SelectedGenreIds { get; set; } = new();

        [BindProperty(SupportsGet = true, Name = "p")]
        public int CurrentPage { get; set; } = 1;

        public PagedResult<UniverseWithDetails>? UniversesResult { get; set; }

        public async Task OnGetAsync()
        {
            var currentUserId = _userService.GetUserId(User);
            UniversesResult = await _dataService.SearchUniversesAsync(CurrentPage, 10, SearchTerm, SelectedGenreIds, currentUserId);
        }
    }
}