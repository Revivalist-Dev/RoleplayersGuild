using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace RoleplayersGuild.Site.Services
{
    public class ViteManifestService : IViteManifestService
    {
        private readonly IWebHostEnvironment _env;
        private readonly string _manifestPath;
        private readonly ViteSettings _viteSettings;
        private JsonDocument? _manifest;

        public ViteManifestService(IWebHostEnvironment env, IOptions<ViteSettings> viteSettings)
        {
            _env = env;
            _manifestPath = Path.Combine(_env.WebRootPath, "Assets", "manifest.json");
            _viteSettings = viteSettings.Value;
        }

        public async Task<IHtmlContent> RenderViteStyles(string entryPoint = "src/main.tsx")
        {
            var assets = new StringBuilder();
            const string cssEntryPoint = "Site.Assets/styles/scss/site.scss";

            // --- DEVELOPMENT ---
            if (_env.IsDevelopment())
            {
                // In development, Vite injects styles automatically. Return nothing.
                return new HtmlString(string.Empty);
            }
            // --- PRODUCTION ---
            else
            {
                // In production, we find the compiled CSS file from the manifest.
                var manifest = await GetManifestAsync();
                // NOTE: We look for the entry point directly, not a 'css' property within another entry.
                if (manifest.RootElement.TryGetProperty(cssEntryPoint, out var entryChunk) &&
                    entryChunk.TryGetProperty("file", out var cssFile))
                {
                    var baseUrl = "/Assets/";
                    assets.AppendLine($@"<link rel=""stylesheet"" href=""{baseUrl}{cssFile.GetString()}"">");
                }
            }

            return new HtmlString(assets.ToString());
        }

        public async Task<IHtmlContent> RenderViteScripts(string entryPoint = "src/main.tsx")
        {
            var assets = new StringBuilder();
            // NOTE: The service is hardened to always use its own list of entry points,
            // ignoring the 'entryPoint' parameter from the Razor view. This prevents
            // incorrect script injection and ensures stability.
            var entryPoints = new[] { "src/main.tsx" };


            // --- DEVELOPMENT ---
            if (_env.IsDevelopment())
            {
                var viteDevServerBase = _viteSettings.DevServerProxy;
                assets.AppendLine($@"<script type=""module"">
                    import RefreshRuntime from ""{viteDevServerBase}/@react-refresh"";
                    RefreshRuntime.injectIntoGlobalHook(window);
                    window.$RefreshReg$ = () => {{}};
                    window.$RefreshSig$ = () => (type) => type;
                    window.__vite_plugin_react_preamble_installed__ = true;
                </script>");
                assets.AppendLine($@"<script type=""module"" src=""{viteDevServerBase}/@vite/client""></script>");

                foreach (var point in entryPoints)
                {
                    assets.AppendLine($@"<script type=""module"" src=""{viteDevServerBase}/{point}""></script>");
                }
            }
            // --- PRODUCTION ---
            else
            {
                var manifest = await GetManifestAsync();
                var baseUrl = "/Assets/";

                foreach (var point in entryPoints)
                {
                    if (manifest.RootElement.TryGetProperty(point, out var entryChunk) &&
                        entryChunk.TryGetProperty("file", out var jsFile))
                    {
                        assets.AppendLine($@"<script type=""module"" src=""{baseUrl}{jsFile.GetString()}""></script>");
                    }
                }
            }

            return new HtmlString(assets.ToString());
        }

        // FIXME: This method was fundamentally flawed, attempting to load an SCSS file as a JS module.
        // It caused direct network requests to source files, bypassing the Vite bundler.
        // It has been deprecated and should be removed in a future cleanup.
        //public async Task<IHtmlContent> RenderBBFrameStyles()
        //{
        //    var assets = new StringBuilder();
        //    const string cssEntryPoint = "@/styles/scss/_bbframe.scss";
        //
        //    // --- DEVELOPMENT ---
        //    if (_env.IsDevelopment())
        //    {
        //        var viteDevServerBase = _viteSettings.DevServerProxy;
        //        assets.AppendLine($@"<script type=""module"" src=""{viteDevServerBase}/{cssEntryPoint}""></script>");
        //        return new HtmlString(assets.ToString());
        //    }
        //    // --- PRODUCTION ---
        //    else
        //    {
        //        var manifest = await GetManifestAsync();
        //        if (manifest.RootElement.TryGetProperty(cssEntryPoint, out var entryChunk) &&
        //            entryChunk.TryGetProperty("file", out var cssFile))
        //        {
        //            var baseUrl = "/Assets/";
        //            assets.AppendLine($@"<link rel=""stylesheet"" href=""{baseUrl}{cssFile.GetString()}"">");
        //        }
        //    }
        //
        //    return new HtmlString(assets.ToString());
        //}
 
        private async Task<JsonDocument> GetManifestAsync()
        {
            if (_manifest != null)
            {
                return _manifest;
            }
 
            if (!File.Exists(_manifestPath))
            {
                throw new FileNotFoundException("Vite manifest not found. Run `npm run build`.", _manifestPath);
            }
 
            _manifest = await JsonDocument.ParseAsync(File.OpenRead(_manifestPath));
            return _manifest;
        }
    }
}
