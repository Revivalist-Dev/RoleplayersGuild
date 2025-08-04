using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.User_Panel.My_Proposals
{
    public class EditMyProposalsModel : UserPanelBaseModel
    {
        private readonly IDataService _dataService;
        private readonly IUserService _userService;

        [BindProperty]
        public ProposalInputModel Input { get; set; } = new();

        public SelectList Ratings { get; set; } = new(Enumerable.Empty<SelectListItem>());
        public SelectList Statuses { get; set; } = new(Enumerable.Empty<SelectListItem>());
        public List<GenreSelectionViewModel> GenreSelection { get; set; } = new();

        public bool IsNew => Input.ProposalId == 0;

        public EditMyProposalsModel(IDataService dataService, IUserService userService, ICookieService cookieService)
            : base(dataService, userService) // Corrected: Passed userService to base constructor.
        {
            _dataService = dataService;
            _userService = userService;
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            var userId = _userService.GetUserId(User);
            if (userId == 0) return RedirectToPage("/");

            if (id.HasValue) // Editing
            {
                var proposal = await _dataService.GetProposalAsync(id.Value);
                if (proposal is null || proposal.UserId != userId) return Forbid();

                Input = new ProposalInputModel(proposal)
                {
                    SelectedGenreIds = (await _dataService.GetProposalGenresAsync(id.Value)).ToList()
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
            var userId = _userService.GetUserId(User);
            if (userId == 0) return Forbid();

            if (!ModelState.IsValid)
            {
                await PopulateSelectListsAsync();
                return Page();
            }

            int proposalId = await _dataService.UpsertProposalAsync(Input, userId);
            await _dataService.UpdateProposalGenresAsync(proposalId, Input.SelectedGenreIds);

            TempData["Message"] = "Proposal saved successfully!";
            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var userId = _userService.GetUserId(User);
            var proposal = await _dataService.GetProposalAsync(id);
            if (userId == 0 || proposal is null || proposal.UserId != userId) return Forbid();

            await _dataService.DeleteProposalAsync(id, userId);
            TempData["Message"] = "Proposal deleted successfully.";
            return RedirectToPage("./Index");
        }
        private async Task PopulateSelectListsAsync()
        {
            // CORRECTED: The text field is "ContentRatingName", not "ContentRating"
            Ratings = new SelectList(await _dataService.GetContentRatingsAsync(), "ContentRatingId", "ContentRatingName", Input.ContentRatingId);
            Statuses = new SelectList(await _dataService.GetProposalStatusesAsync(), "StatusId", "StatusName", Input.StatusId);

            var allGenres = await _dataService.GetGenresAsync();
            GenreSelection = allGenres.Select(g => new GenreSelectionViewModel
            {
                GenreId = g.GenreId,
                GenreName = g.GenreName,
                IsSelected = Input.SelectedGenreIds.Contains(g.GenreId)
            }).ToList();
        }
    }
}