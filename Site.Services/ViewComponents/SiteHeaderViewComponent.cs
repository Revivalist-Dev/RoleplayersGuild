using Microsoft.AspNetCore.Mvc;

namespace RoleplayersGuild.Site.Services.ViewComponents
{
    public class SiteHeaderViewComponent : ViewComponent
    {
        // No services need to be injected for this simple component.
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}