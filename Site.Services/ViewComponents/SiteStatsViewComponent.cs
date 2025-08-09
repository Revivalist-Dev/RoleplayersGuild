using Microsoft.AspNetCore.Mvc;
using RoleplayersGuild.Site.Services;
using RoleplayersGuild.Site.Services.DataServices;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Services.ViewComponents
{
    public class SiteStatsViewComponent : ViewComponent
    {
        private readonly IUserDataService _userDataService;
        private readonly ICharacterDataService _characterDataService;

        public SiteStatsViewComponent(IUserDataService userDataService, ICharacterDataService characterDataService)
        {
            _userDataService = userDataService;
            _characterDataService = characterDataService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var onlineTask = _userDataService.GetOnlineUserCountAsync();
            var totalUsersTask = _userDataService.GetTotalUserCountAsync();
            var totalCharactersTask = _characterDataService.GetTotalCharacterCountAsync();

            await Task.WhenAll(onlineTask, totalUsersTask, totalCharactersTask);

            var model = (Online: onlineTask.Result, Total: totalUsersTask.Result, Characters: totalCharactersTask.Result);

            return View(model);
        }
    }
}
