using Microsoft.AspNetCore.Html;

namespace RoleplayersGuild.Site.Services
{
    public interface IViteManifestService
    {
        Task<IHtmlContent> RenderViteStyles(string entryPoint = "src/main.tsx");
        Task<IHtmlContent> RenderViteScripts(string entryPoint = "src/main.tsx");
    }
}