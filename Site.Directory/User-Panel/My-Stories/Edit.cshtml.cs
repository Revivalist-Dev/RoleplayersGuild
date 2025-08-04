using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.User_Panel.My_Stories
{
    public class EditMyStoriesModel : UserPanelBaseModel
    {
        private readonly IDataService _dataService;
        private readonly IUserService _userService;

        [BindProperty]
        public StoryInputModel Input { get; set; } = new();

        public SelectList Universes { get; set; } = new(Enumerable.Empty<SelectListItem>());
        public SelectList Ratings { get; set; } = new(Enumerable.Empty<SelectListItem>());
        public List<GenreSelectionViewModel> GenreSelection { get; set; } = new();

        public bool IsNew => Input.StoryId == 0;

        public EditMyStoriesModel(IDataService dataService, IUserService userService, ICookieService cookieService)
            : base(dataService, userService) // Corrected: Passed userService to base constructor.
        {
            _dataService = dataService;
            _userService = userService;
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            var userId = _userService.GetUserId(User);
            if (userId == 0) return RedirectToPage("/Index");

            if (id.HasValue)
            {
                var story = await _dataService.GetStoryWithDetailsAsync(id.Value);
                if (story is null || story.UserId != userId) return Forbid();

                Input = new StoryInputModel(story)
                {
                    SelectedGenreIds = (await _dataService.GetStoryGenresAsync(id.Value)).ToList()
                };
            }
            else
            {
                Input = new StoryInputModel();
            }

            await PopulateSelectListsAsync(userId);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = _userService.GetUserId(User);
            if (userId == 0) return Forbid();

            if (Request.Form.ContainsKey("Input.SelectedGenreIds"))
            {
                Input.SelectedGenreIds = Request.Form["Input.SelectedGenreIds"]
                    .Where(s => !string.IsNullOrEmpty(s))
                    .Select(s => int.Parse(s!))
                    .ToList();
            }

            if (!ModelState.IsValid)
            {
                await PopulateSelectListsAsync(userId);
                return Page();
            }

            int storyId = await _dataService.UpsertStoryAsync(Input, userId);
            await _dataService.UpdateStoryGenresAsync(storyId, Input.SelectedGenreIds);

            if (IsNew)
            {
                await _dataService.AwardBadgeIfNotExistingAsync(34, userId); // Story Creator Badge
            }

            TempData["Message"] = "Story saved successfully!";
            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var userId = _userService.GetUserId(User);
            var story = await _dataService.GetStoryWithDetailsAsync(id);
            if (userId == 0 || story is null || story.UserId != userId) return Forbid();

            await _dataService.DeleteStoryAsync(id);
            TempData["Message"] = "Story deleted successfully.";
            return RedirectToPage("./Index");
        }

        private async Task PopulateSelectListsAsync(int userId)
        {
            Universes = new SelectList(await _dataService.GetUserUniversesAsync(userId), "UniverseId", "UniverseName", Input.UniverseId);
            // CORRECTED: The text field is "ContentRatingName", not "ContentRating"
            Ratings = new SelectList(await _dataService.GetContentRatingsAsync(), "ContentRatingId", "ContentRatingName", Input.ContentRatingId);
            var allGenres = await _dataService.GetGenresAsync();
            GenreSelection = allGenres.Select(g => new GenreSelectionViewModel
            {
                GenreId = g.GenreId,
                GenreName = g.GenreName,
                IsSelected = Input.SelectedGenreIds.Contains(g.GenreId)
            }).ToList();
        }
    }
}