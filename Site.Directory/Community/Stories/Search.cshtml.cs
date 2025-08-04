using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.Community.Stories
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

        public PagedResult<StoryForListingViewModel>? StoriesResult { get; set; }
        public ILookup<int, string> GenresLookup { get; set; } = Enumerable.Empty<string>().ToLookup(x => 0);

        public async Task OnGetAsync()
        {
            var currentUserId = _userService.GetUserId(User);
            var userCanViewMature = _userService.GetUserPrefersMature(User);

            StoriesResult = await _dataService.SearchStoriesAsync(CurrentPage, 10, SearchTerm, SelectedGenreIds, userCanViewMature, null, currentUserId);

            if (StoriesResult is not null && StoriesResult.Items.Any())
            {
                var storyIds = StoriesResult.Items.Select(s => s.StoryId);
                GenresLookup = await _dataService.GetGenresForStoryListAsync(storyIds);
            }
        }
    }
}