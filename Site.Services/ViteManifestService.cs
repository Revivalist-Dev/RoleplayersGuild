using Microsoft.AspNetCore.Html;
using System.IO;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace RoleplayersGuild.Site.Services
{
    public class ViteManifestService : IViteManifest
    {
        private readonly IWebHostEnvironment _env;
        private JsonDocument? _manifest;

        public ViteManifestService(IWebHostEnvironment env)
        {
            _env = env;
            if (!_env.IsDevelopment())
            {
                _manifest = ReadManifest();
            }
        }

        public IHtmlContent GetScriptTag(string entryPoint = "src/main.tsx")
        {
            if (_env.IsDevelopment())
            {
                var devScripts = new StringBuilder();
                devScripts.AppendLine(@"<script type=""module"">
                                import RefreshRuntime from ""/vite-dev/@react-refresh"";
                                RefreshRuntime.injectIntoGlobalHook(window);
                                window.$RefreshReg$ = () => {};
                                window.$RefreshSig$ = () => (type) => type;
                                window.__vite_plugin_react_preamble_installed__ = true;
                            </script>");
                devScripts.AppendLine(@"<script type=""module"" src=""/vite-dev/@vite/client""></script>");
                devScripts.AppendLine($@"<script type=""module"" src=""/vite-dev/{entryPoint}""></script>");
                return new HtmlString(devScripts.ToString());
            }

            _manifest ??= ReadManifest();
            var entry = _manifest.RootElement.GetProperty(entryPoint);
            var file = entry.GetProperty("file").GetString();
            return new HtmlString($"<script type=\"module\" src=\"/react-dist/{file}\"></script>");
        }

        public IHtmlContent GetStyleTag(string entryPoint = "src/main.tsx")
        {
            if (_env.IsDevelopment())
            {
                return new HtmlString("");
            }

            _manifest ??= ReadManifest();
            var entry = _manifest.RootElement.GetProperty(entryPoint);
            if (!entry.TryGetProperty("css", out var cssElement))
            {
                return new HtmlString("");
            }
            var file = cssElement.EnumerateArray().First().GetString();
            return new HtmlString($"<link rel=\"stylesheet\" href=\"/react-dist/{file}\" />");
        }

        private JsonDocument ReadManifest()
        {
            // CORRECTED: The path now includes the ".vite" subfolder where the manifest is located.
            var manifestPath = Path.Combine(_env.WebRootPath, "react-dist", "manifest.json");
            if (!File.Exists(manifestPath))
            {
                throw new FileNotFoundException("Vite manifest.json not found. Run `npm run build` in Site.Client.", manifestPath);
            }
            return JsonDocument.Parse(File.ReadAllText(manifestPath));
        }
    }
}