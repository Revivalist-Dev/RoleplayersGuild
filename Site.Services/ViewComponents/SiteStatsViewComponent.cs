using Microsoft.AspNetCore.Mvc;
using RoleplayersGuild.Site.Services;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Services.ViewComponents
{
    public class SiteStatsViewComponent : ViewComponent
    {
        private readonly IDataService _dataService;

        public SiteStatsViewComponent(IDataService dataService)
        {
            _dataService = dataService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Corrected: Start all three tasks without awaiting them individually.
            // Also corrected SQL to use PascalCase.
            var onlineTask = _dataService.GetScalarAsync<int>("""SELECT COUNT("UserId") FROM "Users" WHERE "LastAction" >= NOW() - interval '15 minutes'""");
            var totalUsersTask = _dataService.GetScalarAsync<int>("""SELECT COUNT(*) FROM "Users" """);
            var totalCharactersTask = _dataService.GetScalarAsync<int>("""SELECT COUNT(*) FROM "Characters" """);

            // Corrected: Await all the correctly named tasks at once.
            await Task.WhenAll(onlineTask, totalUsersTask, totalCharactersTask);

            // Corrected: Get the .Result from the correct task variables.
            var model = (Online: onlineTask.Result, Total: totalUsersTask.Result, Characters: totalCharactersTask.Result);

            return View(model);
        }
    }
}