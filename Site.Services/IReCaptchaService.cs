namespace RoleplayersGuild.Site.Services
{
    public interface IReCaptchaService
    {
        Task<ReCaptchaResult> ValidateAsync(string recaptchaToken);
    }
}