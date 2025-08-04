// In F:\...\Project.Configuration\ReCaptchaSettings.cs

using System.ComponentModel.DataAnnotations;

namespace RoleplayersGuild.Project.Configuration
{
    public class RecaptchaSettings
    {
        [Required(ErrorMessage = "reCAPTCHA SiteKey is required.")]
        public string SiteKey { get; set; } = string.Empty;

        [Required(ErrorMessage = "reCAPTCHA SecretKey is required.")]
        public string SecretKey { get; set; } = string.Empty;
    }
}