using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using RoleplayersGuild.Site.Directory.User_Panel;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.Dashboard
{
    public class IndexDashboardModel : UserPanelBaseModel
    {
        public DashboardFunding? Funding { get; private set; }

        // The properties for ActiveChatRooms, PopularStories, etc., have been removed
        // as they are all now loaded dynamically.

        public IndexDashboardModel(IDataService dataService, IUserService userService)
            : base(dataService, userService) { }

        public async Task OnGetAsync()
        {
            Funding = await DataService.GetDashboardFundingAsync();
        }

        public ViewComponentResult OnGetCharacterList(string screenStatus)
        {
            int recordCount = 18;
            bool showFooter = true;
            string displaySize = "profile-card-vertical";
            return new ViewComponentResult
            {
                ViewComponentName = "CharacterListing",
                Arguments = new { displaySize, showFooter, screenStatus, recordCount }
            };
        }

        public async Task<IActionResult> OnGetDashboardListAsync(string itemType, string filter)
        {
            var items = await DataService.GetDashboardItemsAsync(itemType, filter, LoggedInUserId);
            return Partial("_DashboardList", items);
        }

        // NEW HANDLER for the chat room panel
        public async Task<IActionResult> OnGetChatRoomListAsync()
        {
            var rooms = await DataService.GetDashboardChatRoomsAsync(LoggedInUserId);
            return Partial("_ChatRoomList", rooms);
        }
    }
}