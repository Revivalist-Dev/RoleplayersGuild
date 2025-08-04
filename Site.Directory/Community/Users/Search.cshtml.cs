using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.Community.Users
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
        public string? Username { get; set; }

        [BindProperty(SupportsGet = true, Name = "p")]
        public int CurrentPage { get; set; } = 1;

        public PagedResult<User>? SearchResults { get; set; }

        public async Task OnGetAsync()
        {
            const int pageSize = 24;
            var currentUserId = _userService.GetUserId(User);
            SearchResults = await _dataService.SearchUsersAsync(Username, CurrentPage, pageSize, currentUserId);
        }
    }
}