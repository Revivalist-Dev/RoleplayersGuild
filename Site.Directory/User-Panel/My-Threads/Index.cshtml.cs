using Microsoft.AspNetCore.Mvc;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.User_Panel.My_Threads
{
    public class IndexMyThreadsModel : UserPanelBaseModel
    {
        public List<ThreadDetails> Threads { get; set; } = new();
        public string PageTitle { get; set; } = "My Threads";

        [BindProperty(SupportsGet = true)]
        public string? Filter { get; set; }

        // UPDATED: Constructor to match the new base class signature.
        public IndexMyThreadsModel(IDataService dataService, IUserService userService)
            : base(dataService, userService) { }

        public async Task<IActionResult> OnGetAsync()
        {
            var filter = Filter ?? "active";
            PageTitle = GetPageTitle(filter);
            Threads = (await DataService.GetUserThreadsAsync(LoggedInUserId, filter)).ToList();
            return Page();
        }

        private string GetPageTitle(string filter) => filter.ToLower() switch
        {
            "unread" => "Unread Threads",
            "unanswered" => "Unanswered Threads",
            _ => "My Threads"
        };
    }
}