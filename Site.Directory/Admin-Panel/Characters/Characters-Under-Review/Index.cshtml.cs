using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Services.DataServices;

// Corrected: Namespace now matches folder structure
namespace RoleplayersGuild.Site.Directory.Admin_Panel.Characters.Characters_Under_Review
{
    // [Authorize(Policy = "IsStaff")]
    public class IndexModel : PageModel
    {
        private readonly ICharacterDataService _characterDataService;
        private readonly IUserDataService _userDataService;

        public IndexModel(ICharacterDataService characterDataService, IUserDataService userDataService)
        {
            _characterDataService = characterDataService;
            _userDataService = userDataService;
        }

        public IEnumerable<CharactersForListing> CharactersUnderReview { get; set; } = new List<CharactersForListing>();
        public Character? SelectedCharacter { get; set; }
        public int SelectedCharacterUserId { get; set; }

        public async Task OnGetAsync(int? id)
        {
            // Corrected: SQL query uses PascalCase
            CharactersUnderReview = await _characterDataService.GetRecordsAsync<CharactersForListing>("""SELECT "CharacterId", "CharacterDisplayName" FROM "Characters" WHERE "CharacterStatusId" = 2 ORDER BY "CharacterId" """);

            if (id.HasValue)
            {
                SelectedCharacter = await _characterDataService.GetCharacterAsync(id.Value);
                if (SelectedCharacter != null)
                {
                    SelectedCharacterUserId = await _userDataService.GetUserIdFromCharacterAsync(id.Value);
                }
            }
        }

        public async Task<IActionResult> OnPostUnlockCharacterAsync(int id)
        {
            // Corrected: SQL query uses PascalCase
            await _characterDataService.ExecuteAsync("""UPDATE "Characters" SET "CharacterStatusId" = 1 WHERE "CharacterId" = @CharacterId""", new { CharacterId = id });

            return RedirectToPage("/Community/Characters/View", new { id });
        }
    }
}
