// File: Site.Directory/Api/NotificationCounts.cshtml.cs

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Services;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.Api;

/// <summary>
/// A modern API endpoint to retrieve notification counts for the logged-in user.
/// This page model responds to GET requests at the URL: /Api/NotificationCounts
/// </summary>
public class NotificationCountsModel : PageModel
{
    private readonly IDataService _dataService;
    private readonly ICookieService _cookieService;

    public NotificationCountsModel(IDataService dataService, ICookieService cookieService)
    {
        _dataService = dataService;
        _cookieService = cookieService;
    }

    /// <summary>
    /// Handles GET requests and returns notification counts as a JSON object.
    /// </summary>
    public async Task<IActionResult> OnGetAsync()
    {
        var userId = _cookieService.GetUserId();
        if (userId == 0)
        {
            // Return a 401 Unauthorized error if the user is not logged in.
            return Unauthorized();
        }

        // Fetch both counts in parallel for better performance.
        var threadsTask = _dataService.GetUnreadThreadCountAsync(userId);
        var commentsTask = _dataService.GetUnreadImageCommentCountAsync(userId);

        await Task.WhenAll(threadsTask, commentsTask);

        var result = new
        {
            unreadThreads = threadsTask.Result,
            unreadImageComments = commentsTask.Result
        };

        return new JsonResult(result);
    }
}
