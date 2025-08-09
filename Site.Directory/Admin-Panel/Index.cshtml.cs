using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RoleplayersGuild.Site.Directory.Admin;

[Authorize(Policy = "IsStaff")]
public class IndexModel : PageModel
{
    // Properties to expose authorization results to the view
    public bool IsUserStaff { get; set; }
    public bool IsUserAdmin { get; set; }
    public bool IsUserSuperAdmin { get; set; }

    public IndexModel()
    {
    }

    public void OnGet()
    {
        // The [Authorize] attribute ensures the user is at least staff.
        // We can set IsUserStaff to true and then check for higher privileges.
        IsUserStaff = User.IsInRole("Staff") || User.IsInRole("Admin") || User.IsInRole("SuperAdmin");

        // Check for Admin or SuperAdmin roles to show admin-specific sections.
        IsUserAdmin = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");

        // Check for SuperAdmin role for super-admin-specific sections.
        IsUserSuperAdmin = User.IsInRole("SuperAdmin");
    }
}