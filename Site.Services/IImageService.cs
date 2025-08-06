using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Services
{
    public interface IImageService
    {
        /// <summary>
        /// Handles the upload, resizing, and saving of an image to a structured path.
        /// </summary>
        /// <param name="uploadedFile">The IFormFile object from the request.</param>
        /// <param name="userId">The ID of the user uploading the file.</param>
        /// <param name="characterId">The ID of the character the file belongs to.</param>
        /// <param name="imageType">The logical type of image (e.g., "avatar", "card", "gallery").</param>
        /// <returns>The relative storage path of the saved image (e.g., {userId}/{characterId}/Avatars/{ticks}.png), or null if upload failed.</returns>
        Task<string?> UploadImageAsync(IFormFile uploadedFile, int userId, int characterId, string imageType);

        /// <summary>
        /// Deletes an image from storage using its full relative path.
        /// </summary>
        /// <param name="storedPath">The full relative path of the image to delete (from the database).</param>
        Task DeleteImageAsync(string? storedPath);

        /// <summary>
        /// Constructs the full, web-accessible URL for an image.
        /// </summary>
        /// <param name="storedPath">The full relative path of the image (from the database).</param>
        /// <returns>A full URL for the image, or null if the path is empty.</returns>
        string? GetImageUrl(string? storedPath);
    }
}