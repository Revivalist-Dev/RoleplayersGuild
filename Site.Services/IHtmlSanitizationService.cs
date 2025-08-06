namespace RoleplayersGuild.Site.Services
{
    public interface IHtmlSanitizationService
    {
        string Sanitize(string? unsafeHtml);
    }
}