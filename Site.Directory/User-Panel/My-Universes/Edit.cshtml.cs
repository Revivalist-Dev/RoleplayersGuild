using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.User_Panel.My_Universes
{
    // CORRECTED: Inherit from UserPanelBaseModel
    public class EditMyUniversesModel : UserPanelBaseModel
    {
        [BindProperty]
        public UniverseInputModel Input { get; set; } = new();

        public SelectList Ratings { get; set; } = new(Enumerable.Empty<SelectListItem>());
        public SelectList Sources { get; set; } = new(Enumerable.Empty<SelectListItem>());
        public List<GenreSelectionViewModel> GenreSelection { get; set; } = new();

        // CORRECTED: Constructor now uses IUserService and the base class
        public EditMyUniversesModel(IDataService dataService, IUserService userService)
            : base(dataService, userService) { }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            // CORRECTED: Use the modern UserService
            var userId = UserService.GetUserId(User);
            if (userId == 0) return Forbid();

            var universe = await DataService.GetUniverseForEditAsync(id, userId);
            if (universe is null) return Forbid();

            Input = new UniverseInputModel(universe)
            {
                SelectedGenreIds = (await DataService.GetUniverseGenresAsync(id)).ToList()
            };

            await PopulateSelectListsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = UserService.GetUserId(User);
            if (userId == 0) return Forbid();

            if (!ModelState.IsValid)
            {
                await PopulateSelectListsAsync();
                return Page();
            }

            await DataService.UpdateUniverseAsync(Input, userId);
            await DataService.UpdateUniverseGenresAsync(Input.UniverseId, Input.SelectedGenreIds);

            TempData["Message"] = "Universe saved successfully!";
            return RedirectToPage("./Index");
        }

        private async Task PopulateSelectListsAsync()
        {
            var ratings = await DataService.GetContentRatingsAsync();
            // CORRECTED: The property name is "ContentRatingName", not "ContentRating"
            Ratings = new SelectList(ratings, "ContentRatingId", "ContentRatingName", Input.ContentRatingId);

            var sources = await DataService.GetCharacterSourcesAsync();
            Sources = new SelectList(sources, "SourceId", "SourceName", Input.SourceTypeId);

            var allGenres = await DataService.GetGenresAsync();
            GenreSelection = allGenres.Select(g => new GenreSelectionViewModel
            {
                GenreId = g.GenreId,
                GenreName = g.GenreName,
                IsSelected = Input.SelectedGenreIds.Contains(g.GenreId)
            }).ToList();
        }
    }
}