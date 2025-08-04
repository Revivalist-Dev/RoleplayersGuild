using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.Coming_Changes
{
    public class IndexModel : PageModel
    {
        private readonly IDataService _dataService;

        public List<ToDoItemViewModel> DevelopmentItems { get; set; } = new();
        public List<ToDoItemViewModel> ConsiderationItems { get; set; } = new();

        public IndexModel(IDataService dataService)
        {
            _dataService = dataService;
        }

        public async Task OnGetAsync()
        {
            DevelopmentItems = (await _dataService.GetDevelopmentItemsAsync()).ToList();
            ConsiderationItems = (await _dataService.GetConsiderationItemsAsync()).ToList();
        }
    }
}