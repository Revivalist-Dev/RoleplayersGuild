using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.Admin_Panel.Users
{
    // [Authorize(Policy = "IsStaff")]
    public class IndexModel : PageModel
    {
        private readonly IDataService _dataService;
        private readonly ICookieService _cookieService;

        public IndexModel(IDataService dataService, ICookieService cookieService)
        {
            _dataService = dataService;
            _cookieService = cookieService;
        }

        [TempData] public string? Message { get; set; }
        [TempData] public bool IsSuccess { get; set; }
        public bool IsSuperAdmin => _cookieService.GetUserTypeId() >= 3;

        public IEnumerable<UserListingViewModel> AllUsers { get; set; } = new List<UserListingViewModel>();
        public User? SelectedUser { get; set; }
        public IEnumerable<Character> UserCharacters { get; set; } = new List<Character>();
        public IEnumerable<UserBadgeViewModel> UserBadges { get; set; } = new List<UserBadgeViewModel>();
        public IEnumerable<UserNoteViewModel> UserNotes { get; set; } = new List<UserNoteViewModel>();
        public SelectList AllBadges { get; set; } = new(new List<object>());
        public SelectList UserTypes { get; set; } = new(new List<object>());

        [BindProperty] public int? BadgeToAwardId { get; set; }
        [BindProperty, Required] public string? NewNoteContent { get; set; }
        [BindProperty] public int UserTypeId { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            const string userListSql = """
                SELECT "UserId", (CAST("UserId" AS VARCHAR) || ' - ' || COALESCE("Username", '')) AS "Username" 
                FROM "Users" ORDER BY "UserId"
                """;
            AllUsers = await _dataService.GetRecordsAsync<UserListingViewModel>(userListSql);

            if (id.HasValue)
            {
                SelectedUser = await _dataService.GetUserAsync(id.Value);
                if (SelectedUser is null) return NotFound();

                UserTypeId = SelectedUser.UserTypeId;

                UserCharacters = await _dataService.GetRecordsAsync<Character>("""SELECT "CharacterId", "CharacterDisplayName" FROM "Characters" WHERE "UserId" = @id""", new { id });
                UserBadges = await _dataService.GetRecordsAsync<UserBadgeViewModel>("""SELECT b."BadgeName", b."BadgeImageUrl", ub."UserBadgeId" FROM "Badges" b JOIN "UserBadges" ub ON b."BadgeId" = ub."BadgeId" WHERE ub."UserId" = @id""", new { id });
                UserNotes = await _dataService.GetRecordsAsync<UserNoteViewModel>("""SELECT * FROM "UserNotesWithDetails" WHERE "UserId" = @id ORDER BY "NoteTimestamp" DESC""", new { id });

                var allBadges = await _dataService.GetRecordsAsync<Badge>("""SELECT "BadgeId", "BadgeName" FROM "Badges" ORDER BY "BadgeName" """);
                AllBadges = new SelectList(allBadges, "BadgeId", "BadgeName");

                var userTypes = new List<object> {
                    new { Id = 1, Name = "Basic" }, new { Id = 2, Name = "Moderator" },
                    new { Id = 3, Name = "Admin" }
                };
                UserTypes = new SelectList(userTypes, "Id", "Name", SelectedUser.UserTypeId);
            }
            return Page();
        }

        public async Task<IActionResult> OnPostUpdateUserTypeAsync(int id)
        {
            if (!IsSuperAdmin) return Forbid();
            await _dataService.ExecuteAsync("""UPDATE "Users" SET "UserTypeId" = @UserTypeId WHERE "UserId" = @id""", new { UserTypeId, id });
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostBanUserAsync(int id)
        {
            if (!IsSuperAdmin) return Forbid();
            await _dataService.BanUserAsync(id);
            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnPostAwardBadgeAsync(int id)
        {
            if (!BadgeToAwardId.HasValue)
            {
                Message = "You must select a badge to award."; IsSuccess = false;
                return RedirectToPage(new { id });
            }
            await _dataService.ExecuteAsync("""INSERT INTO "UserBadges" ("UserId", "BadgeId") VALUES (@UserId, @BadgeId)""", new { UserId = id, BadgeId = BadgeToAwardId.Value });
            Message = "Badge awarded successfully."; IsSuccess = true;
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostRemoveBadgeAsync(int id, int userBadgeId)
        {
            await _dataService.ExecuteAsync("""DELETE FROM "UserBadges" WHERE "UserBadgeId" = @userBadgeId""", new { userBadgeId });
            Message = "Badge removed."; IsSuccess = true;
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostAddNoteAsync(int id)
        {
            if (!ModelState.IsValid)
            {
                Message = "Note content cannot be empty."; IsSuccess = false;
                return RedirectToPage(new { id });
            }
            var createdByUserId = _cookieService.GetUserId();
            await _dataService.ExecuteAsync("""INSERT INTO "UserNotes" ("UserId", "CreatedByUserId", "NoteContent") VALUES (@UserId, @CreatedBy, @Content)""", new { UserId = id, CreatedBy = createdByUserId, Content = NewNoteContent });
            Message = "Note added."; IsSuccess = true;
            return RedirectToPage(new { id });
        }
    }
}