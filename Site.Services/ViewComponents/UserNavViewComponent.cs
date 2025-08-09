using Microsoft.AspNetCore.Mvc;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services.DataServices;
using System.Linq;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Services.ViewComponents
{
    public class UserNavViewComponent : ViewComponent
    {
        private readonly ICommunityDataService _communityDataService;
        private readonly ICharacterDataService _characterDataService;
        private readonly IUserDataService _userDataService;
        private readonly IMiscDataService _miscDataService;
        private readonly IUserService _userService;

        public UserNavViewComponent(ICommunityDataService communityDataService, ICharacterDataService characterDataService, IUserDataService userDataService, IMiscDataService miscDataService, IUserService userService)
        {
            _communityDataService = communityDataService;
            _characterDataService = characterDataService;
            _userDataService = userDataService;
            _miscDataService = miscDataService;
            _userService = userService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var viewModel = new UserNavViewModel();

            viewModel.IsAuthenticated = User.Identity?.IsAuthenticated ?? false; // <-- ADD THIS LINE

            if (viewModel?.IsAuthenticated == true)
            {
                var user = await _userService.GetCurrentUserAsync();
                if (user != null)
                {
                    viewModel.UnreadThreadCount = await _communityDataService.GetUnreadThreadCountAsync(user.UserId);
                    viewModel.UnreadImageCommentCount = await _userDataService.GetUnreadImageCommentCountAsync(user.UserId);

                    if (User.IsInRole("Staff") || User.IsInRole("Admin") || User.IsInRole("SuperAdmin"))
                    {
                        viewModel.AdminOpenItemCount = await _characterDataService.GetUnapprovedCharacterCountAsync();
                    }

                    var quickLinks = await _miscDataService.GetUserQuickLinksAsync(user.UserId);
                    viewModel.QuickLinks = quickLinks.ToList();
                }
            }

            return View(viewModel);
        }
    }
}
