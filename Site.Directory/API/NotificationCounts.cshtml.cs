// File: Site.Directory/Api/NotificationCounts.cshtml.cs

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Services;
using RoleplayersGuild.Site.Services.DataServices;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.Api;

/// <summary>
/// A modern API endpoint to retrieve notification counts for the logged-in user.
/// This page model responds to GET requests at the URL: /Api/NotificationCounts
/// </summary>
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

    /// <summary>
    /// Handles GET requests and returns notification counts as a JSON object.
    /// </summary>
    public async Task<IActionResult> OnGetAsync()
    {
        var userId = _userService.GetUserId(User);
        if (userId == 0)
        {
            // Return a 401 Unauthorized error if the user is not logged in.
            return Unauthorized();
        }

        // Fetch both counts in parallel for better performance.
        var threadsTask = _communityDataService.GetUnreadThreadCountAsync(userId);
        var commentsTask = _userDataService.GetUnreadImageCommentCountAsync(userId);

        await Task.WhenAll(threadsTask, commentsTask);

        var result = new
        {
            unreadThreads = threadsTask.Result,
            unreadImageComments = commentsTask.Result
        };

        return new JsonResult(result);
    }
}
