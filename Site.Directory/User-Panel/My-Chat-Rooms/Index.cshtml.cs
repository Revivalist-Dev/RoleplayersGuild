using Microsoft.AspNetCore.Mvc;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using RoleplayersGuild.Site.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace RoleplayersGuild.Site.Directory.User_Panel.My_Chat_Rooms
{
    public class IndexMyChatroomsModel : UserPanelBaseModel
    {
        public IEnumerable<ChatRoomWithDetails> ChatRooms { get; private set; } = new List<ChatRoomWithDetails>();

        // UPDATED: Constructor to match the new base class signature.
        public IndexMyChatroomsModel(IDataService dataService, IUserService userService)
            : base(dataService, userService) { }

        public async Task<IActionResult> OnGetAsync()
        {
            const string sql = """
                SELECT * FROM "ChatRoomsForListing" 
                WHERE ("UniverseOwnerId" = @UserId OR "SubmittedByUserId" = @UserId) 
                ORDER BY "ChatRoomName"
                """;

            var rooms = await DataService.GetRecordsAsync<ChatRoomWithDetails>(sql, new { UserId = LoggedInUserId });
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