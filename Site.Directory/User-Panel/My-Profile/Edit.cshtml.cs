using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using RoleplayersGuild.Site.Services.DataServices;

namespace RoleplayersGuild.Site.Directory.User_Panel.My_Profile
{
    public class EditProfileModel : UserPanelBaseModel
    {
        private readonly IUserDataService _userDataService;

        [BindProperty]
        public string? AboutMe { get; set; }

        [BindProperty]
        public List<int> DisplayedBadgeIds { get; set; } = new();

        public List<BadgeSelectionViewModel> Badges { get; set; } = new();

        public EditProfileModel(ICharacterDataService characterDataService, ICommunityDataService communityDataService, IMiscDataService miscDataService, IUserService userService, IUserDataService userDataService)
            : base(characterDataService, communityDataService, miscDataService, userService)
        {
            _userDataService = userDataService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (LoggedInUserId == 0) return Forbid();

            var user = await _userDataService.GetUserAsync(LoggedInUserId);
            if (user is null) return NotFound();

            AboutMe = user.AboutMe;

            var badgesData = await _userDataService.GetUserBadgesForEditingAsync(LoggedInUserId);
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
                var badgesData = await _userDataService.GetUserBadgesForEditingAsync(LoggedInUserId);
                Badges = badgesData.ToList();
                return Page();
            }

            await _userDataService.UpdateUserAboutMeAsync(LoggedInUserId, AboutMe ?? string.Empty);
            await _userDataService.UpdateUserBadgeDisplayAsync(LoggedInUserId, DisplayedBadgeIds);

            TempData["Message"] = "Your profile has been updated successfully!";
            return RedirectToPage();
        }
    }
}
