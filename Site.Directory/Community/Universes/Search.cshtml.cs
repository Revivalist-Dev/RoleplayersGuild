using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using RoleplayersGuild.Site.Services.DataServices;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.Community.Universes
{
    public class SearchModel : PageModel
    {
        private readonly IUniverseDataService _universeDataService;
        private readonly IUserService _userService;

        public SearchModel(IUniverseDataService universeDataService, IUserService userService)
        {
            _universeDataService = universeDataService;
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
            UniversesResult = await _universeDataService.SearchUniversesAsync(CurrentPage, 10, SearchTerm, SelectedGenreIds, currentUserId);
        }
    }
}
