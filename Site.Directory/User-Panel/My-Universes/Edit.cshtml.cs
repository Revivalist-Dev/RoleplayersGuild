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
    public class EditMyUniversesModel : UserPanelBaseModel
    {
        private readonly IUniverseDataService _universeDataService;

        [BindProperty]
        public UniverseInputModel Input { get; set; } = new();

        public SelectList Ratings { get; set; } = new(Enumerable.Empty<SelectListItem>());
        public SelectList Sources { get; set; } = new(Enumerable.Empty<SelectListItem>());
        public List<GenreSelectionViewModel> GenreSelection { get; set; } = new();

        public EditMyUniversesModel(
            ICharacterDataService characterDataService,
            ICommunityDataService communityDataService,
            IMiscDataService miscDataService,
            IUserService userService,
            IUniverseDataService universeDataService)
            : base(characterDataService, communityDataService, miscDataService, userService)
        {
            _universeDataService = universeDataService;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var universe = await _universeDataService.GetUniverseForEditAsync(id, LoggedInUserId);
            if (universe is null) return Forbid();

            Input = new UniverseInputModel(universe)
            {
                SelectedGenreIds = (await _universeDataService.GetUniverseGenresAsync(id)).ToList()
            };

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

            await _universeDataService.UpdateUniverseAsync(Input, LoggedInUserId);
            await _universeDataService.UpdateUniverseGenresAsync(Input.UniverseId, Input.SelectedGenreIds);

            TempData["Message"] = "Universe saved successfully!";
            return RedirectToPage("./Index");
        }

        private async Task PopulateSelectListsAsync()
        {
            var ratings = await _miscDataService.GetContentRatingsAsync();
            Ratings = new SelectList(ratings, "ContentRatingId", "ContentRatingName", Input.ContentRatingId);

            var sources = await _miscDataService.GetCharacterSourcesAsync();
            Sources = new SelectList(sources, "SourceId", "SourceName", Input.SourceTypeId);

            var allGenres = await _miscDataService.GetGenresAsync();
            GenreSelection = allGenres.Select(g => new GenreSelectionViewModel
            {
                GenreId = g.GenreId,
                GenreName = g.GenreName,
                IsSelected = Input.SelectedGenreIds.Contains(g.GenreId)
            }).ToList();
        }
    }
}
