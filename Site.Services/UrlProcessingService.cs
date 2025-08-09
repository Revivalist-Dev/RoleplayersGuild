using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using RoleplayersGuild.Project.Configuration;
using RoleplayersGuild.Site.Services.Models;

namespace RoleplayersGuild.Site.Services
{
    public class UrlProcessingService : IUrlProcessingService
    {
        private readonly string _imageHandlingMode;
        private readonly AwsSettings _awsSettings;

        public UrlProcessingService(IConfiguration config, IOptions<AwsSettings> awsSettings)
        {
            _imageHandlingMode = config.GetValue<string>("ImageHandling", "S3")!;
            _awsSettings = awsSettings.Value;
        }

        public string GetCharacterImageUrl(ImageUploadPath? storedPathInfo)
        {
            var storedPath = storedPathInfo?.Path?.Replace('\\', '/');
            if (string.IsNullOrEmpty(storedPath))
            {
                return "/images/Defaults/NewCharacter.png";
            }

            if (storedPath.StartsWith("http"))
            {
                return storedPath;
            }

            const string pathSegment = "images/UserFiles/";
            if (storedPath.Contains(pathSegment))
            {
                storedPath = storedPath.Substring(storedPath.LastIndexOf(pathSegment) + pathSegment.Length);
            }
            storedPath = storedPath.TrimStart('/');

            if (storedPath.Contains("NewCharacter.png"))
            {
                return "/images/Defaults/NewCharacter.png";
            }
            if (storedPath.Contains("NewAvatar.png"))
            {
                return "/images/Defaults/NewAvatar.png";
            }

            if (_imageHandlingMode.Equals("Local", System.StringComparison.OrdinalIgnoreCase))
            {
                return $"/{pathSegment}{storedPath}";
            }
            else // S3
            {
                if (string.IsNullOrEmpty(_awsSettings.CloudFrontDomain)) return "/images/Defaults/NewCharacter.png";
                return $"{_awsSettings.CloudFrontDomain.TrimEnd('/')}/{pathSegment}{storedPath}";
            }
        }
    }
}
