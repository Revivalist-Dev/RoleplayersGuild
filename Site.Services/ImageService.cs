using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using RoleplayersGuild.Site.Services.Models;
using RoleplayersGuild.Site.Services.DataServices;

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
        private readonly IUserDataService _userDataService;

        public ImageService(IConfiguration config, IWebHostEnvironment webHostEnvironment,
                            IOptions<AwsSettings> awsSettings, IOptions<ImageSettings> imageSettings,
                            ILogger<ImageService> logger, IUserDataService userDataService, IAmazonS3? s3Client = null)
        {
            _webHostEnvironment = webHostEnvironment;
            _imageHandlingMode = config.GetValue<string>("ImageHandling", "S3")!;
            _awsSettings = awsSettings.Value;
            _imageSettings = imageSettings.Value;
            _s3Client = s3Client;
            _logger = logger;
            _userDataService = userDataService;
        }

        private string GetImageTypeSubfolder(string imageType) => imageType.ToLowerInvariant() switch
        {
            "avatar" => _awsSettings.AvatarsFolder,
            "card" => _awsSettings.CardsFolder,
            "inline" => _awsSettings.InlinesFolder,
            _ => _awsSettings.ImagesFolder,
        };

        public async Task<(ImageUploadPath? path, int width, int height)> UploadImageAsync(IFormFile uploadedFile, int userId, int characterId, string imageType)
        {
            if (uploadedFile is null || uploadedFile.Length == 0 || !IsSupportedImageType(uploadedFile.ContentType))
            {
                _logger.LogWarning("UploadImageAsync failed due to invalid file (null, zero length, or unsupported type).");
                return (null, 0, 0);
            }

            var membershipTypeId = await _userDataService.GetMembershipTypeIdAsync(userId);
            var maxFileSizeMb = GetMaxFileSizeForMembership(membershipTypeId);
            if (uploadedFile.Length > maxFileSizeMb * 1024 * 1024)
            {
                _logger.LogWarning("UploadImageAsync failed for user {UserId} due to file size {FileSize} exceeding limit of {MaxFileSize} MB.", userId, uploadedFile.Length, maxFileSizeMb);
                // Returning a specific error code or message could be useful here for the frontend.
                return (null, 0, 0);
            }

            var fileExtension = Path.GetExtension(uploadedFile.FileName).ToLowerInvariant();
            var uniqueFileName = $"{DateTime.UtcNow.Ticks}{fileExtension}";

            var imageTypeSubfolder = GetImageTypeSubfolder(imageType);
            var relativePath = Path.Combine(userId.ToString(), characterId.ToString(), imageTypeSubfolder, uniqueFileName);
            int width = 0;
            int height = 0;

            try
            {
                await using var stream = uploadedFile.OpenReadStream();
                using var image = await Image.LoadAsync(stream);
                width = image.Width;
                height = image.Height;
                stream.Position = 0; // Reset stream position after reading dimensions

                if (_imageHandlingMode.Equals("Local", StringComparison.OrdinalIgnoreCase))
                {
                    var fullSavePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "UserFiles", relativePath);
                    await ResizeAndSaveLocallyAsync(image, fullSavePath, 1200, 1200);
                }
                else // S3
                {
                    var s3Key = Path.Combine("images", "UserFiles", relativePath).Replace('\\', '/');
                    await ResizeAndUploadToS3Async(image, s3Key, 1200, 1200);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occurred during image upload for path {RelativePath}.", relativePath);
                return (null, 0, 0);
            }

            return (new ImageUploadPath(relativePath), width, height);
        }

        public async Task DeleteImageAsync(ImageUploadPath? storedPath)
        {
            if (storedPath is null || string.IsNullOrEmpty(storedPath.Path) || storedPath.Path.Contains("NewCharacter.png") || storedPath.Path.Contains("NewAvatar.png"))
            {
                return;
            }
            
            var cleanPath = storedPath.Path.Replace('\\', '/');
            const string pathSegment = "images/UserFiles/";
            if (cleanPath.Contains(pathSegment))
            {
                cleanPath = cleanPath.Substring(cleanPath.LastIndexOf(pathSegment) + pathSegment.Length);
            }
            cleanPath = cleanPath.TrimStart('/');


            if (_imageHandlingMode.Equals("Local", StringComparison.OrdinalIgnoreCase))
            {
                var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "UserFiles", cleanPath);
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    _logger.LogInformation("Deleted local file: {FullPath}", fullPath);
                }
            }
            else // S3
            {
                if (_s3Client is null) throw new InvalidOperationException("S3 client is not configured.");
                var s3Key = Path.Combine("images", "UserFiles", cleanPath).Replace('\\', '/');
                await _s3Client.DeleteObjectAsync(_awsSettings.BucketName, s3Key);
                _logger.LogInformation("Deleted S3 object: {BucketName}/{S3Key}", _awsSettings.BucketName, s3Key);
            }
        }

        private int GetMaxFileSizeForMembership(int membershipTypeId) => membershipTypeId switch
        {
            1 => _imageSettings.BronzeMaxFileSizeMb,
            2 => _imageSettings.SilverMaxFileSizeMb,
            3 => _imageSettings.GoldMaxFileSizeMb,
            4 => _imageSettings.PlatinumMaxFileSizeMb,
            _ => _imageSettings.MaxFileSizeMb,
        };

        private async Task ResizeAndSaveLocallyAsync(Image image, string fullPath, int maxWidth, int maxHeight)
        {
            var directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }

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
