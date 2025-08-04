using System.ComponentModel.DataAnnotations;

namespace RoleplayersGuild.Project.Configuration // Or appropriate namespace
{
    public class SmtpSettings
    {
        [Required(ErrorMessage = "SMTP Password is required.")]
        public string Password { get; set; } = string.Empty;
        // Add other SMTP settings like Host, Port, Username, FromEmail, etc.
        // [Required] public string Host { get; set; } = string.Empty;
        // [Required] public int Port { get; set; }
        // [Required] public string Username { get; set; } = string.Empty;
        // [Required] public string FromEmail { get; set; } = string.Empty;
    }
}