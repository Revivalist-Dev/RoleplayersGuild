using Microsoft.AspNetCore.Mvc;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using RoleplayersGuild.Site.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.User_Panel.My_Stories
{
    public class IndexMyStoriesModel : UserPanelBaseModel
    {
        public IEnumerable<StoryWithDetails> Stories { get; private set; } = new List<StoryWithDetails>();

        public IndexMyStoriesModel(IDataService dataService, IUserService userService)
            : base(dataService, userService) { }

        public async Task<IActionResult> OnGetAsync()
        {
            var stories = await DataService.GetUserStoriesAsync(LoggedInUserId);
            if (stories != null)
            {
                Stories = stories;
            }
            return Page();
        }
    }
}