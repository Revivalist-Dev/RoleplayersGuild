using Microsoft.AspNetCore.Mvc;
using RoleplayersGuild.Site.Model;
using System.Linq;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Services.ViewComponents
{
    public class UserNavViewComponent : ViewComponent
    {
        private readonly IDataService _dataService;
        private readonly IUserService _userService;

        public UserNavViewComponent(IDataService dataService, IUserService userService)
        {
            _dataService = dataService;
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
                    viewModel.IsStaff = user.IsAdmin;
                    viewModel.UnreadThreadCount = await _dataService.GetUnreadThreadCountAsync(user.UserId);
                    viewModel.UnreadImageCommentCount = await _dataService.GetUnreadImageCommentCountAsync(user.UserId);

                    if (viewModel.IsStaff)
                    {
                        viewModel.AdminOpenItemCount = await _dataService.GetScalarAsync<int>("""SELECT COUNT(*) FROM "Characters" WHERE "IsApproved" = FALSE""");
                    }

                    var quickLinks = await _dataService.GetUserQuickLinksAsync(user.UserId);
                    viewModel.QuickLinks = quickLinks.ToList();
                }
            }

            return View(viewModel);
        }
    }
}