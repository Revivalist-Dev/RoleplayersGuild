using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.Community.Characters.Galleries.Image
{
    public class ImageModel : PageModel
    {
        private readonly IDataService _dataService;
        // ADDED: Inject the modern user service
        private readonly IUserService _userService;

        // UPDATED: The constructor now accepts IUserService
        public ImageModel(IDataService dataService, IUserService userService)
        {
            _dataService = dataService;
            _userService = userService;
        }

        public CharacterImageWithDetails Image { get; set; } = new();
        public List<ImageCommentViewModel> Comments { get; set; } = new();
        public bool IsOwner { get; set; }
        public bool IsLoggedIn { get; set; }

        [BindProperty]
        public CommentInputModel NewComment { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var image = await _dataService.GetImageDetailsAsync(id);
            if (image == null) return NotFound();
            Image = image;

            // UPDATED: Use the modern service to get the user ID from the login session
            var currentUserId = _userService.GetUserId(User);
            IsLoggedIn = currentUserId != 0;
            IsOwner = currentUserId == Image.UserId;

            var comments = await _dataService.GetImageCommentsAsync(id);
            Comments = comments.ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // UPDATED: Use the modern service here as well
            var currentUserId = _userService.GetUserId(User);
            if (currentUserId == 0) return Forbid(); // More explicit check for not being logged in

            if (!ModelState.IsValid)
            {
                // Repopulate the page data if the model state is invalid
                await OnGetAsync(NewComment.ImageId);
                return Page();
            }

            var sendAsCharacterId = await _dataService.GetCurrentSendAsCharacterIdAsync();
            if (sendAsCharacterId == 0)
            {
                TempData["Message"] = "You must have a character selected to post a comment.";
                return RedirectToPage(new { id = NewComment.ImageId });
            }

            await _dataService.AddImageCommentAsync(NewComment.ImageId, sendAsCharacterId, NewComment.CommentText);
            return RedirectToPage(new { id = NewComment.ImageId });
        }
    }

    public class CommentInputModel
    {
        public int ImageId { get; set; }
        [Required]
        public string CommentText { get; set; } = string.Empty;
    }
}