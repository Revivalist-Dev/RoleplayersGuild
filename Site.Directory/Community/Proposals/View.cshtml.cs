using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using RoleplayersGuild.Site.Services.DataServices;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.Community.Proposals
{
    public class ViewProposalModel : PageModel
    {
        private readonly IMiscDataService _miscDataService;
        private readonly IContentDataService _contentDataService;
        private readonly IUserService _userService;

        public ViewProposalModel(IMiscDataService miscDataService, IContentDataService contentDataService, IUserService userService)
        {
            _miscDataService = miscDataService;
            _contentDataService = contentDataService;
            _userService = userService;
        }

        public ProposalWithDetails? Proposal { get; set; }
        public IEnumerable<string> Genres { get; set; } = Enumerable.Empty<string>();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (id == 0)
            {
                return RedirectToPage("./Search");
            }

            const string query = """SELECT * FROM "ProposalsWithDetails" WHERE "ProposalId" = @Id""";
            Proposal = await _contentDataService.GetRecordAsync<ProposalWithDetails>(query, new { Id = id });

            if (Proposal is null)
            {
                TempData["Message"] = "The requested proposal could not be found.";
                return RedirectToPage("./Search");
            }

            var allGenres = await _miscDataService.GetGenresAsync();
            var proposalGenreIds = (await _contentDataService.GetProposalGenresAsync(id)).ToHashSet();
            Genres = allGenres.Where(g => proposalGenreIds.Contains(g.GenreId)).Select(g => g.GenreName);

            ViewData["Title"] = Proposal.Title;

            return Page();
        }
    }
}
