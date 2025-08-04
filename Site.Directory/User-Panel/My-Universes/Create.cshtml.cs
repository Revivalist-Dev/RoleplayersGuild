using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// CORRECTED: The namespace was missing from the original file
namespace RoleplayersGuild.Site.Directory.User_Panel.My_Universes
{
    // CORRECTED: Inherit from UserPanelBaseModel for consistency
    public class CreateModel : UserPanelBaseModel
    {
        [BindProperty]
        public UniverseInputModel Input { get; set; } = new();

        public SelectList Ratings { get; set; } = new SelectList(Enumerable.Empty<SelectListItem>());
        public SelectList Sources { get; set; } = new SelectList(Enumerable.Empty<SelectListItem>());
        public List<GenreSelectionViewModel> GenreSelection { get; set; } = new();

        // CORRECTED: Constructor now injects IUserService and uses the base class
        public CreateModel(IDataService dataService, IUserService userService)
            : base(dataService, userService) { }

        public async Task<IActionResult> OnGetAsync()
        {
            // CORRECTED: Use the modern UserService to get the logged-in user's ID
            var userId = UserService.GetUserId(User);
            if (userId == 0) return Forbid();

            await PopulateSelectListsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // CORRECTED: Use the modern UserService
            var userId = UserService.GetUserId(User);
            if (userId == 0) return Forbid();

            if (!ModelState.IsValid)
            {
                await PopulateSelectListsAsync();
                return Page();
            }

            var newUniverseId = await DataService.CreateNewUniverseAsync(userId);
            Input.UniverseId = newUniverseId;

            await DataService.UpdateUniverseAsync(Input, userId);
            await DataService.UpdateUniverseGenresAsync(newUniverseId, Input.SelectedGenreIds);

            // Award Universe Creator Badge
            await DataService.AwardBadgeIfNotExistingAsync(35, userId);

            TempData["Message"] = "Universe created successfully!";
            return RedirectToPage("./Index");
        }

        private async Task PopulateSelectListsAsync()
        {
            var ratings = await DataService.GetContentRatingsAsync();
            // CORRECTED: The property name is "ContentRatingName", not "ContentRating"
            Ratings = new SelectList(ratings, "ContentRatingId", "ContentRatingName");

            var sources = await DataService.GetCharacterSourcesAsync();
            // The property name "SourceName" on the model was incorrect, it should be "Source"
            Sources = new SelectList(sources, "SourceId", "SourceName");

            var allGenres = await DataService.GetGenresAsync();
            GenreSelection = allGenres.Select(g => new GenreSelectionViewModel
            {
                GenreId = g.GenreId,
                GenreName = g.GenreName,
                IsSelected = false
            }).ToList();
        }
    }
}