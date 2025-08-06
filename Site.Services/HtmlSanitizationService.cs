using Ganss.Xss;

namespace RoleplayersGuild.Site.Services
{
    public class HtmlSanitizationService : IHtmlSanitizationService
    {
        private readonly IHtmlSanitizer _sanitizer;

        public HtmlSanitizationService()
        {
            // Configure the sanitizer
            _sanitizer = new HtmlSanitizer();

            // Define a whitelist of allowed tags and attributes
            _sanitizer.AllowedTags.Clear();
            _sanitizer.AllowedTags.Add("b");
            _sanitizer.AllowedTags.Add("i");
            _sanitizer.AllowedTags.Add("u");
            _sanitizer.AllowedTags.Add("p");
            _sanitizer.AllowedTags.Add("br");
            _sanitizer.AllowedTags.Add("div");
            _sanitizer.AllowedTags.Add("span");
            _sanitizer.AllowedTags.Add("h1");
            _sanitizer.AllowedTags.Add("h2");
            _sanitizer.AllowedTags.Add("h3");
            _sanitizer.AllowedTags.Add("a");
            _sanitizer.AllowedTags.Add("img");

            _sanitizer.AllowedAttributes.Clear();
            _sanitizer.AllowedAttributes.Add("class");
            _sanitizer.AllowedAttributes.Add("id");
            _sanitizer.AllowedAttributes.Add("style");
            _sanitizer.AllowedAttributes.Add("href"); // Only for <a> tags
            _sanitizer.AllowedAttributes.Add("src");  // Only for <img> tags
            _sanitizer.AllowedAttributes.Add("alt");  // Only for <img> tags

            // Allow specific, safe CSS properties
            _sanitizer.AllowedCssProperties.Clear();
            _sanitizer.AllowedCssProperties.Add("color");
            _sanitizer.AllowedCssProperties.Add("background-color");
            _sanitizer.AllowedCssProperties.Add("font-weight");
            _sanitizer.AllowedCssProperties.Add("font-size");
            _sanitizer.AllowedCssProperties.Add("text-align");
        }

        public string Sanitize(string? unsafeHtml)
        {
            if (string.IsNullOrEmpty(unsafeHtml))
            {
                return string.Empty;
            }
            return _sanitizer.Sanitize(unsafeHtml);
        }
    }
}