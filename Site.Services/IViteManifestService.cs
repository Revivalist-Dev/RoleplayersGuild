using Microsoft.AspNetCore.Html;

namespace RoleplayersGuild.Site.Services
{
    public interface IViteManifest
    {
        IHtmlContent GetScriptTag(string entryPoint = "src/main.tsx");
        IHtmlContent GetStyleTag(string entryPoint = "src/main.tsx");
    }
}