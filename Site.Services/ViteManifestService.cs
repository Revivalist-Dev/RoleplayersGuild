using Microsoft.AspNetCore.Html;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace RoleplayersGuild.Site.Services
{
    public class ViteManifestService : IViteManifestService
    {
        private readonly IWebHostEnvironment _env;
        private readonly string _manifestPath;
        private readonly IConfiguration _config;
        private JsonDocument? _manifest;

        public ViteManifestService(IWebHostEnvironment env, IConfiguration config)
        {
            _env = env;
            _manifestPath = Path.Combine(_env.WebRootPath, "react-dist", "manifest.json");
            _config = config;
        }

        public async Task<IHtmlContent> RenderViteStyles(string entryPoint = "src/main.tsx")
        {
            // In development, Vite injects styles via JavaScript, so we don't need to do anything.
            if (_env.IsDevelopment())
            {
                return HtmlString.Empty;
            }

            // In production, we render the <link> tag for the compiled CSS.
            var manifest = await GetManifestAsync();
            if (!manifest.RootElement.TryGetProperty(entryPoint, out var entryChunk) || !entryChunk.TryGetProperty("css", out var cssFiles))
            {
                return HtmlString.Empty;
            }

            var assets = new StringBuilder();
            var baseUrl = "/react-dist/";
            foreach (var cssFile in cssFiles.EnumerateArray())
            {
                assets.AppendLine($@"<link rel=""stylesheet"" href=""{baseUrl}{cssFile.GetString()}"">");
            }

            return new HtmlString(assets.ToString());
        }

        public async Task<IHtmlContent> RenderViteScripts(string entryPoint = "src/main.tsx")
        {
            var assets = new StringBuilder();

            // --- DEVELOPMENT ---
            if (_env.IsDevelopment())
            {
                // Use the proxy path from configuration. It must match the path in vite.config.ts and Program.cs
                var baseUrl = _config.GetValue<string>("Vite:DevServerProxy")?.TrimEnd('/') ?? "/vite-dev";
                assets.AppendLine($@"<script type=""module"" src=""{baseUrl}/@vite/client""></script>");
                assets.AppendLine($@"<script type=""module"" src=""{baseUrl}/{entryPoint}""></script>");
            }
            // --- PRODUCTION ---
            else
            {
                var manifest = await GetManifestAsync();
                if (manifest.RootElement.TryGetProperty(entryPoint, out var entryChunk) &&
                    entryChunk.TryGetProperty("file", out var jsFile))
                {
                    var baseUrl = "/react-dist/";
                    assets.AppendLine($@"<script type=""module"" src=""{baseUrl}{jsFile.GetString()}""></script>");
                }
            }

            return new HtmlString(assets.ToString());
        }

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
