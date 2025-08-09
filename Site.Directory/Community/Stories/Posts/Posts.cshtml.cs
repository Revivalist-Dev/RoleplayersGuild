using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using RoleplayersGuild.Site.Services.DataServices;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.Community.Stories
{
    public class PostsModel : PageModel
    {
        private readonly IContentDataService _contentDataService;
        private readonly IUserDataService _userDataService;
        private readonly ICharacterDataService _characterDataService;
        private readonly IUserService _userService;
        private readonly INotificationService _notificationService;

        public PostsModel(IContentDataService contentDataService, IUserDataService userDataService, ICharacterDataService characterDataService, IUserService userService, INotificationService notificationService)
        {
            _contentDataService = contentDataService;
            _userDataService = userDataService;
            _characterDataService = characterDataService;
            _userService = userService;
            _notificationService = notificationService;
        }

        [BindProperty(SupportsGet = true, Name = "id")]
        public int StoryId { get; set; }

        [BindProperty(SupportsGet = true, Name = "p")]
        public int CurrentPage { get; set; } = 1;

        [BindProperty]
        [Required(ErrorMessage = "You cannot submit an empty post.")]
        public string NewPostContent { get; set; } = string.Empty;

        [BindProperty]
        public int PostAsCharacterId { get; set; }

        [TempData]
        public string? Message { get; set; }

        public StoryWithDetails? Story { get; set; }
        public PagedResult<StoryPostViewModel>? Posts { get; set; }
        public List<Character> UserCharacters { get; set; } = new();
        public bool IsBlocked { get; set; }
        public int CurrentUserId { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            Story = await _contentDataService.GetStoryWithDetailsAsync(StoryId);
            if (Story is null)
            {
                return NotFound();
            }

            CurrentUserId = _userService.GetUserId(User);

            if (CurrentUserId > 0 && Story.UserId > 0)
            {
                IsBlocked = await _userDataService.IsUserBlockedAsync(Story.UserId, CurrentUserId);
            }

            if (IsBlocked)
            {
                TempData["Message"] = "You and the owner of this story have blocked each other, so you cannot view or add posts.";
            }
            else
            {
                Posts = await _contentDataService.GetStoryPostsPagedAsync(StoryId, CurrentPage, 10);
            }

            if (CurrentUserId > 0)
            {
                UserCharacters = (await _characterDataService.GetActiveCharactersForUserAsync(CurrentUserId)).ToList();
                var currentPostAs = await _userDataService.GetCurrentSendAsCharacterIdAsync();

                if (UserCharacters.Any(c => c.CharacterId == currentPostAs))
                {
                    PostAsCharacterId = currentPostAs;
                }
                else if (UserCharacters.Any())
                {
                    PostAsCharacterId = UserCharacters.First().CharacterId;
                }
            }

            ViewData["Title"] = Story.StoryTitle;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            CurrentUserId = _userService.GetUserId(User);
            if (CurrentUserId == 0) return Forbid();

            if (!ModelState.IsValid)
            {
                return await OnGetAsync();
            }

            var userChars = (await _characterDataService.GetActiveCharactersForUserAsync(CurrentUserId)).ToList();
            if (!userChars.Any(c => c.CharacterId == PostAsCharacterId))
            {
                TempData["Message"] = "Invalid character selected.";
                return await OnGetAsync();
            }

            await _contentDataService.AddStoryPostAsync(StoryId, PostAsCharacterId, NewPostContent);

            var story = await _contentDataService.GetStoryWithDetailsAsync(StoryId);
            if (story is not null && story.UserId != CurrentUserId)
            {
                await _notificationService.NotifyStoryOwnerOfNewPostAsync(StoryId, story.UserId, PostAsCharacterId);
            }

            const int pageSize = 10;
            var postResult = await _contentDataService.GetStoryPostsPagedAsync(StoryId, 1, pageSize);
            var totalCount = postResult?.TotalCount ?? 1;
            var lastPage = (totalCount + pageSize - 1) / pageSize;

            return RedirectToPage(new { id = StoryId, p = lastPage });
        }

        public async Task<IActionResult> OnPostDeleteAsync(int postId)
        {
            var currentUserId = _userService.GetUserId(User);
            var story = await _contentDataService.GetStoryWithDetailsAsync(StoryId);

            if (currentUserId == 0 || story is null) return Forbid();

            await _contentDataService.DeleteStoryPostAsync(postId, currentUserId, story.UserId);

            TempData["Message"] = "Post successfully deleted.";
            return RedirectToPage(new { id = StoryId, p = CurrentPage });
        }
    }
}
