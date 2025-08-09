using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using RoleplayersGuild.Site.Services.DataServices;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.Coming_Changes
{
    public class IndexModel : PageModel
    {
        private readonly IMiscDataService _miscDataService;

        public List<ToDoItemViewModel> DevelopmentItems { get; set; } = new();
        public List<ToDoItemViewModel> ConsiderationItems { get; set; } = new();

        public IndexModel(IMiscDataService miscDataService)
        {
            _miscDataService = miscDataService;
        }

        public async Task OnGetAsync()
        {
            DevelopmentItems = (await _miscDataService.GetDevelopmentItemsAsync()).ToList();
            ConsiderationItems = (await _miscDataService.GetConsiderationItemsAsync()).ToList();
        }
    }
}
