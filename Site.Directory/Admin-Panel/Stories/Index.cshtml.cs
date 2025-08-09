using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using RoleplayersGuild.Site.Services.DataServices;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

// Corrected: Namespace now matches folder structure
namespace RoleplayersGuild.Site.Directory.Admin_Panel.Stories
{
    // [Authorize(Policy = "IsStaff")]
    public class IndexModel : PageModel
    {
        private readonly IContentDataService _contentDataService;
        private readonly IMiscDataService _miscDataService;

        public IndexModel(IContentDataService contentDataService, IMiscDataService miscDataService)
        {
            _contentDataService = contentDataService;
            _miscDataService = miscDataService;
        }

        [TempData]
        public string? Message { get; set; }
        [TempData]
        public bool IsSuccess { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? SearchStoryId { get; set; }

        [BindProperty]
        public StoryEditModel? Story { get; set; }

        [BindProperty]
        public List<GenreSelectionViewModel> Genres { get; set; } = new();

        public SelectList? Universes { get; set; }

        public async Task OnGetAsync()
        {
            if (SearchStoryId.HasValue)
            {
                var story = await _contentDataService.GetStoryWithDetailsAsync(SearchStoryId.Value);
                if (story == null)
                {
                    IsSuccess = false;
                    Message = $"No story found with ID {SearchStoryId.Value}.";
                    return;
                }

                Story = new StoryEditModel
                {
                    StoryId = story.StoryId,
                    Title = story.StoryTitle,
                    Description = story.StoryDescription,
                    ContentRatingId = story.ContentRatingId ?? 1,
                    IsPrivate = story.IsPrivate,
                    UniverseId = story.UniverseId ?? 0,
                    UserId = story.UserId
                };

                var storyGenres = await _contentDataService.GetStoryGenresAsync(story.StoryId);
                var allGenres = await _miscDataService.GetGenresAsync();
                Genres = allGenres.Select(g => new GenreSelectionViewModel
                {
                    GenreId = g.GenreId,
                    GenreName = g.GenreName,
                    IsSelected = storyGenres.Contains(g.GenreId)
                }).ToList();

                var universes = await _contentDataService.GetRecordsAsync<Universe>("""SELECT "UniverseId", "UniverseName" FROM "Universes" ORDER BY "UniverseName" """);
                Universes = new SelectList(universes, "UniverseId", "UniverseName", Story.UniverseId);
            }
        }

        public IActionResult OnPostSearch()
        {
            return RedirectToPage(new { SearchStoryId });
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            if (Story == null || !ModelState.IsValid)
            {
                if (Story?.StoryId > 0)
                {
                    // If validation fails, we need to repopulate the dropdowns
                    await OnGetAsync();
                }
                return Page();
            }

            var model = new StoryInputModel
            {
                StoryId = Story.StoryId,
                StoryTitle = Story.Title,
                StoryDescription = Story.Description,
                ContentRatingId = Story.ContentRatingId,
                IsPrivate = Story.IsPrivate,
                UniverseId = Story.UniverseId == 0 ? null : Story.UniverseId
            };

            await _contentDataService.UpdateStoryAsync(model);
            var selectedGenreIds = Genres.Where(g => g.IsSelected).Select(g => g.GenreId).ToList();
            await _contentDataService.UpdateStoryGenresAsync(Story.StoryId, selectedGenreIds);

            IsSuccess = true;
            Message = "The story has been saved.";
            return RedirectToPage(new { SearchStoryId = Story.StoryId });
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            await _contentDataService.DeleteStoryAsync(id);
            TempData["IsSuccess"] = true;
            TempData["Message"] = "The story has been deleted.";
            return RedirectToPage("/Admin-Panel/Stories/Index");
        }
    }

    // Local ViewModel for this page
    public class StoryEditModel
    {
        public int StoryId { get; set; }
        public int UserId { get; set; }
        [Required]
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int ContentRatingId { get; set; }
        public bool IsPrivate { get; set; }
        public int? UniverseId { get; set; }
    }
}
