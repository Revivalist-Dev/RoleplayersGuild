using System.ComponentModel.DataAnnotations;

namespace RoleplayersGuild.Project.Configuration
{
    public class AwsSettings
    {
        public string Region { get; set; } = string.Empty;
        public string? ServiceURL { get; set; }
        public string? AccessKeyId { get; set; }
        public string? SecretAccessKey { get; set; }

        [Required(ErrorMessage = "AWS S3 Bucket Name is required.")]
        public string BucketName { get; set; } = string.Empty;

        public string? CloudFrontDomain { get; set; }

        public string ImagesFolder { get; set; } = string.Empty;
        public string InlinesFolder { get; set; } = string.Empty;
        public string AvatarsFolder { get; set; } = string.Empty;
        public string CardsFolder { get; set; } = string.Empty;
    }
}