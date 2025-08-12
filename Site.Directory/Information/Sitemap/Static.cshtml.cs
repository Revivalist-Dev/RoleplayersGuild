using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Xml.Linq;

namespace RoleplayersGuild.Site.Directory.Information.Sitemap;

public class StaticModel : PageModel
{
    public IActionResult OnGet()
    {
        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        var urlset = new XElement(ns + "urlset",
            new XElement(ns + "url",
                new XElement(ns + "loc", "https://www.roleplayersguild.com/"),
                new XElement(ns + "changefreq", "daily"),
                new XElement(ns + "priority", "1.0")
            ),
            // ... your other static URLs ...
            new XElement(ns + "url",
                new XElement(ns + "loc", "https://www.roleplayersguild.com/Account/Register"),
                new XElement(ns + "changefreq", "monthly"),
                new XElement(ns + "priority", "0.8")
            )
        );

        var sitemap = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), urlset);
        return Content(sitemap.ToString(), "application/xml", Encoding.UTF8);
    }
}