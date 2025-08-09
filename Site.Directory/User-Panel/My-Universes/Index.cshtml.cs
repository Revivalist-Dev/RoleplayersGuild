using Microsoft.AspNetCore.Mvc;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using RoleplayersGuild.Site.Services.DataServices;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.User_Panel.My_Universes
{
    public class IndexMyUniversesModel : UserPanelBaseModel
    {
        private readonly IUniverseDataService _universeDataService;
        public List<UniverseWithDetails> Universes { get; set; } = new();

        public IndexMyUniversesModel(
            ICharacterDataService characterDataService,
            ICommunityDataService communityDataService,
            IMiscDataService miscDataService,
            IUserService userService,
            IUniverseDataService universeDataService)
            : base(characterDataService, communityDataService, miscDataService, userService)
        {
            _universeDataService = universeDataService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            Universes = (await _universeDataService.GetUserUniversesAsync(LoggedInUserId)).ToList();
            return Page();
        }
    }
}
