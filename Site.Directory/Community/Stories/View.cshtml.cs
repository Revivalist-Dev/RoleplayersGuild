using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.Community.Stories
{
    public class ViewStoryModel : PageModel
    {
        private readonly IDataService _dataService;
        private readonly IUserService _userService;

        public ViewStoryModel(IDataService dataService, IUserService userService)
        {
            _dataService = dataService;
            _userService = userService;
        }

        public StoryWithDetails? Story { get; set; }
        public bool IsStaff { get; set; }
        public bool IsBlocked { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Story = await _dataService.GetStoryWithDetailsAsync(id);
            if (Story is null)
            {
                return NotFound();
            }

            var currentUserId = _userService.GetUserId(User);
            if (currentUserId > 0 && Story.UserId > 0)
            {
                IsBlocked = await _dataService.IsUserBlockedAsync(Story.UserId, currentUserId);
                if (IsBlocked)
                {
                    Story.StoryDescription = "[Content Hidden due to user block]";
                }
            }

            IsStaff = await _userService.IsCurrentUserStaffAsync();

            ViewData["Title"] = $"{Story.StoryTitle} — Role-Players Guild";
            ViewData["MetaDescription"] = $"Read \"{Story.StoryTitle}\" on RPG today!";

            return Page();
        }
    }
}