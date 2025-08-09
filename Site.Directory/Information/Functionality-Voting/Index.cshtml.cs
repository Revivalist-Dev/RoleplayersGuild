using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using RoleplayersGuild.Site.Services.DataServices;

namespace RoleplayersGuild.Site.Directory.Functionality_Voting
{
    public class IndexModel : PageModel
    {
        private readonly IMiscDataService _miscDataService;
        private readonly IUserService _userService;

        public IndexModel(IMiscDataService miscDataService, IUserService userService)
        {
            _miscDataService = miscDataService;
            _userService = userService;
        }

        public List<ToDoItemViewModel> ConsiderationItems { get; set; } = new();

        [BindProperty]
        public NewIdeaInputModel NewIdea { get; set; } = new();

        [TempData]
        public string? Message { get; set; }

        public async Task OnGetAsync()
        {
            var userId = _userService.GetUserId(User);
            var items = await _miscDataService.GetConsiderationItemsAsync();
            foreach (var item in items)
            {
                item.HasVoted = userId != 0 && await _miscDataService.HasUserVotedAsync(item.ItemId, userId);
            }
            ConsiderationItems = items.ToList();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            var userId = _userService.GetUserId(User);
            if (userId == 0) return Forbid();

            await _miscDataService.AddToDoItemAsync(NewIdea.Name, NewIdea.Description, userId);
            Message = "Your idea has been submitted successfully!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAddVoteAsync(int itemId)
        {
            var userId = _userService.GetUserId(User);
            if (userId == 0) return Forbid();

            await _miscDataService.AddVoteAsync(itemId, userId);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRemoveVoteAsync(int itemId)
        {
            var userId = _userService.GetUserId(User);
            if (userId == 0) return Forbid();

            await _miscDataService.RemoveVoteAsync(itemId, userId);
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
