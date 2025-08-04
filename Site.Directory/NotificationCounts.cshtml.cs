using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Services;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.API
{
    public class NotificationCountsModel : PageModel
    {
        private readonly IDataService _dataService;
        private readonly ICookieService _cookieService;

        public NotificationCountsModel(IDataService dataService, ICookieService cookieService)
        {
            _dataService = dataService;
            _cookieService = cookieService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // The GetUserId method from ICookieService should already return an int
            // so no direct casing change is needed here, but keeping it for context.
            var userId = _cookieService.GetUserId();
            if (userId == 0)
            {
                return new JsonResult(new { });
            }

            // These methods in IDataService will internally call the database.
            // The SQL within those IDataService methods will need to be updated.
            var threadCount = await _dataService.GetUnreadThreadCountAsync(userId);
            var imageCommentCount = await _dataService.GetUnreadImageCommentCountAsync(userId);

            return new JsonResult(new
            {
                threadCount,
                imageCommentCount
            });
        }
    }
}