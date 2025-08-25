using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Services.ViewComponents
{
    public class AdBannerViewModel
    {
        public bool IsDevelopment { get; set; }
        public string AdType { get; set; } = "Default";
    }

    public class AdBannerViewComponent : ViewComponent
    {
        private readonly IUserService _userService;
        private readonly IWebHostEnvironment _env;

        public AdBannerViewComponent(IUserService userService, IWebHostEnvironment env)
        {
            _userService = userService;
            _env = env;
        }

        public async Task<IViewComponentResult> InvokeAsync(string adType = "Default")
        {
            // If the user has an active membership, don't show any ad.
            if (await _userService.CurrentUserHasActiveMembershipAsync())
            {
                return Content(string.Empty);
            }

            // Otherwise, prepare the model for the view.
            var model = new AdBannerViewModel
            {
                IsDevelopment = _env.IsDevelopment(),
                AdType = adType
            };

            return View(model);
        }
    }
}
