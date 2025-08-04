using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

// Corrected: Namespace now matches folder structure
namespace RoleplayersGuild.Site.Directory.Admin_Panel.ToDo
{
    // [Authorize(Policy = "IsStaff")]
    public class IndexModel : PageModel
    {
        private readonly IDataService _dataService;

        public IndexModel(IDataService dataService)
        {
            _dataService = dataService;
        }

        [BindProperty(SupportsGet = true)]
        public int? AssignedToFilter { get; set; }
        [BindProperty(SupportsGet = true)]
        public int? StatusFilter { get; set; }
        [BindProperty(SupportsGet = true)]
        public int? TypeFilter { get; set; }

        public IEnumerable<ToDoItemViewModel> ToDoItems { get; set; } = new List<ToDoItemViewModel>();
        public SelectList AssignableUsers { get; set; } = new SelectList(new List<User>());
        public SelectList Statuses { get; set; } = new SelectList(new List<ToDoStatus>());
        public SelectList Types { get; set; } = new SelectList(new List<ToDoType>());

        public async Task OnGetAsync()
        {
            await PopulateSelectListsAsync();

            // Corrected: SQL query uses PascalCase and COALESCE for null handling
            var sqlBuilder = new StringBuilder("""SELECT "ItemId", "ItemName", "ItemDescription", "TypeName", "StatusName", COALESCE("AssignedToUsername", 'No One') AS "AssignedToUsername" FROM "TodoItemsWithDetails" WHERE 1=1""");
            var parameters = new DynamicParameters();

            if (AssignedToFilter.HasValue && AssignedToFilter > 0)
            {
                sqlBuilder.Append(""" AND "AssignedToUserId" = @AssignedToUserId""");
                parameters.Add("AssignedToUserId", AssignedToFilter.Value);
            }

            if (StatusFilter.HasValue && StatusFilter > 0)
            {
                sqlBuilder.Append(""" AND "StatusId" = @StatusId""");
                parameters.Add("StatusId", StatusFilter.Value);
            }
            else
            {
                sqlBuilder.Append(""" AND "StatusId" NOT IN (3, 5)"""); // Not 'Completed' or 'Rejected'
            }

            if (TypeFilter.HasValue && TypeFilter > 0)
            {
                sqlBuilder.Append(""" AND "TypeId" = @TypeId""");
                parameters.Add("TypeId", TypeFilter.Value);
            }

            sqlBuilder.Append(""" ORDER BY "ItemId" DESC""");
            ToDoItems = await _dataService.GetRecordsAsync<ToDoItemViewModel>(sqlBuilder.ToString(), parameters);
        }

        private async Task PopulateSelectListsAsync()
        {
            var users = await _dataService.GetRecordsAsync<User>("""SELECT "UserId", "Username" FROM "Users" WHERE "UserTypeId" IN (2, 3, 4) ORDER BY "Username" """);
            AssignableUsers = new SelectList(users, "UserId", "Username", AssignedToFilter);

            var statuses = await _dataService.GetRecordsAsync<ToDoStatus>("""SELECT "StatusId", "StatusName" FROM "TodoItemStatuses" ORDER BY "StatusName" """);
            Statuses = new SelectList(statuses, "StatusId", "StatusName", StatusFilter);

            var types = await _dataService.GetRecordsAsync<ToDoType>("""SELECT "TypeId", "TypeName" FROM "TodoItemTypes" ORDER BY "TypeName" """);
            Types = new SelectList(types, "TypeId", "TypeName", TypeFilter);
        }
    }

    public class ToDoItemViewModel
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string? ItemDescription { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public string TypeName { get; set; } = string.Empty;
        public string AssignedToUsername { get; set; } = "No One";
    }

    public class ToDoStatus
    {
        // Corrected: Property name is StatusId
        public int StatusId { get; set; }
        public string StatusName { get; set; } = "";
    }

    public class ToDoType
    {
        public int TypeId { get; set; }
        public string TypeName { get; set; } = "";
    }
}