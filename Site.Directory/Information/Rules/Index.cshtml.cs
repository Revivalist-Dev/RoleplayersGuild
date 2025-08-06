using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Services;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.Information.Rules
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
            var content = await _dataService.GetScalarAsync<string>("""SELECT "RulesPageContent" FROM "GeneralSettings" LIMIT 1""");
            if (!string.IsNullOrEmpty(content))
            {
                RulesContent = content;
            }
        }
    }
}