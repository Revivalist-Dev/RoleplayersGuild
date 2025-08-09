using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Services;
using RoleplayersGuild.Site.Services.DataServices;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.API
{
    public class NotificationCountsModel : PageModel
    {
        private readonly ICommunityDataService _communityDataService;
        private readonly IUserDataService _userDataService;
        private readonly IUserService _userService;

        public NotificationCountsModel(ICommunityDataService communityDataService, IUserDataService userDataService, IUserService userService)
        {
            _communityDataService = communityDataService;
            _userDataService = userDataService;
            _userService = userService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // The GetUserId method from ICookieService should already return an int
            // so no direct casing change is needed here, but keeping it for context.
            var userId = _userService.GetUserId(User);
            if (userId == 0)
            {
                return new JsonResult(new { });
            }

            // These methods in IDataService will internally call the database.
            // The SQL within those IDataService methods will need to be updated.
            var threadCount = await _communityDataService.GetUnreadThreadCountAsync(userId);
            var imageCommentCount = await _userDataService.GetUnreadImageCommentCountAsync(userId);

            return new JsonResult(new
            {
                threadCount,
                imageCommentCount
            });
        }
    }
}
