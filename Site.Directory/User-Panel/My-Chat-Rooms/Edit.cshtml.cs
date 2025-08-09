using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using RoleplayersGuild.Site.Services.DataServices;
using System.Linq;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.User_Panel.My_Chat_Rooms
{
    public class EditMyChatRoomsModel : UserPanelBaseModel
    {
        private readonly IUniverseDataService _universeDataService;
        private readonly IUserDataService _userDataService;
        private readonly INotificationService _notificationService;

        [BindProperty]
        public ChatRoomInputModel Input { get; set; } = new();

        public SelectList Universes { get; set; } = new(Enumerable.Empty<SelectListItem>());
        public SelectList Ratings { get; set; } = new(Enumerable.Empty<SelectListItem>());

        public bool IsNew => Input.ChatRoomId == 0;

        public EditMyChatRoomsModel(ICharacterDataService characterDataService, ICommunityDataService communityDataService, IMiscDataService miscDataService, IUserService userService, IUniverseDataService universeDataService, IUserDataService userDataService, INotificationService notificationService)
            : base(characterDataService, communityDataService, miscDataService, userService)
        {
            _universeDataService = universeDataService;
            _userDataService = userDataService;
            _notificationService = notificationService;
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            var userId = _userService.GetUserId(User);
            if (userId == 0) return RedirectToPage("/");

            if (id.HasValue) // Editing
            {
                var chatRoom = await _communityDataService.GetChatRoomWithDetailsAsync(id.Value);
                if (chatRoom is null || chatRoom.SubmittedByUserId != userId)
                {
                    return Forbid();
                }
                Input = new ChatRoomInputModel(chatRoom);
            }
            else // New
            {
                Input = new ChatRoomInputModel();
            }

            await PopulateSelectListsAsync(userId);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = _userService.GetUserId(User);
            if (userId == 0) return RedirectToPage("/");

            if (!ModelState.IsValid)
            {
                await PopulateSelectListsAsync(userId);
                return Page();
            }

            int chatRoomId = Input.ChatRoomId;

            if (IsNew)
            {
                chatRoomId = await _communityDataService.CreateNewChatRoomAsync(userId);
                Input.ChatRoomId = chatRoomId;
                await _notificationService.SendMessageToStaffAsync("[Staff] - New Chat Room Submitted", "A new chat room has been submitted, please review.");
                await _userDataService.AwardBadgeIfNotExistingAsync(8, userId); // Chat Room Creator Badge
            }

            await _communityDataService.UpdateChatRoomAsync(Input);

            TempData["Message"] = "Chat Room saved successfully!";
            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var userId = _userService.GetUserId(User);
            var chatRoom = await _communityDataService.GetChatRoomAsync(id);
            if (userId == 0 || chatRoom is null || chatRoom.SubmittedByUserId != userId)
            {
                return Forbid();
            }

            await _communityDataService.DeleteChatRoomAsync(id);
            TempData["Message"] = "Chat Room deleted successfully.";
            return RedirectToPage("./Index");
        }

        private async Task PopulateSelectListsAsync(int userId)
        {
            var userUniverses = await _universeDataService.GetUserUniversesAsync(userId);
            var contentRatings = await _miscDataService.GetContentRatingsAsync();

            Universes = new SelectList(userUniverses, "UniverseId", "UniverseName", Input.UniverseId);
            Ratings = new SelectList(contentRatings, "ContentRatingId", "ContentRatingName", Input.ContentRatingId);
        }
    }
}
