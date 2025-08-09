using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using RoleplayersGuild.Site.Services.DataServices;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.Community.Proposals
{
    public class SearchModel : PageModel
    {
        private readonly IContentDataService _contentDataService;

        public SearchModel(IContentDataService contentDataService)
        {
            _contentDataService = contentDataService;
        }

        [BindProperty(SupportsGet = true)]
        public List<int> SelectedGenreIds { get; set; } = new();

        [BindProperty(SupportsGet = true, Name = "p")]
        public int CurrentPage { get; set; } = 1;

        public PagedResult<ProposalWithDetails>? ProposalsResult { get; set; }

        public async Task OnGetAsync()
        {
            const int pageSize = 20;
            ProposalsResult = await _contentDataService.SearchProposalsAsync(CurrentPage, pageSize, SelectedGenreIds);
        }
    }
}
