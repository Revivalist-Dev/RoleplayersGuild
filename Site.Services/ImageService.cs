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
        private readonly ImageSettings _imageSettings;
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
            _imageSettings = imageSettings.Value;
            _s3Client = s3Client;
            _logger = logger;
        }

        public string? GetImageUrl(string? imageName, string imageType)
        {
            if (string.IsNullOrEmpty(imageName))
            {
                return null;
            }

            if (_imageHandlingMode.Equals("Local", StringComparison.OrdinalIgnoreCase))
            {
                string displayFolder = imageType.ToLowerInvariant() switch
                {
                    "avatar" => _imageSettings.DisplayCharacterAvatarsFolder,
                    "card" => _imageSettings.DisplayCharacterCardsFolder,
                    "inline" => _imageSettings.DisplayCharacterInlinesFolder,
                    _ => _imageSettings.DisplayCharacterImagesFolder,
                };
                return $"{displayFolder.TrimEnd('/')}/{imageName}";
            }
            else // S3
            {
                if (string.IsNullOrEmpty(_awsSettings.CloudFrontDomain))
                {
                    return null;
                }

                string s3Folder = imageType.ToLowerInvariant() switch
                {
                    "avatar" => _awsSettings.CharacterAvatarsFolder,
                    "card" => _awsSettings.CharacterCardsFolder,
                    "inline" => _awsSettings.CharacterInlinesFolder,
                    _ => _awsSettings.CharacterImagesFolder,
                };

                return $"{_awsSettings.CloudFrontDomain.TrimEnd('/')}/{s3Folder.TrimEnd('/')}/{imageName}";
            }
        }

        public async Task<string?> UploadImageAsync(IFormFile uploadedFile, string? subfolder = "images")
        {
            if (uploadedFile is null)
            {
                _logger.LogWarning("UploadImageAsync failed: IFormFile was null.");
                return null;
            }
            if (uploadedFile.Length == 0)
            {
                _logger.LogWarning("UploadImageAsync failed: File length was 0.");
                return null;
            }
            if (!IsSupportedImageType(uploadedFile.ContentType))
            {
                _logger.LogWarning("UploadImageAsync failed: Unsupported content type '{ContentType}'.", uploadedFile.ContentType);
                return null;
            }

            var fileExtension = Path.GetExtension(uploadedFile.FileName).ToLowerInvariant();
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";

            try
            {
                if (_imageHandlingMode.Equals("Local", StringComparison.OrdinalIgnoreCase))
                {
                    var storagePath = GetLocalStoragePath(subfolder);
                    var fullSavePath = Path.Combine(_webHostEnvironment.ContentRootPath, storagePath, uniqueFileName);
                    await ResizeAndSaveLocallyAsync(uploadedFile, fullSavePath, 1200, 1200);
                }
                else // S3 is the default
                {
                    _logger.LogInformation("Attempting to upload to S3 bucket: {BucketName}", _awsSettings.BucketName);
                    var s3Folder = GetS3Folder(subfolder);
                    await using var stream = uploadedFile.OpenReadStream();
                    using var image = await Image.LoadAsync(stream);
                    await ResizeAndUploadToS3Async(image, s3Folder, uniqueFileName, 1200, 1200);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occurred during image upload and processing.");
                return null;
            }

            return uniqueFileName;
        }

        public async Task DeleteImageAsync(string imageName)
        {
            if (string.IsNullOrEmpty(imageName)) return;

            var possibleSubfolders = new[] { "images", "inlines", "avatars", "cards" };

            if (_imageHandlingMode.Equals("Local", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var subfolder in possibleSubfolders)
                {
                    var storagePath = GetLocalStoragePath(subfolder);
                    var fullPath = Path.Combine(_webHostEnvironment.ContentRootPath, storagePath, imageName);
                    if (File.Exists(fullPath))
                    {
                        File.Delete(fullPath);
                        _logger.LogInformation("Deleted local file: {FullPath}", fullPath);
                        return;
                    }
                }
            }
            else // S3 is the default
            {
                if (_s3Client is null) throw new InvalidOperationException("S3 client is not configured.");

                foreach (var subfolder in possibleSubfolders)
                {
                    var s3Folder = GetS3Folder(subfolder);
                    try
                    {
                        var s3Key = s3Folder.EndsWith('/') ? $"{s3Folder}{imageName}" : $"{s3Folder}/{imageName}";
                        await _s3Client.DeleteObjectAsync(_awsSettings.BucketName, s3Key);
                        _logger.LogInformation("Deleted S3 object: {BucketName}/{S3Key}", _awsSettings.BucketName, s3Key);
                        return;
                    }
                    catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        // Object not found in this folder, continue checking others
                    }
                }
            }
        }

        private async Task ResizeAndSaveLocallyAsync(IFormFile file, string fullPath, int maxWidth, int maxHeight)
        {
            var directory = Path.GetDirectoryName(fullPath);

            if (!string.IsNullOrEmpty(directory))
            {
                // FIX: Specify the full namespace to resolve ambiguity.
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

        private async Task ResizeAndUploadToS3Async(Image originalImage, string s3Folder, string fileName, int maxWidth, int maxHeight)
        {
            if (_s3Client is null) throw new InvalidOperationException("S3 client is not configured.");

            await using var memoryStream = new MemoryStream();
            using var resizedImage = originalImage.Clone(ctx => ctx.Resize(new ResizeOptions
            {
                Size = new Size(maxWidth, maxHeight),
                Mode = ResizeMode.Max
            }));
            await resizedImage.SaveAsync(memoryStream, resizedImage.Metadata.DecodedImageFormat!);
            memoryStream.Position = 0;

            var fileTransferUtility = new TransferUtility(_s3Client);
            var s3Key = s3Folder.EndsWith('/') ? $"{s3Folder}{fileName}" : $"{s3Folder}/{fileName}";

            await fileTransferUtility.UploadAsync(memoryStream, _awsSettings.BucketName, s3Key);
        }

        private string GetLocalStoragePath(string? subfolder) => subfolder?.ToLowerInvariant() switch
        {
            "avatars" => _imageSettings.LocalAvatarsStoragePath,
            "cards" => _imageSettings.LocalCardsStoragePath,
            "inlines" => _imageSettings.LocalInlinesStoragePath,
            _ => _imageSettings.LocalImagesStoragePath,
        };

        private string GetS3Folder(string? subfolder) => subfolder?.ToLowerInvariant() switch
        {
            "avatars" => _awsSettings.CharacterAvatarsFolder,
            "cards" => _awsSettings.CharacterCardsFolder,
            "inlines" => _awsSettings.CharacterInlinesFolder,
            _ => _awsSettings.CharacterImagesFolder,
        };

        private static bool IsSupportedImageType(string contentType) =>
            new[] { "image/jpeg", "image/png", "image/gif", "image/webp" }.Contains(contentType.ToLowerInvariant());
    }
}