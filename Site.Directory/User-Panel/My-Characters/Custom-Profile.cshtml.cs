using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;

namespace RoleplayersGuild.Site.Directory.User_Panel.My_Characters
{
    public class CustomProfileModel : UserPanelBaseModel
    {
        // NOTE: Removed redundant _dataService field. The 'DataService' property from the base class will be used.

        // UPDATED: Constructor now injects IUserService
        public CustomProfileModel(IDataService dataService, IUserService userService)
            : base(dataService, userService)
        {
        }

        [BindProperty]
        public ProfileInputModel Input { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            // Use the DataService property from the base class
            var character = await DataService.GetCharacterForEditAsync(id, LoggedInUserId);
            if (character is null) return Forbid();

            Input = new ProfileInputModel
            {
                CharacterId = id,
                ProfileCSS = character.ProfileCss,
                ProfileHTML = character.ProfileHtml,
                IsEnabled = character.CustomProfileEnabled
            };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            // Use the DataService property from the base class
            var character = await DataService.GetCharacterForEditAsync(Input.CharacterId, LoggedInUserId);
            if (character is null) return Forbid();

            if (ContainsForbiddenTags(Input.ProfileCSS) || ContainsForbiddenTags(Input.ProfileHTML))
            {
                ModelState.AddModelError(string.Empty, "Your code contains forbidden tags like <script>, <style>, or <meta>. Please remove them.");
                return Page();
            }

            // Use the DataService property from the base class
            await DataService.UpdateCharacterCustomProfileAsync(Input.CharacterId, Input.ProfileCSS, Input.ProfileHTML, Input.IsEnabled);
            TempData["Message"] = "Your custom profile has been saved!";
            return RedirectToPage(new { id = Input.CharacterId });
        }

        private bool ContainsForbiddenTags(string? content)
        {
            if (string.IsNullOrEmpty(content)) return false;
            var upperContent = content.ToUpperInvariant();
            return upperContent.Contains("</STYLE>") ||
                   upperContent.Contains("<SCRIPT") ||
                   upperContent.Contains("<META");
        }
    }
}