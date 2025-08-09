using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using RoleplayersGuild.Site.Services.DataServices;
using System.ComponentModel.DataAnnotations;

// Corrected: Namespace now matches folder structure
namespace RoleplayersGuild.Site.Directory.Admin_Panel.Chat_Rooms
{
    // [Authorize(Policy = "IsStaff")]
    public class IndexModel : PageModel
    {
        private readonly ICommunityDataService _communityDataService;

        public IndexModel(ICommunityDataService communityDataService)
        {
            _communityDataService = communityDataService;
        }

        [TempData]
        public string? Message { get; set; }
        [TempData]
        public bool IsSuccess { get; set; }

        public IEnumerable<ChatRoomListingViewModel> AllChatRooms { get; set; } = new List<ChatRoomListingViewModel>();
        public SelectList? Universes { get; set; }
        public IEnumerable<LockedUserViewModel> LockedUsers { get; set; } = new List<LockedUserViewModel>();

        [BindProperty]
        public ChatRoomEditModel? ChatRoom { get; set; }

        [BindProperty]
        public int? UserIdToLock { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            const string listSql = """
                SELECT "ChatRoomId", CASE WHEN "ChatRoomStatusId" = 1 THEN '[PA] - ' || "ChatRoomName" ELSE "ChatRoomName" END AS "ChatRoomName", "ChatRoomStatusId" 
                FROM "ChatRooms" ORDER BY "ChatRoomStatusId", "ChatRoomName"
                """;
            AllChatRooms = await _communityDataService.GetRecordsAsync<ChatRoomListingViewModel>(listSql);

            if (id.HasValue)
            {
                var room = await _communityDataService.GetChatRoomWithDetailsAsync(id.Value);
                if (room == null) return NotFound();

                ChatRoom = new ChatRoomEditModel
                {
                    ChatRoomId = room.ChatRoomId,
                    Name = room.ChatRoomName,
                    Description = room.ChatRoomDescription,
                    ContentRatingId = room.ContentRatingId ?? 1,
                    UniverseId = room.UniverseId ?? 0,
                    IsApproved = room.ChatRoomStatusId == 2,
                    SubmittedByUserId = room.SubmittedByUserId,
                    SubmittedByUsername = room.Username
                };

                var universes = await _communityDataService.GetRecordsAsync<Universe>("""SELECT "UniverseId", "UniverseName" FROM "Universes" ORDER BY "UniverseName" """);
                Universes = new SelectList(universes, "UniverseId", "UniverseName", ChatRoom.UniverseId);

                const string lockedUsersSql = """
                    SELECT u."UserId", u."Username" FROM "Users" u 
                    JOIN "ChatRoomLocks" l ON u."UserId" = l."UserId" 
                    WHERE l."ChatRoomId" = @ChatRoomId
                    """;
                LockedUsers = await _communityDataService.GetRecordsAsync<LockedUserViewModel>(lockedUsersSql, new { ChatRoomId = id.Value });
            }

            return Page();
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            if (ChatRoom == null || !ModelState.IsValid) return Page();

            var model = new ChatRoomInputModel
            {
                ChatRoomId = ChatRoom.ChatRoomId,
                ChatRoomName = ChatRoom.Name,
                ChatRoomDescription = ChatRoom.Description,
                ContentRatingId = ChatRoom.ContentRatingId,
                UniverseId = ChatRoom.UniverseId == 0 ? null : ChatRoom.UniverseId,
            };
            await _communityDataService.UpdateChatRoomAsync(model);

            int statusId = ChatRoom.IsApproved ? 2 : 1;
            await _communityDataService.ExecuteAsync("""UPDATE "ChatRooms" SET "ChatRoomStatusId" = @StatusID WHERE "ChatRoomId" = @ChatRoomId""", new { StatusID = statusId, ChatRoom.ChatRoomId });

            IsSuccess = true;
            Message = "Chat room has been saved.";
            return RedirectToPage(new { id = ChatRoom.ChatRoomId });
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            await _communityDataService.DeleteChatRoomAsync(id);
            TempData["IsSuccess"] = true;
            TempData["Message"] = "The chat room has been deleted.";
            return RedirectToPage("/Admin-Panel/Chat-Rooms/Index");
        }

        public async Task<IActionResult> OnPostPurgeAsync(int id)
        {
            await _communityDataService.ExecuteAsync("""DELETE FROM "ChatRoomPosts" WHERE "ChatRoomId" = @ChatRoomId""", new { ChatRoomId = id });
            IsSuccess = true;
            Message = "The chat room has been purged.";
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostLockUserAsync(int id)
        {
            if (UserIdToLock.HasValue)
            {
                await _communityDataService.ExecuteAsync("""INSERT INTO "ChatRoomLocks" ("ChatRoomId", "UserId") VALUES (@ChatRoomId, @UserId)""", new { ChatRoomId = id, UserId = UserIdToLock.Value });
            }
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostUnlockUserAsync(int id, int userId)
        {
            await _communityDataService.ExecuteAsync("""DELETE FROM "ChatRoomLocks" WHERE "ChatRoomId" = @ChatRoomId AND "UserId" = @UserId""", new { ChatRoomId = id, UserId = userId });
            return RedirectToPage(new { id });
        }
    }

    // Local ViewModels for this page
    public class ChatRoomListingViewModel
    {
        public int ChatRoomId { get; set; }
        public string ChatRoomName { get; set; } = string.Empty;
        public int ChatRoomStatusId { get; set; }
    }

    public class ChatRoomEditModel
    {
        public int ChatRoomId { get; set; }
        [Required]
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int ContentRatingId { get; set; }
        public int? UniverseId { get; set; }
        public bool IsApproved { get; set; }
        public int? SubmittedByUserId { get; set; }
        public string? SubmittedByUsername { get; set; }
    }

    public class LockedUserViewModel
    {
        public int UserId { get; set; }
        public string? Username { get; set; }
    }
}
