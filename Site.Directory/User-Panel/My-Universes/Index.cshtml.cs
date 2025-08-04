using Microsoft.AspNetCore.Mvc;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.User_Panel.My_Universes
{
    public class IndexMyUniversesModel : UserPanelBaseModel
    {
        public List<UniverseWithDetails> Universes { get; set; } = new();

        // UPDATED: Constructor to match the new base class signature.
        public IndexMyUniversesModel(IDataService dataService, IUserService userService)
            : base(dataService, userService) { }

        public async Task<IActionResult> OnGetAsync()
        {
            Universes = (await DataService.GetUserUniversesAsync(LoggedInUserId)).ToList();
            return Page();
        }
    }
}