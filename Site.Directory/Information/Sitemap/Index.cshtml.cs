using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Xml.Linq;

namespace RoleplayersGuild.Site.Directory.Sitemap; // Corrected namespace

public class IndexModel : PageModel
{
    public IActionResult OnGet()
    {
        var sitemap = new XDocument(
            new XDeclaration("1.0", "utf-8", "yes"),
            new XElement("sitemapindex",
                new XAttribute("xmlns", "http://www.sitemaps.org/schemas/sitemap/0.9"),
                new XElement("sitemap", new XElement("loc", "https://www.roleplayersguild.com/sitemap/static")),
                new XElement("sitemap", new XElement("loc", "https://www.roleplayersguild.com/sitemap/characters")),
                new XElement("sitemap", new XElement("loc", "https://www.roleplayersguild.com/sitemap/images")),
                new XElement("sitemap", new XElement("loc", "https://www.roleplayersguild.com/sitemap/articles")), // Added
                new XElement("sitemap", new XElement("loc", "https://www.roleplayersguild.com/sitemap/stories")),   // Added
                new XElement("sitemap", new XElement("loc", "https://www.roleplayersguild.com/sitemap/universes")) // Added
            )
        );

        return Content(sitemap.ToString(), "application/xml", Encoding.UTF8);
    }
}