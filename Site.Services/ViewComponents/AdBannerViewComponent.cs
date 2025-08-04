using Microsoft.AspNetCore.Mvc;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Services.ViewComponents
{
    // A simple model to pass ad data to the view
    public class AdBannerViewModel
    {
        public string? ImageUrl { get; set; }
        public string? LinkUrl { get; set; }
    }

    public class AdBannerViewComponent : ViewComponent
    {
        private readonly IDataService _dataService;
        private readonly IUserService _userService; // Using IUserService to check membership

        public AdBannerViewComponent(IDataService dataService, IUserService userService)
        {
            _dataService = dataService;
            _userService = userService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Check if the user is a paying member. If so, don't show an ad.
            if (await _userService.CurrentUserHasActiveMembershipAsync())
            {
                return Content(string.Empty); // Return empty content, hiding the ad
            }

            // Fetch a random ad from the database (AdType 1 for top banner)
            var ad = await _dataService.GetRandomAdAsync(1);

            if (ad is null)
            {
                // You could also render a default Google AdSense block here if no database ad is found
                return Content(string.Empty); // Or return nothing if no ad is available
            }

            var model = new AdBannerViewModel
            {
                ImageUrl = ad.AdImageUrl,
                LinkUrl = ad.AdLink
            };

            return View(model);
        }
    }
}