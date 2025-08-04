using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Services
{
    public interface IImageService
    {
        /// <summary>
        /// Handles the upload, resizing, and saving of an image file.
        /// </summary>
        /// <param name="uploadedFile">The IFormFile object from the request.</param>
        /// <param name="subfolder">The logical subfolder (e.g., "avatars", "cards"). Defaults to "images".</param>
        /// <returns>The unique filename of the saved image, or null if the upload failed.</returns>
        // FIX: Default value changed from null to "images" to match the implementation.
        Task<string?> UploadImageAsync(IFormFile uploadedFile, string? subfolder = "images");

        /// <summary>
        /// Deletes an image from storage.
        /// </summary>
        /// <param name="imageName">The unique filename of the image to delete.</param>
        Task DeleteImageAsync(string imageName);

        /// <summary>
        /// Constructs the full, web-accessible URL for an image.
        /// </summary>
        /// <param name="imageName">The unique file name of the image (from the database).</param>
        /// <param name="imageType">The type of image (e.g., "avatar", "card", "gallery").</param>
        /// <returns>A full URL for the image, or null if the imageName is empty.</returns>
        string? GetImageUrl(string? imageName, string imageType);
    }
}