using System.ComponentModel.DataAnnotations;

namespace RoleplayersGuild.Project.Configuration
{
    public class AwsSettings
    {
        public string Region { get; set; } = string.Empty;

        [Required(ErrorMessage = "AWS S3 Bucket Name is required.")]
        public string BucketName { get; set; } = string.Empty;

        // ADD THIS LINE
        public string? CloudFrontDomain { get; set; }

        public string CharacterImagesFolder { get; set; } = string.Empty;
        public string CharacterInlinesFolder { get; set; } = string.Empty;
        public string CharacterAvatarsFolder { get; set; } = string.Empty;
        public string CharacterCardsFolder { get; set; } = string.Empty;
    }
}