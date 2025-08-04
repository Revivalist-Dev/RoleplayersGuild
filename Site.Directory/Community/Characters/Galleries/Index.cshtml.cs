using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

// Corrected: Namespace now matches the folder structure
namespace RoleplayersGuild.Site.Directory.Admin_Panel
{
    [Authorize(Policy = "IsStaff")] // Apply the base policy to the entire Admin-Panel
    public class IndexModel : PageModel
    {
        private readonly IAuthorizationService _authorizationService;

        // Properties to expose authorization results to the view
        public bool IsUserStaff { get; set; }
        public bool IsUserAdmin { get; set; }
        public bool IsUserSuperAdmin { get; set; }

        public IndexModel(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }

        public async Task OnGetAsync()
        {
            // Perform authorization checks using the injected service and the current User
            IsUserStaff = (await _authorizationService.AuthorizeAsync(User, "IsStaff")).Succeeded;
            IsUserAdmin = (await _authorizationService.AuthorizeAsync(User, "IsAdmin")).Succeeded;
            IsUserSuperAdmin = (await _authorizationService.AuthorizeAsync(User, "IsSuperAdmin")).Succeeded;
        }
    }
}