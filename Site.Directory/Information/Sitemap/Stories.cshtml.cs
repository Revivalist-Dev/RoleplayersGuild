using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Services.DataServices;
using System.Text;

using System.Xml.Linq;

namespace RoleplayersGuild.Site.Directory.Information.Sitemap;

public class StoriesModel : PageModel
{
    private readonly IContentDataService _contentDataService;
    public StoriesModel(IContentDataService contentDataService) { _contentDataService = contentDataService; }

    public async Task<IActionResult> OnGetAsync()
    {
        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        var urlset = new XElement(ns + "urlset");

        var storyIds = await _contentDataService.GetRecordsAsync<int>("SELECT StoryId FROM Stories WHERE IsPrivate = 0");

        foreach (var id in storyIds)
        {
            urlset.Add(
                new XElement(ns + "url",
                    new XElement(ns + "loc", $"https://www.roleplayersguild.com/Stories/View/{id}")
                )
            );
        }

        var sitemap = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), urlset);
        return Content(sitemap.ToString(), "application/xml", Encoding.UTF8);
    }
}
