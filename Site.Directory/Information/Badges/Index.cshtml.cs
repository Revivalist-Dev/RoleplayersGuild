using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.Badges
{
    public class IndexModel : PageModel
    {
        private readonly IDataService _dataService;

        public List<UserBadgeViewModel> Badges { get; set; } = new();

        public IndexModel(IDataService dataService)
        {
            _dataService = dataService;
        }

        public async Task OnGetAsync()
        {
            var badgesData = await _dataService.GetAvailableBadgesAsync();
            Badges = badgesData.ToList();
        }
    }
}