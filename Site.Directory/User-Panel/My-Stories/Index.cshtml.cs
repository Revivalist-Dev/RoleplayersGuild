using Microsoft.AspNetCore.Mvc;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using RoleplayersGuild.Site.Services.DataServices;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.User_Panel.My_Stories
{
    public class IndexMyStoriesModel : UserPanelBaseModel
    {
        private readonly IContentDataService _contentDataService;
        public IEnumerable<StoryWithDetails> Stories { get; private set; } = new List<StoryWithDetails>();

        public IndexMyStoriesModel(
            ICharacterDataService characterDataService,
            ICommunityDataService communityDataService,
            IMiscDataService miscDataService,
            IUserService userService,
            IContentDataService contentDataService)
            : base(characterDataService, communityDataService, miscDataService, userService)
        {
            _contentDataService = contentDataService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var stories = await _contentDataService.GetUserStoriesAsync(LoggedInUserId);
            if (stories != null)
            {
                Stories = stories;
            }
            return Page();
        }
    }
}
