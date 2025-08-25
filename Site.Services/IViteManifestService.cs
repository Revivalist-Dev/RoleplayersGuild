using Microsoft.AspNetCore.Html;

namespace RoleplayersGuild.Site.Services
{
    public interface IViteManifestService
    {
        Task<IHtmlContent> RenderViteStyles(string entryPoint = "src/main.tsx");
        Task<IHtmlContent> RenderViteScripts(string entryPoint = "src/main.tsx");
        // FIXME: This method was fundamentally flawed and caused direct requests to .scss files.
        // It has been deprecated and should be removed in a future cleanup.
        // Task<IHtmlContent> RenderBBFrameStyles();
    }
}