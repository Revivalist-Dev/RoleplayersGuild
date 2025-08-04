using Microsoft.AspNetCore.Mvc;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.User_Panel.My_Characters
{
    public class IndexMyCharactersModel : UserPanelBaseModel
    {
        public List<CharactersForListing> Characters { get; set; } = new();

        public IndexMyCharactersModel(IDataService dataService, IUserService userService)
            : base(dataService, userService) { }

        public async Task<IActionResult> OnGetAsync()
        {
            // CORRECTED: Call the new, specific method that handles image URL processing.
            var characters = await DataService.GetUserCharactersForListingAsync(LoggedInUserId);
            Characters = characters.ToList();
            return Page();
        }
    }
}