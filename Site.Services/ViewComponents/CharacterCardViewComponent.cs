using Microsoft.AspNetCore.Mvc;
using RoleplayersGuild.Site.Model;

namespace RoleplayersGuild.Site.Services.ViewComponents
{
    public class CharacterCardViewComponent : ViewComponent
    {
        // The IImageService dependency has been removed.
        public CharacterCardViewComponent() { }

        public IViewComponentResult Invoke(CharactersForListing character, bool showAdminControls = false)
        {
            // The component no longer processes the URLs. It trusts the model it receives.
            ViewData["showAdminControls"] = showAdminControls;
            return View("~/Site.Directory/Shared/Components/CharacterCard/Default.cshtml", character);
        }
    }
}