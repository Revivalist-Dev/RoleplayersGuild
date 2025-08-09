using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using RoleplayersGuild.Site.Services.DataServices;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

// Corrected: Namespace now matches folder structure
namespace RoleplayersGuild.Site.Directory.Admin_Panel.Characters
{
    // [Authorize(Policy = "IsStaff")]
    public class IndexModel : PageModel
    {
        private readonly ICharacterDataService _characterDataService;
        private readonly IUserService _userService;
        public IndexModel(ICharacterDataService characterDataService, IUserService userService)
        {
            _characterDataService = characterDataService;
            _userService = userService;
        }

        [TempData]
        public string? Message { get; set; }

        [TempData]
        public bool IsSuccess { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? SearchCharacterId { get; set; }

        [BindProperty]
        public CharacterEditModel? Character { get; set; }

        public SelectList? CharacterTypes { get; set; }

        public async Task OnGetAsync()
        {
            if (SearchCharacterId.HasValue)
            {
                var character = await _characterDataService.GetCharacterAsync(SearchCharacterId.Value);
                if (character != null)
                {
                    Character = new CharacterEditModel
                    {
                        CharacterId = character.CharacterId,
                        CharacterDisplayName = character.CharacterDisplayName,
                        TypeId = character.TypeId,
                        UserId = character.UserId
                    };
                    await PopulateSelectListsAsync(character.TypeId);
                }
                else
                {
                    IsSuccess = false;
                    Message = "No character found with that ID.";
                }
            }
        }

        public async Task<IActionResult> OnPostSaveChangesAsync()
        {
            if (Character == null || !ModelState.IsValid)
            {
                await PopulateSelectListsAsync(Character?.TypeId ?? 0);
                return Page();
            }

            // Corrected: SQL query uses PascalCase
            await _characterDataService.ExecuteAsync("""UPDATE "Characters" SET "TypeId" = @TypeId WHERE "CharacterId" = @CharacterId""", new { Character.TypeId, Character.CharacterId });

            IsSuccess = true;
            Message = "Character type has been changed.";
            return RedirectToPage(new { SearchCharacterId = Character.CharacterId });
        }

        public async Task<IActionResult> OnPostMarkForReviewAsync(int id)
        {
            var adminUserId = _userService.GetUserId(User);
            await _userService.MarkCharacterForReviewAsync(id, adminUserId);

            IsSuccess = true;
            Message = "Character has been marked for review.";
            return RedirectToPage(new { SearchCharacterId = id });
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            await _characterDataService.DeleteCharacterAsync(id);
            TempData["IsSuccess"] = true;
            TempData["Message"] = "The character has been deleted.";
            return RedirectToPage("/Admin-Panel/Characters/Index");
        }

        private async Task PopulateSelectListsAsync(int selectedType)
        {
            // Corrected: SQL query uses PascalCase
            var types = await _characterDataService.GetRecordsAsync<CharacterType>("""SELECT "TypeId", "TypeName" FROM "CharacterType" ORDER BY "TypeName" """);
            CharacterTypes = new SelectList(types, "TypeId", "TypeName", selectedType);
        }
    }

    public class CharacterEditModel
    {
        public int CharacterId { get; set; }
        public string? CharacterDisplayName { get; set; }
        public int UserId { get; set; }
        [Required]
        public int TypeId { get; set; }
    }
}
