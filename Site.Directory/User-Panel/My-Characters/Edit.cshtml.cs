using Microsoft.AspNetCore.Mvc;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using RoleplayersGuild.Site.Services.DataServices;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.User_Panel.My_Characters
{
    public class EditMyCharactersModel : UserPanelBaseModel
    {
        public EditMyCharactersModel(
            ICharacterDataService characterDataService,
            ICommunityDataService communityDataService,
            IMiscDataService miscDataService,
            IUserService userService)
            : base(characterDataService, communityDataService, miscDataService, userService)
        {
        }

        [BindProperty(SupportsGet = true)]
        public CharacterInputModel Input { get; set; } = new();

        public bool IsNewCharacter => Input.CharacterId == 0;
        public string? InitialAvatarUrl { get; private set; }
        public string? InitialCardUrl { get; private set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id.HasValue && id > 0)
            {
                var character = await _characterDataService.GetCharacterWithDetailsAsync(id.Value);
                if (character is null)
                {
                    return Forbid();
                }
                Input.CharacterId = character.CharacterId;
                InitialAvatarUrl = Url.Content(character.AvatarImageUrl ?? "");
                InitialCardUrl = Url.Content(character.CardImageUrl ?? "");
            }
            else
            {
                Input.CharacterId = 0;
                InitialAvatarUrl = Url.Content("~/images/Defaults/NewAvatar.png");
                InitialCardUrl = Url.Content("~/images/Defaults/NewCharacter.png");
            }

            return Page();
        }
    }
}
