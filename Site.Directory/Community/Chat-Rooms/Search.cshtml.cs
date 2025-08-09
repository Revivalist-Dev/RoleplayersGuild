using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using RoleplayersGuild.Site.Services.DataServices;
using RoleplayersGuild.Site.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.Community.Chat_Rooms
{
    public class SearchModel : PageModel
    {
        private readonly ICommunityDataService _communityDataService;

        public SearchModel(ICommunityDataService communityDataService)
        {
            _communityDataService = communityDataService;
        }

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public List<int> SelectedGenreIds { get; set; } = new();

        [BindProperty(SupportsGet = true, Name = "p")]
        public int CurrentPage { get; set; } = 1;

        public PagedResult<ChatRoomWithDetails>? ChatRoomsResult { get; private set; }

        public async Task OnGetAsync()
        {
            const int pageSize = 20;
            ChatRoomsResult = await _communityDataService.SearchChatRoomsAsync(CurrentPage, pageSize, SearchTerm, SelectedGenreIds);
        }

        public string GetTimeAgo(DateTime? date)
        {
            if (!date.HasValue) return "No posts yet";
            return DateUtils.TimeAgo(date.Value);
        }
    }
}
