using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Services;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.Rules
{
    public class IndexModel : PageModel
    {
        private readonly IDataService _dataService;

        public IndexModel(IDataService dataService)
        {
            _dataService = dataService;
        }

        public string RulesContent { get; set; } = string.Empty;

        public async Task OnGetAsync()
        {
            // Assumes a method in IDataService to fetch this setting.
            // You may need to add this method.
            var content = await _dataService.GetScalarAsync<string>("SELECT RulesPageContent FROM General_Settings");
            if (!string.IsNullOrEmpty(content))
            {
                RulesContent = content;
            }
        }
    }
}