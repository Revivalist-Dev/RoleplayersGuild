using Microsoft.AspNetCore.Mvc;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.User_Panel.My_Proposals
{
    public class IndexMyProposalsModel : UserPanelBaseModel
    {
        public List<ProposalWithDetails> Proposals { get; set; } = new();

        // UPDATED: Constructor to match the new base class signature.
        public IndexMyProposalsModel(IDataService dataService, IUserService userService)
            : base(dataService, userService) { }

        public async Task<IActionResult> OnGetAsync()
        {
            Proposals = (await DataService.GetUserProposalsAsync(LoggedInUserId)).ToList();
            return Page();
        }
    }
}