using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.Community.Proposals
{
    public class ViewProposalModel : PageModel
    {
        private readonly IDataService _dataService;
        private readonly IUserService _userService;

        public ViewProposalModel(IDataService dataService, IUserService userService)
        {
            _dataService = dataService;
            _userService = userService;
        }

        public ProposalWithDetails? Proposal { get; set; }
        public IEnumerable<string> Genres { get; set; } = Enumerable.Empty<string>();
        public bool IsStaff { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (id == 0)
            {
                return RedirectToPage("./Search");
            }

            const string query = """SELECT * FROM "ProposalsWithDetails" WHERE "ProposalId" = @Id""";
            Proposal = await _dataService.GetRecordAsync<ProposalWithDetails>(query, new { Id = id });

            if (Proposal is null)
            {
                TempData["Message"] = "The requested proposal could not be found.";
                return RedirectToPage("./Search");
            }

            var allGenres = await _dataService.GetGenresAsync();
            var proposalGenreIds = (await _dataService.GetProposalGenresAsync(id)).ToHashSet();
            Genres = allGenres.Where(g => proposalGenreIds.Contains(g.GenreId)).Select(g => g.GenreName);

            ViewData["Title"] = Proposal.Title;
            IsStaff = await _userService.IsCurrentUserStaffAsync();

            return Page();
        }
    }
}