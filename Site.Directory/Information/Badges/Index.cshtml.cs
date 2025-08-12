using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using RoleplayersGuild.Site.Services.DataServices;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.Information.Badges
{
    public class IndexModel : PageModel
    {
        private readonly IUserDataService _userDataService;

        public List<UserBadgeViewModel> Badges { get; set; } = new();

        public IndexModel(IUserDataService userDataService)
        {
            _userDataService = userDataService;
        }

        public async Task OnGetAsync()
        {
            var badgesData = await _userDataService.GetAvailableBadgesAsync();
            Badges = badgesData.ToList();
        }
    }
}
