using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using RoleplayersGuild.Site.Services.DataServices;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.User_Panel.My_Universes
{
    public class CreateModel : UserPanelBaseModel
    {
        private readonly IUniverseDataService _universeDataService;
        private readonly IUserDataService _userDataService;

        [BindProperty]
        public UniverseInputModel Input { get; set; } = new();

        public SelectList Ratings { get; set; } = new SelectList(Enumerable.Empty<SelectListItem>());
        public SelectList Sources { get; set; } = new SelectList(Enumerable.Empty<SelectListItem>());
        public List<GenreSelectionViewModel> GenreSelection { get; set; } = new();

        public CreateModel(
            ICharacterDataService characterDataService,
            ICommunityDataService communityDataService,
            IMiscDataService miscDataService,
            IUserService userService,
            IUniverseDataService universeDataService,
            IUserDataService userDataService)
            : base(characterDataService, communityDataService, miscDataService, userService)
        {
            _universeDataService = universeDataService;
            _userDataService = userDataService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            await PopulateSelectListsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await PopulateSelectListsAsync();
                return Page();
            }

            var newUniverseId = await _universeDataService.CreateNewUniverseAsync(LoggedInUserId);
            Input.UniverseId = newUniverseId;

            await _universeDataService.UpdateUniverseAsync(Input, LoggedInUserId);
            await _universeDataService.UpdateUniverseGenresAsync(newUniverseId, Input.SelectedGenreIds);

            await _userDataService.AwardBadgeIfNotExistingAsync(35, LoggedInUserId);

            TempData["Message"] = "Universe created successfully!";
            return RedirectToPage("./Index");
        }

        private async Task PopulateSelectListsAsync()
        {
            var ratings = await _miscDataService.GetContentRatingsAsync();
            Ratings = new SelectList(ratings, "ContentRatingId", "ContentRatingName");

            var sources = await _miscDataService.GetCharacterSourcesAsync();
            Sources = new SelectList(sources, "SourceId", "SourceName");

            var allGenres = await _miscDataService.GetGenresAsync();
            GenreSelection = allGenres.Select(g => new GenreSelectionViewModel
            {
                GenreId = g.GenreId,
                GenreName = g.GenreName,
                IsSelected = false
            }).ToList();
        }
    }
}
