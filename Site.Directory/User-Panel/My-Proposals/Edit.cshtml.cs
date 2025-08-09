using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using RoleplayersGuild.Site.Services.DataServices;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.User_Panel.My_Proposals
{
    public class EditMyProposalsModel : UserPanelBaseModel
    {
        private readonly IContentDataService _contentDataService;

        [BindProperty]
        public ProposalInputModel Input { get; set; } = new();

        public SelectList Ratings { get; set; } = new(Enumerable.Empty<SelectListItem>());
        public SelectList Statuses { get; set; } = new(Enumerable.Empty<SelectListItem>());
        public List<GenreSelectionViewModel> GenreSelection { get; set; } = new();

        public bool IsNew => Input.ProposalId == 0;

        public EditMyProposalsModel(
            ICharacterDataService characterDataService,
            ICommunityDataService communityDataService,
            IMiscDataService miscDataService,
            IUserService userService,
            IContentDataService contentDataService)
            : base(characterDataService, communityDataService, miscDataService, userService)
        {
            _contentDataService = contentDataService;
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id.HasValue) // Editing
            {
                var proposal = await _contentDataService.GetProposalAsync(id.Value);
                if (proposal is null || proposal.UserId != LoggedInUserId) return Forbid();

                Input = new ProposalInputModel(proposal)
                {
                    SelectedGenreIds = (await _contentDataService.GetProposalGenresAsync(id.Value)).ToList()
                };
            }
            else // New
            {
                Input = new ProposalInputModel { StatusId = 1 }; // Default to "Open"
            }

            await PopulateSelectListsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await PopulateSelectListsAsync();
                return Page();
            }

            int proposalId = await _contentDataService.UpsertProposalAsync(Input, LoggedInUserId);
            await _contentDataService.UpdateProposalGenresAsync(proposalId, Input.SelectedGenreIds);

            TempData["Message"] = "Proposal saved successfully!";
            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var proposal = await _contentDataService.GetProposalAsync(id);
            if (proposal is null || proposal.UserId != LoggedInUserId) return Forbid();

            await _contentDataService.DeleteProposalAsync(id, LoggedInUserId);
            TempData["Message"] = "Proposal deleted successfully.";
            return RedirectToPage("./Index");
        }
        private async Task PopulateSelectListsAsync()
        {
            Ratings = new SelectList(await _miscDataService.GetContentRatingsAsync(), "ContentRatingId", "ContentRatingName", Input.ContentRatingId);
            Statuses = new SelectList(await _contentDataService.GetProposalStatusesAsync(), "StatusId", "StatusName", Input.StatusId);

            var allGenres = await _miscDataService.GetGenresAsync();
            GenreSelection = allGenres.Select(g => new GenreSelectionViewModel
            {
                GenreId = g.GenreId,
                GenreName = g.GenreName,
                IsSelected = Input.SelectedGenreIds.Contains(g.GenreId)
            }).ToList();
        }
    }
}
