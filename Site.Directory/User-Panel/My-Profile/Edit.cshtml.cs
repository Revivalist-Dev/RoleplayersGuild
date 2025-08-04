using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;

namespace RoleplayersGuild.Site.Directory.User_Panel.My_Profile
{
    public class EditProfileModel : UserPanelBaseModel
    {
        // UPDATED: Constructor to match the new base class signature.
        public EditProfileModel(IDataService dataService, IUserService userService)
            : base(dataService, userService)
        {
        }

        [BindProperty]
        public string? AboutMe { get; set; }

        [BindProperty]
        public List<int> DisplayedBadgeIds { get; set; } = new();

        public List<BadgeSelectionViewModel> Badges { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            if (LoggedInUserId == 0) return Forbid();

            var user = await DataService.GetUserAsync(LoggedInUserId);
            if (user is null) return NotFound();

            AboutMe = user.AboutMe;

            var badgesData = await DataService.GetUserBadgesForEditingAsync(LoggedInUserId);
            Badges = badgesData.ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (LoggedInUserId == 0) return Forbid();

            if (AboutMe is not null && AboutMe.ToLower().Contains("<script"))
            {
                ModelState.AddModelError("AboutMe", "Script tags are not allowed in your profile.");
            }

            if (!ModelState.IsValid)
            {
                var badgesData = await DataService.GetUserBadgesForEditingAsync(LoggedInUserId);
                Badges = badgesData.ToList();
                return Page();
            }

            await DataService.UpdateUserAboutMeAsync(LoggedInUserId, AboutMe ?? string.Empty);
            await DataService.UpdateUserBadgeDisplayAsync(LoggedInUserId, DisplayedBadgeIds);

            TempData["Message"] = "Your profile has been updated successfully!";
            return RedirectToPage();
        }
    }
}