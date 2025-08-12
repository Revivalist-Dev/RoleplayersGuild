using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using RoleplayersGuild.Site.Services.DataServices;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

// Corrected: Namespace now matches folder structure
namespace RoleplayersGuild.Site.Directory.Admin_Panel.ToDo
{
    // [Authorize(Policy = "IsStaff")]
    public class CreateModel : PageModel
    {
        private readonly IMiscDataService _miscDataService;
        private readonly IUserService _userService;

        public CreateModel(IMiscDataService miscDataService, IUserService userService)
        {
            _miscDataService = miscDataService;
            _userService = userService;
        }

        [BindProperty]
        public ToDoItemInputModel ToDoItem { get; set; } = new();

        public SelectList AssignableUsers { get; set; } = new SelectList(new List<User>());
        public SelectList Statuses { get; set; } = new SelectList(new List<ToDoStatus>());
        public SelectList Types { get; set; } = new SelectList(new List<ToDoType>());

        public async Task<IActionResult> OnGetAsync()
        {
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

            var createdByUserId = _userService.GetUserId(User);

            // Corrected: SQL query uses PascalCase
            const string sql = """
                INSERT INTO "TodoItems"("ItemName", "StatusId", "TypeId", "AssignedToUserId", "CreatedByUserId", "ItemDescription") 
                VALUES (@ItemName, @StatusId, @TypeId, @AssignedToUserId, @CreatedByUserId, @ItemDescription)
                """;

            await _miscDataService.ExecuteAsync(sql, new
            {
                ToDoItem.ItemName,
                ToDoItem.StatusId,
                ToDoItem.TypeId,
                AssignedToUserId = ToDoItem.AssignedToUserId == 0 ? (int?)null : ToDoItem.AssignedToUserId,
                CreatedByUserId = createdByUserId,
                ToDoItem.ItemDescription
            });

            return RedirectToPage("./Index");
        }

        private async Task PopulateSelectListsAsync()
        {
            var users = await _miscDataService.GetRecordsAsync<User>("""SELECT "UserId", "Username" FROM "Users" WHERE "UserTypeId" IN (2, 3, 4) ORDER BY "Username" """);
            AssignableUsers = new SelectList(users, "UserId", "Username");

            var statuses = await _miscDataService.GetRecordsAsync<ToDoStatus>("""SELECT "StatusId", "StatusName" FROM "TodoItemStatuses" ORDER BY "StatusName" """);
            Statuses = new SelectList(statuses, "StatusId", "StatusName");

            var types = await _miscDataService.GetRecordsAsync<ToDoType>("""SELECT "TypeId", "TypeName" FROM "TodoItemTypes" ORDER BY "TypeName" """);
            Types = new SelectList(types, "TypeId", "TypeName");
        }
    }

    public class ToDoItemInputModel
    {
        public int ItemId { get; set; }
        [Required]
        public string ItemName { get; set; } = string.Empty;
        public string? ItemDescription { get; set; }
        // Corrected: Property name is StatusId
        public int StatusId { get; set; }
        public int TypeId { get; set; }
        public int? AssignedToUserId { get; set; }
    }
}
