using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

// Corrected: Namespace now matches folder structure
namespace RoleplayersGuild.Site.Directory.Admin_Panel.Universes
{
    // [Authorize(Policy = "IsStaff")]
    public class IndexModel : PageModel
    {
        private readonly IDataService _dataService;

        public IndexModel(IDataService dataService)
        {
            _dataService = dataService;
        }

        [TempData] public string? Message { get; set; }
        [TempData] public bool IsSuccess { get; set; }

        public IEnumerable<UniverseListingViewModel> AllUniverses { get; set; } = new List<UniverseListingViewModel>();

        [BindProperty] public UniverseEditModel? Universe { get; set; }
        [BindProperty] public List<GenreSelectionViewModel> Genres { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            // Corrected: SQL query uses PascalCase and || for concatenation
            const string listSql = """
                SELECT "UniverseId", CASE WHEN "StatusId" = 1 THEN '[PA] - ' || "UniverseName" ELSE "UniverseName" END AS "UniverseName" 
                FROM "Universes" ORDER BY "StatusId", "UniverseName"
                """;
            AllUniverses = await _dataService.GetRecordsAsync<UniverseListingViewModel>(listSql);

            if (id.HasValue)
            {
                // Corrected: SQL query uses PascalCase
                const string detailSql = """
                    SELECT u.*, o."Username" AS "OwnerUsername", s."Username" AS "SubmittedByUsername" 
                    FROM "Universes" u 
                    LEFT JOIN "Users" o ON u."UniverseOwnerId" = o."UserId"
                    LEFT JOIN "Users" s ON u."SubmittedById" = s."UserId"
                    WHERE u."UniverseId" = @id
                    """;
                var universe = await _dataService.GetRecordAsync<UniverseWithUsernames>(detailSql, new { id });

                if (universe == null) return NotFound();

                Universe = new UniverseEditModel
                {
                    UniverseId = universe.UniverseId,
                    Name = universe.UniverseName,
                    Description = universe.UniverseDescription,
                    IsApproved = universe.StatusId == 2,
                    SourceTypeId = universe.SourceTypeId ?? 1,
                    ContentRatingId = universe.ContentRatingId ?? 1,
                    OwnerUserId = universe.UniverseOwnerId,
                    OwnerUsername = universe.OwnerUsername,
                    SubmittedByUserId = universe.SubmittedById,
                    SubmittedByUsername = universe.SubmittedByUsername
                };

                var universeGenres = await _dataService.GetUniverseGenresAsync(id.Value);
                var allGenres = await _dataService.GetGenresAsync();
                Genres = allGenres.Select(g => new GenreSelectionViewModel
                {
                    GenreId = g.GenreId,
                    GenreName = g.GenreName,
                    IsSelected = universeGenres.Contains(g.GenreId)
                }).ToList();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            if (Universe == null || !ModelState.IsValid) return Page();

            // Corrected: SQL query uses PascalCase
            const string updateSql = """
                UPDATE "Universes" SET "UniverseName" = @Name, "UniverseDescription" = @Description, 
                "SourceTypeId" = @SourceTypeId, "ContentRatingId" = @ContentRatingId, "StatusId" = @StatusId 
                WHERE "UniverseId" = @UniverseId
                """;

            await _dataService.ExecuteAsync(updateSql, new
            {
                Universe.UniverseId,
                Universe.Name,
                Universe.Description,
                Universe.SourceTypeId,
                Universe.ContentRatingId,
                StatusId = Universe.IsApproved ? 2 : 1
            });

            var selectedGenreIds = Genres.Where(g => g.IsSelected).Select(g => g.GenreId).ToList();
            await _dataService.UpdateUniverseGenresAsync(Universe.UniverseId, selectedGenreIds);

            IsSuccess = true;
            Message = "The changes have been saved.";
            return RedirectToPage(new { id = Universe.UniverseId });
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            await _dataService.DeleteUniverseAsync(id);
            TempData["IsSuccess"] = true;
            TempData["Message"] = "The universe has been deleted.";
            return RedirectToPage("./Index");
        }
    }

    // Local ViewModels for this page
    public class UniverseListingViewModel { public int UniverseId { get; set; } public string UniverseName { get; set; } = ""; }
    public class UniverseEditModel { public int UniverseId { get; set; } [Required] public string? Name { get; set; } public string? Description { get; set; } public bool IsApproved { get; set; } public int SourceTypeId { get; set; } public int ContentRatingId { get; set; } public int OwnerUserId { get; set; } public string? OwnerUsername { get; set; } public int? SubmittedByUserId { get; set; } public string? SubmittedByUsername { get; set; } }
    public class GenreSelectionViewModel { public int GenreId { get; set; } public string GenreName { get; set; } = ""; public bool IsSelected { get; set; } }
    public class UniverseWithUsernames : Universe { public string? OwnerUsername { get; set; } public string? SubmittedByUsername { get; set; } }
}