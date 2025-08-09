using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using RoleplayersGuild.Site.Services.DataServices;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.User_Panel.My_Stories
{
    public class EditMyStoriesModel : UserPanelBaseModel
    {
        private readonly IContentDataService _contentDataService;
        private readonly IUniverseDataService _universeDataService;
        private readonly IUserDataService _userDataService;

        [BindProperty]
        public StoryInputModel Input { get; set; } = new();

        public SelectList Universes { get; set; } = new(Enumerable.Empty<SelectListItem>());
        public SelectList Ratings { get; set; } = new(Enumerable.Empty<SelectListItem>());
        public List<GenreSelectionViewModel> GenreSelection { get; set; } = new();

        public bool IsNew => Input.StoryId == 0;

        public EditMyStoriesModel(
            ICharacterDataService characterDataService,
            ICommunityDataService communityDataService,
            IMiscDataService miscDataService,
            IUserService userService,
            IContentDataService contentDataService,
            IUniverseDataService universeDataService,
            IUserDataService userDataService)
            : base(characterDataService, communityDataService, miscDataService, userService)
        {
            _contentDataService = contentDataService;
            _universeDataService = universeDataService;
            _userDataService = userDataService;
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id.HasValue)
            {
                var story = await _contentDataService.GetStoryWithDetailsAsync(id.Value);
                if (story is null || story.UserId != LoggedInUserId) return Forbid();

                Input = new StoryInputModel(story)
                {
                    SelectedGenreIds = (await _contentDataService.GetStoryGenresAsync(id.Value)).ToList()
                };
            }
            else
            {
                Input = new StoryInputModel();
            }

            await PopulateSelectListsAsync(LoggedInUserId);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Request.Form.ContainsKey("Input.SelectedGenreIds"))
            {
                Input.SelectedGenreIds = Request.Form["Input.SelectedGenreIds"]
                    .Where(s => !string.IsNullOrEmpty(s))
                    .Select(s => int.Parse(s!))
                    .ToList();
            }

            if (!ModelState.IsValid)
            {
                await PopulateSelectListsAsync(LoggedInUserId);
                return Page();
            }

            int storyId = await _contentDataService.UpsertStoryAsync(Input, LoggedInUserId);
            await _contentDataService.UpdateStoryGenresAsync(storyId, Input.SelectedGenreIds);

            if (IsNew)
            {
                await _userDataService.AwardBadgeIfNotExistingAsync(34, LoggedInUserId); // Story Creator Badge
            }

            TempData["Message"] = "Story saved successfully!";
            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var story = await _contentDataService.GetStoryWithDetailsAsync(id);
            if (story is null || story.UserId != LoggedInUserId) return Forbid();

            await _contentDataService.DeleteStoryAsync(id);
            TempData["Message"] = "Story deleted successfully.";
            return RedirectToPage("./Index");
        }

        private async Task PopulateSelectListsAsync(int userId)
        {
            Universes = new SelectList(await _universeDataService.GetUserUniversesAsync(userId), "UniverseId", "UniverseName", Input.UniverseId);
            Ratings = new SelectList(await _miscDataService.GetContentRatingsAsync(), "ContentRatingId", "ContentRatingName", Input.ContentRatingId);
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
