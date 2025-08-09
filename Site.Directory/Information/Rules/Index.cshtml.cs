using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Services;
using RoleplayersGuild.Site.Services.DataServices;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.Information.Rules
{
    public class IndexModel : PageModel
    {
        private readonly IBaseDataService _baseDataService;

        public IndexModel(IBaseDataService baseDataService)
        {
            _baseDataService = baseDataService;
        }

        public string RulesContent { get; set; } = string.Empty;

        public async Task OnGetAsync()
        {
            var content = await _baseDataService.GetScalarAsync<string>("""SELECT "RulesPageContent" FROM "GeneralSettings" LIMIT 1""");
            if (!string.IsNullOrEmpty(content))
            {
                RulesContent = content;
            }
        }
    }
}
