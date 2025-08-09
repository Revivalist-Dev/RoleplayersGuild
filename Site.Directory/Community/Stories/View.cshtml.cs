using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using RoleplayersGuild.Site.Services.DataServices;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.Community.Stories
{
    public class ViewStoryModel : PageModel
    {
        private readonly IContentDataService _contentDataService;
        private readonly IUserDataService _userDataService;
        private readonly IUserService _userService;

        public ViewStoryModel(IContentDataService contentDataService, IUserDataService userDataService, IUserService userService)
        {
            _contentDataService = contentDataService;
            _userDataService = userDataService;
            _userService = userService;
        }

        public StoryWithDetails? Story { get; set; }
        public bool IsBlocked { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Story = await _contentDataService.GetStoryWithDetailsAsync(id);
            if (Story is null)
            {
                return NotFound();
            }

            var currentUserId = _userService.GetUserId(User);
            if (currentUserId > 0 && Story.UserId > 0)
            {
                IsBlocked = await _userDataService.IsUserBlockedAsync(Story.UserId, currentUserId);
                if (IsBlocked)
                {
                    Story.StoryDescription = "[Content Hidden due to user block]";
                }
            }


            ViewData["Title"] = $"{Story.StoryTitle} — Role-Players Guild";
            ViewData["MetaDescription"] = $"Read \"{Story.StoryTitle}\" on RPG today!";

            return Page();
        }
    }
}
