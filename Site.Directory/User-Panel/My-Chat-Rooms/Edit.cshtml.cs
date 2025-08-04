using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using System.Linq;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.User_Panel.My_Chat_Rooms
{
    public class EditMyChatRoomsModel : UserPanelBaseModel
    {
        // NOTE: The IDataService and IUserService are now inherited from the base class.
        private readonly INotificationService _notificationService;

        [BindProperty]
        public ChatRoomInputModel Input { get; set; } = new();

        public SelectList Universes { get; set; } = new(Enumerable.Empty<SelectListItem>());
        public SelectList Ratings { get; set; } = new(Enumerable.Empty<SelectListItem>());

        public bool IsNew => Input.ChatRoomId == 0;

        // UPDATED: Constructor to match the new base class signature.
        public EditMyChatRoomsModel(IDataService dataService, IUserService userService, INotificationService notificationService)
            : base(dataService, userService)
        {
            // Only assign services not provided by the base class.
            _notificationService = notificationService;
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            // Use the UserService property from the base class
            var userId = UserService.GetUserId(User);
            if (userId == 0) return RedirectToPage("/");

            if (id.HasValue) // Editing
            {
                // Use the DataService property from the base class
                var chatRoom = await DataService.GetChatRoomWithDetailsAsync(id.Value);
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
            var userId = UserService.GetUserId(User);
            if (userId == 0) return RedirectToPage("/");

            if (!ModelState.IsValid)
            {
                await PopulateSelectListsAsync(userId);
                return Page();
            }

            int chatRoomId = Input.ChatRoomId;

            if (IsNew)
            {
                chatRoomId = await DataService.CreateNewChatRoomAsync(userId);
                Input.ChatRoomId = chatRoomId;
                await _notificationService.SendMessageToStaffAsync("[Staff] - New Chat Room Submitted", "A new chat room has been submitted, please review.");
                await DataService.AwardBadgeIfNotExistingAsync(8, userId); // Chat Room Creator Badge
            }

            await DataService.UpdateChatRoomAsync(Input);

            TempData["Message"] = "Chat Room saved successfully!";
            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var userId = UserService.GetUserId(User);
            var chatRoom = await DataService.GetChatRoomAsync(id);
            if (userId == 0 || chatRoom is null || chatRoom.SubmittedByUserId != userId)
            {
                return Forbid();
            }

            await DataService.DeleteChatRoomAsync(id);
            TempData["Message"] = "Chat Room deleted successfully.";
            return RedirectToPage("./Index");
        }

        private async Task PopulateSelectListsAsync(int userId)
        {
            var userUniverses = await DataService.GetUserUniversesAsync(userId);
            var contentRatings = await DataService.GetContentRatingsAsync();

            Universes = new SelectList(userUniverses, "UniverseId", "UniverseName", Input.UniverseId);
            Ratings = new SelectList(contentRatings, "ContentRatingId", "ContentRatingName", Input.ContentRatingId);
        }
    }
}