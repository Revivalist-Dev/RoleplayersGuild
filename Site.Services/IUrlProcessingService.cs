using RoleplayersGuild.Site.Services.Models;

namespace RoleplayersGuild.Site.Services
{
    public interface IUrlProcessingService
    {
        string GetCharacterImageUrl(ImageUploadPath? storedPath);
    }
}
