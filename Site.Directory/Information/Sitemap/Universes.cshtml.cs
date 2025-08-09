using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Services;
using RoleplayersGuild.Site.Services.DataServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RoleplayersGuild.Site.Directory.Sitemap;

public class UniversesModel : PageModel
{
    private readonly IBaseDataService _baseDataService;
    public UniversesModel(IBaseDataService baseDataService) { _baseDataService = baseDataService; }

    public async Task<IActionResult> OnGetAsync()
    {
        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        var urlset = new XElement(ns + "urlset");

        // StatusID = 2 means 'Approved'
        var universeIds = await _baseDataService.GetRecordsAsync<int>("SELECT UniverseId FROM Universes WHERE StatusID = 2");

        foreach (var id in universeIds)
        {
            urlset.Add(
                new XElement(ns + "url",
                    new XElement(ns + "loc", $"https://www.roleplayersguild.com/Universes/View/{id}")
                )
            );
        }

        var sitemap = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), urlset);
        return Content(sitemap.ToString(), "application/xml", Encoding.UTF8);
    }
}
