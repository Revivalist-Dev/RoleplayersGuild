using RoleplayersGuild.Site.Services.Models;

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
        /// <returns>A tuple containing the relative storage path, width, and height of the saved image, or (null, 0, 0) if upload failed.</returns>
        Task<(ImageUploadPath? path, int width, int height)> UploadImageAsync(IFormFile uploadedFile, int userId, int characterId, string imageType);

        /// <summary>
        /// Deletes an image from storage using its full relative path.
        /// </summary>
        /// <param name="storedPath">The full relative path of the image to delete (from the database).</param>
        Task DeleteImageAsync(ImageUploadPath? storedPath);

    }
}
