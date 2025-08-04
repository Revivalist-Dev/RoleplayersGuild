using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

// Corrected: Namespace now matches folder structure
namespace RoleplayersGuild.Site.Directory.Admin_Panel.Characters
{
    // [Authorize(Policy = "IsStaff")]
    public class IndexModel : PageModel
    {
        private readonly IDataService _dataService;
        private readonly IUserService _userService;
        private readonly ICookieService _cookieService;

        public IndexModel(IDataService dataService, IUserService userService, ICookieService cookieService)
        {
            _dataService = dataService;
            _userService = userService;
            _cookieService = cookieService;
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
                var character = await _dataService.GetCharacterAsync(SearchCharacterId.Value);
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
            await _dataService.ExecuteAsync("""UPDATE "Characters" SET "TypeId" = @TypeId WHERE "CharacterId" = @CharacterId""", new { Character.TypeId, Character.CharacterId });

            IsSuccess = true;
            Message = "Character type has been changed.";
            return RedirectToPage(new { SearchCharacterId = Character.CharacterId });
        }

        public async Task<IActionResult> OnPostMarkForReviewAsync(int id)
        {
            var adminUserId = _cookieService.GetUserId();
            await _userService.MarkCharacterForReviewAsync(id, adminUserId);

            IsSuccess = true;
            Message = "Character has been marked for review.";
            return RedirectToPage(new { SearchCharacterId = id });
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            await _dataService.DeleteCharacterAsync(id);
            TempData["IsSuccess"] = true;
            TempData["Message"] = "The character has been deleted.";
            return RedirectToPage("/Admin-Panel/Characters/Index");
        }

        private async Task PopulateSelectListsAsync(int selectedType)
        {
            // Corrected: SQL query uses PascalCase
            var types = await _dataService.GetRecordsAsync<CharacterType>("""SELECT "TypeId", "TypeName" FROM "CharacterType" ORDER BY "TypeName" """);
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