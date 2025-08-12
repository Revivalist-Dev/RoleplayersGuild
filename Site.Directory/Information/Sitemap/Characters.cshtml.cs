using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Services;
using RoleplayersGuild.Site.Services.DataServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RoleplayersGuild.Site.Directory.Information.Sitemap;

public class CharactersModel : PageModel
{
    private readonly ICharacterDataService _characterDataService;
    public CharactersModel(ICharacterDataService characterDataService) { _characterDataService = characterDataService; }

    public async Task<IActionResult> OnGetAsync()
    {
        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        var urlset = new XElement(ns + "urlset");

        var characterIds = await _characterDataService.GetRecordsAsync<int>("SELECT CharacterId FROM Characters WHERE IsPrivate = 0 AND IsApproved = 1");

        foreach (var id in characterIds)
        {
            urlset.Add(
                new XElement(ns + "url",
                    new XElement(ns + "loc", $"https://www.roleplayersguild.com/Characters/View/{id}")
                )
            );
        }

        var sitemap = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), urlset);
        return Content(sitemap.ToString(), "application/xml", Encoding.UTF8);
    }
}
