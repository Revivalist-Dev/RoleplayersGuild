using Microsoft.AspNetCore.Mvc;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using RoleplayersGuild.Site.Services.DataServices;
using RoleplayersGuild.Site.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace RoleplayersGuild.Site.Directory.User_Panel.My_Chat_Rooms
{
    public class IndexMyChatroomsModel : UserPanelBaseModel
    {
        public IEnumerable<ChatRoomWithDetails> ChatRooms { get; private set; } = new List<ChatRoomWithDetails>();

        public IndexMyChatroomsModel(ICharacterDataService characterDataService, ICommunityDataService communityDataService, IMiscDataService miscDataService, IUserService userService)
            : base(characterDataService, communityDataService, miscDataService, userService) { }

        public async Task<IActionResult> OnGetAsync()
        {
            var rooms = await _communityDataService.GetMyChatRoomsAsync(LoggedInUserId);
            if (rooms != null)
            {
                ChatRooms = rooms;
            }
            return Page();
        }

        public string GetTimeAgo(object? timeToCalculate)
        {
            if (timeToCalculate == null || timeToCalculate == DBNull.Value)
            {
                return "No posts yet";
            }
            return DateUtils.TimeAgo((DateTime)timeToCalculate);
        }
    }
}
