using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RoleplayersGuild.Project.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Services
{
    public class ImageService : IImageService
    {
        private readonly string _imageHandlingMode;
        private readonly AwsSettings _awsSettings;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IAmazonS3? _s3Client;
        private readonly ILogger<ImageService> _logger;

        public ImageService(IConfiguration config, IWebHostEnvironment webHostEnvironment,
                            IOptions<AwsSettings> awsSettings, IOptions<ImageSettings> imageSettings,
                            ILogger<ImageService> logger, IAmazonS3? s3Client = null)
        {
            _webHostEnvironment = webHostEnvironment;
            _imageHandlingMode = config.GetValue<string>("ImageHandling", "S3")!;
            _awsSettings = awsSettings.Value;
            // imageSettings is no longer used for pathing but might be for other rules.
            _s3Client = s3Client;
            _logger = logger;
        }

        private string GetImageTypeSubfolder(string imageType) => imageType.ToLowerInvariant() switch
        {
            "avatar" => "Avatars",
            "card" => "Cards",
            "inline" => "Inlines",
            _ => "Images", // Default for gallery images
        };

        public async Task<string?> UploadImageAsync(IFormFile uploadedFile, int userId, int characterId, string imageType)
        {
            if (uploadedFile is null || uploadedFile.Length == 0 || !IsSupportedImageType(uploadedFile.ContentType))
            {
                _logger.LogWarning("UploadImageAsync failed due to invalid file.");
                return null;
            }

            var fileExtension = Path.GetExtension(uploadedFile.FileName).ToLowerInvariant();
            var uniqueFileName = $"{DateTime.UtcNow.Ticks}{fileExtension}";

            var imageTypeSubfolder = GetImageTypeSubfolder(imageType);
            var relativePath = Path.Combine(userId.ToString(), characterId.ToString(), imageTypeSubfolder, uniqueFileName);

            try
            {
                if (_imageHandlingMode.Equals("Local", StringComparison.OrdinalIgnoreCase))
                {
                    var fullSavePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "UserFiles", relativePath);
                    await ResizeAndSaveLocallyAsync(uploadedFile, fullSavePath, 1200, 1200);
                }
                else // S3
                {
                    var s3Key = Path.Combine("UserFiles", relativePath).Replace('\\', '/');
                    await using var stream = uploadedFile.OpenReadStream();
                    using var image = await Image.LoadAsync(stream);
                    await ResizeAndUploadToS3Async(image, s3Key, 1200, 1200);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occurred during image upload for path {RelativePath}.", relativePath);
                return null;
            }

            return relativePath;
        }

        public string? GetImageUrl(string? storedPath)
        {
            // If the path is null or empty, return the generic default image URL.
            if (string.IsNullOrEmpty(storedPath))
            {
                return "/images/Defaults/NewCharacter.png";
            }

            // Handle specific default image filenames by pointing to their new location.
            if (storedPath.Contains("NewCharacter.png"))
            {
                return "/images/Defaults/NewCharacter.png";
            }
            if (storedPath.Contains("NewAvatar.png"))
            {
                return "/images/Defaults/NewAvatar.png";
            }

            // For all other paths, build the URL based on the storage mode.
            if (_imageHandlingMode.Equals("Local", StringComparison.OrdinalIgnoreCase))
            {
                return $"/images/UserFiles/{storedPath.Replace('\\', '/')}";
            }
            else // S3
            {
                if (string.IsNullOrEmpty(_awsSettings.CloudFrontDomain)) return "/images/Defaults/NewCharacter.png";
                return $"{_awsSettings.CloudFrontDomain.TrimEnd('/')}/UserFiles/{storedPath.Replace('\\', '/')}";
            }
        }

        public async Task DeleteImageAsync(string? storedPath)
        {
            if (string.IsNullOrEmpty(storedPath) || storedPath.Contains("NewCharacter.png") || storedPath.Contains("NewAvatar.png"))
            {
                return;
            }

            if (_imageHandlingMode.Equals("Local", StringComparison.OrdinalIgnoreCase))
            {
                var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "UserFiles", storedPath);
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    _logger.LogInformation("Deleted local file: {FullPath}", fullPath);
                }
            }
            else // S3
            {
                if (_s3Client is null) throw new InvalidOperationException("S3 client is not configured.");
                var s3Key = Path.Combine("UserFiles", storedPath).Replace('\\', '/');
                await _s3Client.DeleteObjectAsync(_awsSettings.BucketName, s3Key);
                _logger.LogInformation("Deleted S3 object: {BucketName}/{S3Key}", _awsSettings.BucketName, s3Key);
            }
        }

        private async Task ResizeAndSaveLocallyAsync(IFormFile file, string fullPath, int maxWidth, int maxHeight)
        {
            var directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }

            await using var stream = file.OpenReadStream();
            using var image = await Image.LoadAsync(stream);
            image.Mutate(ctx => ctx.Resize(new ResizeOptions
            {
                Size = new Size(maxWidth, maxHeight),
                Mode = ResizeMode.Max
            }));
            await image.SaveAsync(fullPath);
        }

        private async Task ResizeAndUploadToS3Async(Image originalImage, string s3Key, int maxWidth, int maxHeight)
        {
            if (_s3Client is null) throw new InvalidOperationException("S3 client not configured for upload.");

            originalImage.Mutate(ctx => ctx.Resize(new ResizeOptions
            {
                Size = new Size(maxWidth, maxHeight),
                Mode = ResizeMode.Max
            }));

            await using var ms = new MemoryStream();
            await originalImage.SaveAsync(ms, originalImage.Metadata.DecodedImageFormat!);
            ms.Position = 0;

            var fileTransferUtility = new TransferUtility(_s3Client);
            await fileTransferUtility.UploadAsync(ms, _awsSettings.BucketName, s3Key);
        }

        private static bool IsSupportedImageType(string contentType) =>
            new[] { "image/jpeg", "image/png", "image/gif", "image/webp" }.Contains(contentType.ToLowerInvariant());
    }
}