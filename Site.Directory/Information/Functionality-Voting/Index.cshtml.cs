using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;

namespace RoleplayersGuild.Site.Directory.Functionality_Voting
{
    public class IndexModel : PageModel
    {
        private readonly IDataService _dataService;
        private readonly ICookieService _cookieService;

        public IndexModel(IDataService dataService, ICookieService cookieService)
        {
            _dataService = dataService;
            _cookieService = cookieService;
        }

        public List<ToDoItemViewModel> ConsiderationItems { get; set; } = new();

        [BindProperty]
        public NewIdeaInputModel NewIdea { get; set; } = new();

        [TempData]
        public string? Message { get; set; }

        public async Task OnGetAsync()
        {
            var userId = _cookieService.GetUserId();
            var items = await _dataService.GetConsiderationItemsAsync();
            foreach (var item in items)
            {
                item.HasVoted = userId != 0 && await _dataService.HasUserVotedAsync(item.ItemId, userId);
            }
            ConsiderationItems = items.ToList();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            var userId = _cookieService.GetUserId();
            if (userId == 0) return Forbid();

            await _dataService.AddToDoItemAsync(NewIdea.Name, NewIdea.Description, userId);
            Message = "Your idea has been submitted successfully!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAddVoteAsync(int itemId)
        {
            var userId = _cookieService.GetUserId();
            if (userId == 0) return Forbid();

            await _dataService.AddVoteAsync(itemId, userId);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRemoveVoteAsync(int itemId)
        {
            var userId = _cookieService.GetUserId();
            if (userId == 0) return Forbid();

            await _dataService.RemoveVoteAsync(itemId, userId);
            return RedirectToPage();
        }
    }

    public class NewIdeaInputModel
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;
    }
}