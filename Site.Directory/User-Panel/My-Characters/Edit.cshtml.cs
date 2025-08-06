using Microsoft.AspNetCore.Mvc;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.User_Panel.My_Characters
{
    // This Page Model is now much simpler. Its only job is to load the page 
    // and provide the characterId to the React component.
    public class EditMyCharactersModel : UserPanelBaseModel
    {
        private readonly IUserService _userService;

        public EditMyCharactersModel(IDataService dataService, IUserService userService)
            : base(dataService, userService)
        {
            // We only need the UserService for the initial ownership check.
            _userService = userService;
        }

        // We only need a simple model to pass the CharacterId to the view.
        // Using CharacterInputModel is fine, as it just needs the one property.
        [BindProperty(SupportsGet = true)]
        public CharacterInputModel Input { get; set; } = new();

        public bool IsNewCharacter => Input.CharacterId == 0;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            var userId = _userService.GetUserId(User);
            if (userId == 0) return RedirectToPage("/");

            if (id.HasValue && id > 0)
            {
                // Verify the user owns this character before allowing them to see the editor.
                var character = await DataService.GetCharacterForEditAsync(id.Value, userId);
                if (character is null)
                {
                    return Forbid(); // Or Redirect if they don't own it
                }
                Input.CharacterId = character.CharacterId;
            }
            else
            {
                // This is a new character.
                Input.CharacterId = 0;
            }

            return Page();
        }
    }
}