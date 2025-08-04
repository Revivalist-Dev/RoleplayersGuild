using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RoleplayersGuild.Site.Directory.User_Panel.My_Characters
{
    public class CreateModel : PageModel
    {
        public IActionResult OnGet()
        {
            // The Edit page handles both creating and editing.
            // Redirect to the Edit page without an ID to enter "create" mode.
            return RedirectToPage("./Edit");
        }
    }
}