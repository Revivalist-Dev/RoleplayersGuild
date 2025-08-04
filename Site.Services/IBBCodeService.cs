// F:\Visual Studio\RoleplayersGuild\Site.Services\IBBCodeService.cs

using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Services
{
    public interface IBBCodeService
    {
        Task<string> ParseAsync(string bbcode, int characterId);
    }
}