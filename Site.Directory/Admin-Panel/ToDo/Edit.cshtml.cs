using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
// Corrected: Using statement now matches the actual namespace of the local models
using RoleplayersGuild.Site.Directory.Admin_Panel.ToDo;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using RoleplayersGuild.Site.Services.DataServices;
using System.Collections.Generic;
using System.Threading.Tasks;

// Corrected: The '{' was removed after the file-scoped namespace
namespace RoleplayersGuild.Site.Directory.Admin_Panel.ToDo;

// [Authorize(Policy = "IsStaff")]
public class EditModel : PageModel
{
    private readonly IMiscDataService _miscDataService;

    public EditModel(IMiscDataService miscDataService)
    {
        _miscDataService = miscDataService;
    }

    [BindProperty]
    public ToDoItemInputModel ToDoItem { get; set; } = new();

    public SelectList AssignableUsers { get; set; } = new SelectList(new List<User>());
    public SelectList Statuses { get; set; } = new SelectList(new List<ToDoStatus>());
    public SelectList Types { get; set; } = new SelectList(new List<ToDoType>());

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var item = await _miscDataService.GetRecordAsync<ToDoItemInputModel>("""SELECT "ItemId", "ItemName", "ItemDescription", "StatusId", "TypeId", "AssignedToUserId" FROM "TodoItems" WHERE "ItemId" = @id""", new { id });

        if (item == null) return NotFound();

        ToDoItem = item;

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

        const string sql = """
            UPDATE "TodoItems" SET "ItemName" = @ItemName, "ItemDescription" = @ItemDescription, "StatusId" = @StatusId, 
            "TypeId" = @TypeId, "AssignedToUserId" = @AssignedToUserId WHERE "ItemId" = @ItemId
            """;

        await _miscDataService.ExecuteAsync(sql, new
        {
            ToDoItem.ItemId,
            ToDoItem.ItemName,
            ToDoItem.ItemDescription,
            ToDoItem.StatusId,
            ToDoItem.TypeId,
            AssignedToUserId = ToDoItem.AssignedToUserId == 0 ? (int?)null : ToDoItem.AssignedToUserId
        });

        return RedirectToPage("./Index");
    }

    private async Task PopulateSelectListsAsync()
    {
        var users = await _miscDataService.GetRecordsAsync<User>("""SELECT "UserId", "Username" FROM "Users" WHERE "UserTypeId" IN (2, 3, 4) ORDER BY "Username" """);
        AssignableUsers = new SelectList(users, "UserId", "Username", ToDoItem.AssignedToUserId);

        var statuses = await _miscDataService.GetRecordsAsync<ToDoStatus>("""SELECT "StatusId", "StatusName" FROM "TodoItemStatuses" ORDER BY "StatusName" """);
        Statuses = new SelectList(statuses, "StatusId", "StatusName", ToDoItem.StatusId);

        var types = await _miscDataService.GetRecordsAsync<ToDoType>("""SELECT "TypeId", "TypeName" FROM "TodoItemTypes" ORDER BY "TypeName" """);
        Types = new SelectList(types, "TypeId", "TypeName", ToDoItem.TypeId);
    }
}
// Corrected: Removed the extra '}' that was here
