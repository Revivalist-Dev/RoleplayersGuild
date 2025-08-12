using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using RoleplayersGuild.Site.Services.DataServices;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using RoleplayersGuild.Site.Services.Models;

namespace RoleplayersGuild.Site.Controllers
{
    public class UpdateProfileInput
    {
        public string? ProfileCSS { get; set; }
        public string? ProfileHTML { get; set; }
        public bool IsEnabled { get; set; }
    }

    // ADDED CLASS
    public class UpdateBBFrameInput
    {
        public string? BBFrameContent { get; set; }
    }

    [ApiController]
    [Route("api/characters")]
    public class CharactersApiController(
        ICharacterDataService characterDataService,
        IUserService userService,
        IImageService imageService,
        IHtmlSanitizationService htmlSanitizer,
        IMiscDataService miscDataService,
        IUrlProcessingService urlProcessingService) : ControllerBase
    {
        private readonly ICharacterDataService _characterDataService = characterDataService;
        private readonly IUserService _userService = userService;
        private readonly IMiscDataService _miscDataService = miscDataService;
        private readonly IImageService _imageService = imageService;
        private readonly IHtmlSanitizationService _htmlSanitizer = htmlSanitizer;
        private readonly IUrlProcessingService _urlProcessingService = urlProcessingService;

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCharacterForEdit(int id)
        {
            var userId = _userService.GetUserId(User);
            if (userId == 0) return Unauthorized();

            var character = await _characterDataService.GetCharacterForEditAsync(id, userId);
            if (character == null) return Forbid();

            var genres = await _characterDataService.GetCharacterGenresAsync(id);
            var images = (await _characterDataService.GetCharacterImagesForGalleryAsync(id))
                .Select(img => {
                    img.CharacterImageUrl = _urlProcessingService.GetCharacterImageUrl((ImageUploadPath)img.CharacterImageUrl);
                    return img;
                }).ToList();
            var inlines = (await _characterDataService.GetInlineImagesAsync(id))
                .Select(inl => {
                    inl.InlineImageUrl = _urlProcessingService.GetCharacterImageUrl((ImageUploadPath)inl.InlineImageUrl);
                    return inl;
                }).ToList();
            var avatar = await _characterDataService.GetCharacterAvatarAsync(id);

            var editorData = new
            {
                Character = character,
                SelectedGenreIds = genres.Select(g => g.GenreId),
                Images = images,
                Inlines = inlines,
                AvatarUrl = _urlProcessingService.GetCharacterImageUrl((ImageUploadPath)avatar?.AvatarImageUrl) ?? "/images/Defaults/NewAvatar.png",
                CardUrl = _urlProcessingService.GetCharacterImageUrl((ImageUploadPath)character.CardImageUrl) ?? "/images/Defaults/NewCharacter.png"
            };

            var serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            return new JsonResult(editorData, serializerOptions);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCharacter([FromForm] CharacterInputModel input, IFormFile? avatarImage, IFormFile? cardImage)
        {
            var userId = _userService.GetUserId(User);
            if (userId == 0) return Unauthorized();

            var newCharacterId = await _characterDataService.CreateNewCharacterAsync(userId);
            if (newCharacterId == 0)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to create character record." });
            }

            input.CharacterId = newCharacterId;

            if (cardImage is not null)
            {
                var (path, _, _) = await _imageService.UploadImageAsync(cardImage, userId, newCharacterId, "card");
                if (path is not null)
                {
                    input.CardImageUrl = path.ToString();
                }
            }

            await _characterDataService.UpdateCharacterAsync(input);

            if (avatarImage is not null)
            {
                var (path, _, _) = await _imageService.UploadImageAsync(avatarImage, userId, newCharacterId, "avatar");
                if (path is not null)
                {
                    await _characterDataService.UpsertCharacterAvatarAsync(newCharacterId, path.ToString());
                }
            }

            await _characterDataService.UpdateCharacterGenresAsync(newCharacterId, input.SelectedGenreIds);

            return Ok(new { characterId = newCharacterId });
        }

        [HttpPost("{id:int}/details")]
        public async Task<IActionResult> UpdateCharacterDetails(int id, [FromForm] CharacterInputModel input, IFormFile? avatarImage, IFormFile? cardImage)
        {
            var userId = _userService.GetUserId(User);
            if (userId == 0) return Unauthorized();

            var character = await _characterDataService.GetCharacterForEditAsync(id, userId);
            if (character == null) return Forbid();

            input.CharacterId = id;

            if (avatarImage is not null)
            {
                var oldAvatar = await _characterDataService.GetCharacterAvatarAsync(id);
                if (oldAvatar?.AvatarImageUrl is not null)
                    await _imageService.DeleteImageAsync((ImageUploadPath)oldAvatar.AvatarImageUrl);

                var (path, _, _) = await _imageService.UploadImageAsync(avatarImage, userId, id, "avatar");
                if (path is not null)
                {
                    await _characterDataService.UpsertCharacterAvatarAsync(id, path.ToString());
                }
            }

            if (cardImage is not null)
            {
                if (character.CardImageUrl is not null)
                    await _imageService.DeleteImageAsync((ImageUploadPath)character.CardImageUrl);
                var (path, _, _) = await _imageService.UploadImageAsync(cardImage, userId, id, "card");
                if (path is not null)
                {
                    input.CardImageUrl = path.ToString();
                }
            }
            else
            {
                input.CardImageUrl = character.CardImageUrl;
            }

            await _characterDataService.UpdateCharacterAsync(input);
            await _characterDataService.UpdateCharacterGenresAsync(id, input.SelectedGenreIds);

            var newAvatar = await _characterDataService.GetCharacterAvatarAsync(id);
            var updatedCharacter = await _characterDataService.GetCharacterAsync(id);

            return Ok(new
            {
                message = "Character details saved successfully!",
                avatarUrl = _urlProcessingService.GetCharacterImageUrl((ImageUploadPath)newAvatar?.AvatarImageUrl),
                cardUrl = _urlProcessingService.GetCharacterImageUrl((ImageUploadPath)updatedCharacter?.CardImageUrl)
            });
        }

        // ADDED METHOD
        [HttpPut("{id:int}/bbframe")]
        public async Task<IActionResult> UpdateBBFrame(int id, [FromBody] UpdateBBFrameInput input)
        {
            var userId = _userService.GetUserId(User);
            if (userId == 0) return Unauthorized();

            var character = await _characterDataService.GetCharacterForEditAsync(id, userId);
            if (character is null) return Forbid();

            await _characterDataService.UpdateCharacterBBFrameAsync(id, input.BBFrameContent ?? string.Empty);

            return Ok(new { message = "BBFrame saved successfully!" });
        }

        [HttpGet("editor-lookups")]
        public async Task<IActionResult> GetEditorLookups()
        {
            var lookups = new
            {
                Genders = await _miscDataService.GetGendersAsync(),
                SexualOrientations = await _miscDataService.GetSexualOrientationsAsync(),
                Sources = await _miscDataService.GetCharacterSourcesAsync(),
                PostLengths = await _miscDataService.GetPostLengthsAsync(),
                LiteracyLevels = await _miscDataService.GetLiteracyLevelsAsync(),
                LfrpStatuses = await _miscDataService.GetLfrpStatusesAsync(),
                EroticaPreferences = await _miscDataService.GetEroticaPreferencesAsync(),
                Genres = await _miscDataService.GetGenresAsync()
            };
            return Ok(lookups);
        }

        [HttpPost("{id:int}/gallery/upload")]
        [RequestFormLimits(MultipartBodyLengthLimit = 104857600)]
        public async Task<IActionResult> UploadGalleryImages(int id, [FromForm] List<IFormFile> uploadedImages)
        {
            var userId = _userService.GetUserId(User);
            if (userId == 0) return Unauthorized();

            var character = await _characterDataService.GetCharacterForEditAsync(id, userId);
            if (character == null) return Forbid();

            var newImages = new List<CharacterImage>();
            foreach (var file in uploadedImages)
            {
                var (path, width, height) = await _imageService.UploadImageAsync(file, userId, id, "gallery");
                if (path is not null)
                {
                    var newImageId = await _characterDataService.AddImageAsync(path.ToString(), id, userId, false, "New gallery image", width, height);
                    if (newImageId > 0)
                    {
                        newImages.Add(new CharacterImage
                        {
                            CharacterImageId = newImageId,
                            CharacterImageUrl = _urlProcessingService.GetCharacterImageUrl(path),
                            ImageCaption = "New gallery image",
                            IsMature = false,
                            UserId = userId,
                            CharacterId = id,
                            Width = width,
                            Height = height,
                            ImageScale = 0
                        });
                    }
                }
            }

            return Ok(newImages);
        }

        [HttpPost("{id:int}/inlines/upload")]
        [RequestFormLimits(MultipartBodyLengthLimit = 5242880)]
        public async Task<IActionResult> UploadInlineImage(int id, [FromForm] string name, [FromForm] IFormFile file)
        {
            var userId = _userService.GetUserId(User);
            if (userId == 0) return Unauthorized();

            var character = await _characterDataService.GetCharacterForEditAsync(id, userId);
            if (character == null) return Forbid();

            if (file is null || file.Length == 0 || string.IsNullOrWhiteSpace(name))
            {
                return BadRequest(new { message = "An image file and a name are required." });
            }

            var (path, _, _) = await _imageService.UploadImageAsync(file, userId, id, "inline");
            if (path is null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to upload image." });
            }

            await _characterDataService.AddInlineImageAsync(path.ToString(), id, userId, name);

            return Ok(new { location = _urlProcessingService.GetCharacterImageUrl(path) });
        }

        [HttpDelete("{id:int}/inlines/{inlineId:int}")]
        public async Task<IActionResult> DeleteInlineImage(int id, int inlineId)
        {
            var userId = _userService.GetUserId(User);
            if (userId == 0) return Unauthorized();

            var character = await _characterDataService.GetCharacterForEditAsync(id, userId);
            if (character == null) return Forbid();

            var inline = await _characterDataService.GetInlineImageAsync(inlineId);
            if (inline == null) return NotFound();

            if (inline.UserId != userId) return Forbid();

            await _imageService.DeleteImageAsync((ImageUploadPath)inline.InlineImageUrl);
            await _characterDataService.DeleteInlineImageRecordAsync(inlineId);

            return Ok(new { message = "Inline image deleted successfully." });
        }

        [HttpPut("{id:int}/gallery/update")]
        public async Task<IActionResult> UpdateGalleryImages(int id, [FromBody] ImageUpdateInputModel galleryInput)
        {
            var userId = _userService.GetUserId(User);
            if (userId == 0) return Unauthorized();

            var character = await _characterDataService.GetCharacterForEditAsync(id, userId);
            if (character is null) return Forbid();

            if (galleryInput.ImagesToDelete is not null)
            {
                foreach (var imageId in galleryInput.ImagesToDelete)
                {
                    var image = await _characterDataService.GetImageAsync(imageId);
                    if (image is not null && image.UserId == userId)
                    {
                        await _imageService.DeleteImageAsync((ImageUploadPath)image.CharacterImageUrl);
                        await _characterDataService.DeleteImageRecordAsync(imageId);
                    }
                }
            }

            if (galleryInput.Images is not null)
            {
                foreach (var imageUpdate in galleryInput.Images)
                {
                    var image = await _characterDataService.GetImageAsync(imageUpdate.ImageId);
                    if (image is not null && image.UserId == userId)
                    {
                        await _characterDataService.UpdateImageDetailsAsync(imageUpdate.ImageId, imageUpdate.ImageCaption ?? "", imageUpdate.ImageScale);
                    }
                }
            }

            return Ok(new { message = "Gallery updated successfully." });
        }
        [HttpPost("UpdateImagePositions")]
        public async Task<IActionResult> UpdateImagePositions([FromBody] ImagePositionUpdateModel input)
        {
            var userId = _userService.GetUserId(User);
            if (userId == 0) return Unauthorized();

            var character = await _characterDataService.GetCharacterForEditAsync(input.CharacterId, userId);
            if (character is null) return Forbid();

            await _characterDataService.UpdateImagePositionsAsync(input.ImageIds);

            return Ok(new { message = "Image positions updated successfully." });
        }

        [HttpPost("DeleteImages")]
        public async Task<IActionResult> DeleteImages([FromBody] List<int> imageIds)
        {
            var userId = _userService.GetUserId(User);
            if (userId == 0) return Unauthorized();

            if (imageIds == null || !imageIds.Any())
            {
                return BadRequest(new { message = "No image IDs provided for deletion." });
            }

            await _characterDataService.DeleteImagesAsync(imageIds, userId);

            return Ok(new { message = "Images deleted successfully." });
        }

        [HttpPut("{id:int}/profile")]
        public async Task<IActionResult> UpdateCustomProfile(int id, [FromBody] UpdateProfileInput input)
        {
            var userId = _userService.GetUserId(User);
            if (userId == 0) return Unauthorized();

            var character = await _characterDataService.GetCharacterForEditAsync(id, userId);
            if (character is null) return Forbid();

            var sanitizedHtml = _htmlSanitizer.Sanitize(input.ProfileHTML);
            var sanitizedCss = _htmlSanitizer.Sanitize(input.ProfileCSS);

            await _characterDataService.UpdateCharacterCustomProfileAsync(id, sanitizedCss, sanitizedHtml, input.IsEnabled);
            return Ok(new { message = "Custom profile saved successfully!" });
        }
    }
}
