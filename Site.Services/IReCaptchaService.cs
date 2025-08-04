// In F:\...\Site.Services\IReCaptchaService.cs

using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Services
{
    public interface IReCaptchaService
    {
        Task<ReCaptchaResult> ValidateAsync(string recaptchaToken);
    }
}