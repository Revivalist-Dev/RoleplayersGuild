using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using RoleplayersGuild.Site.Services.DataServices;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.Community.Universes
{
    public class ViewUniverseModel : PageModel
    {
        private readonly IUniverseDataService _universeDataService;
        private readonly IUserDataService _userDataService;
        private readonly ICharacterDataService _characterDataService;
        private readonly IMiscDataService _miscDataService;
        private readonly IUserService _userService;

        public ViewUniverseModel(IUniverseDataService universeDataService, IUserDataService userDataService, ICharacterDataService characterDataService, IMiscDataService miscDataService, IUserService userService)
        {
            _universeDataService = universeDataService;
            _userDataService = userDataService;
            _characterDataService = characterDataService;
            _miscDataService = miscDataService;
            _userService = userService;
        }

        public UniverseWithDetails? Universe { get; set; }
        public bool IsBlocked { get; set; }
        public IEnumerable<string> Genres { get; set; } = Enumerable.Empty<string>();

        public IEnumerable<ArticleForListingViewModel> Objects { get; set; } = Enumerable.Empty<ArticleForListingViewModel>();
        public IEnumerable<ArticleForListingViewModel> Creatures { get; set; } = Enumerable.Empty<ArticleForListingViewModel>();
        public IEnumerable<ArticleForListingViewModel> Areas { get; set; } = Enumerable.Empty<ArticleForListingViewModel>();

        public IEnumerable<Character> CharactersToJoin { get; set; } = Enumerable.Empty<Character>();
        public IEnumerable<Character> CharactersToLeave { get; set; } = Enumerable.Empty<Character>();

        [BindProperty]
        public List<int> SelectedCharacterIds { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Universe = await _universeDataService.GetUniverseWithDetailsAsync(id);
            if (Universe is null) return NotFound();

            var currentUserId = _userService.GetUserId(User);
            if (currentUserId > 0)
            {
                IsBlocked = await _userDataService.IsUserBlockedAsync(Universe.UniverseOwnerId, currentUserId);
                if (!IsBlocked)
                {
                    CharactersToJoin = await _universeDataService.GetUserCharactersNotInUniverseAsync(currentUserId, id);
                    CharactersToLeave = await _universeDataService.GetUserCharactersInUniverseAsync(currentUserId, id);
                }
            }

            var allGenres = await _miscDataService.GetGenresAsync();
            var universeGenreIds = (await _universeDataService.GetUniverseGenresAsync(id)).ToHashSet();
            Genres = allGenres.Where(g => universeGenreIds.Contains(g.GenreId)).Select(g => g.GenreName);

            Objects = await _universeDataService.GetUniverseArticlesByCategoryAsync(id, 10); // CategoryId 10 for Objects
            Creatures = await _universeDataService.GetUniverseArticlesByCategoryAsync(id, 8); // CategoryId 8 for Creatures
            Areas = await _universeDataService.GetUniverseArticlesByCategoryAsync(id, 9); // CategoryId 9 for Areas

            ViewData["Title"] = Universe.UniverseName;
            return Page();
        }

        public async Task<IActionResult> OnPostJoinAsync(int id)
        {
            await _universeDataService.AddCharactersToUniverseAsync(id, SelectedCharacterIds);
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostLeaveAsync(int id)
        {
            await _universeDataService.RemoveCharactersFromUniverseAsync(id, SelectedCharacterIds);
            return RedirectToPage(new { id });
        }
    }
}
