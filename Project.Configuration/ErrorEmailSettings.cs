using System.ComponentModel.DataAnnotations;

namespace RoleplayersGuild.Project.Configuration // Or appropriate namespace
{
    public class ErrorEmailSettings
    {
        [Required(ErrorMessage = "Error Email Password is required.")]
        public string Password { get; set; } = string.Empty;
        // Add other error email settings like ToAddress, FromAddress, etc.
        // [Required] public string ToAddress { get; set; } = string.Empty;
        // [Required] public string FromAddress { get; set; } = string.Empty;
    }
}