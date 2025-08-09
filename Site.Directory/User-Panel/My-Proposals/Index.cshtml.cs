using Microsoft.AspNetCore.Mvc;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using RoleplayersGuild.Site.Services.DataServices;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.User_Panel.My_Proposals
{
    public class IndexMyProposalsModel : UserPanelBaseModel
    {
        private readonly IContentDataService _contentDataService;
        public List<ProposalWithDetails> Proposals { get; set; } = new();

        public IndexMyProposalsModel(
            ICharacterDataService characterDataService,
            ICommunityDataService communityDataService,
            IMiscDataService miscDataService,
            IUserService userService,
            IContentDataService contentDataService)
            : base(characterDataService, communityDataService, miscDataService, userService)
        {
            _contentDataService = contentDataService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            Proposals = (await _contentDataService.GetUserProposalsAsync(LoggedInUserId)).ToList();
            return Page();
        }
    }
}
