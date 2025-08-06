using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class UpdateProfileInput
{
    public string? ProfileCSS { get; set; }
    public string? ProfileHTML { get; set; }
    public bool IsEnabled { get; set; }
}

[ApiController]
[Route("api/characters")]
public class CharactersApiController : ControllerBase
{
    private readonly IDataService _dataService;
    private readonly IUserService _userService;
    private readonly IImageService _imageService;
    private readonly IHtmlSanitizationService _htmlSanitizer;

    public CharactersApiController(
        IDataService dataService,
        IUserService userService,
        IImageService imageService,
        IHtmlSanitizationService htmlSanitizer)
    {
        _dataService = dataService;
        _userService = userService;
        _imageService = imageService;
        _htmlSanitizer = htmlSanitizer;
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetCharacterForEdit(int id)
    {
        var userId = _userService.GetUserId(User);
        if (userId == 0) return Unauthorized();

        var character = await _dataService.GetCharacterForEditAsync(id, userId);
        if (character == null) return Forbid();

        var genres = await _dataService.GetCharacterGenresAsync(id);
        var images = (await _dataService.GetCharacterImagesForGalleryAsync(id)).ToList();
        var inlines = (await _dataService.GetRecordsAsync<CharacterInline>("""SELECT * FROM "CharacterInlines" WHERE "CharacterId" = @id ORDER BY "InlineName" """, new { id })).ToList();
        var avatar = await _dataService.GetRecordAsync<CharacterAvatar>("""SELECT * FROM "CharacterAvatars" WHERE "CharacterId" = @id""", new { id });

        foreach (var img in images)
        {
            img.CharacterImageUrl = _imageService.GetImageUrl(img.CharacterImageUrl);
        }

        foreach (var inline in inlines)
        {
            inline.InlineImageUrl = _imageService.GetImageUrl(inline.InlineImageUrl);
        }

        var editorData = new
        {
            Character = character,
            SelectedGenreIds = genres.Select(g => g.GenreId),
            Images = images,
            Inlines = inlines,
            AvatarUrl = _imageService.GetImageUrl(avatar?.AvatarImageUrl ?? "/images/UserFiles/CharacterAvatars/NewAvatar.png"),
            CardUrl = _imageService.GetImageUrl(character.CardImageUrl ?? "/images/UserFiles/CharacterCards/NewCharacter.png")
        };

        return Ok(editorData);
    }

    [HttpPost("{id:int}/details")]
    public async Task<IActionResult> UpdateCharacterDetails(int id, [FromForm] CharacterInputModel input, IFormFile? avatarImage, IFormFile? cardImage)
    {
        var userId = _userService.GetUserId(User);
        if (userId == 0) return Unauthorized();

        var character = await _dataService.GetCharacterForEditAsync(id, userId);
        if (character == null) return Forbid();

        input.CharacterId = id;

        if (avatarImage is not null)
        {
            var oldAvatar = await _dataService.GetRecordAsync<CharacterAvatar>("""SELECT * FROM "CharacterAvatars" WHERE "CharacterId" = @id""", new { id });
            await _imageService.DeleteImageAsync(oldAvatar?.AvatarImageUrl);

            var storedPath = await _imageService.UploadImageAsync(avatarImage, userId, id, "avatar");
            if (!string.IsNullOrEmpty(storedPath))
            {
                await _dataService.UpsertCharacterAvatarAsync(id, storedPath);
            }
        }

        if (cardImage is not null)
        {
            await _imageService.DeleteImageAsync(character.CardImageUrl);
            input.CardImageUrl = await _imageService.UploadImageAsync(cardImage, userId, id, "card");
        }
        else
        {
            input.CardImageUrl = character.CardImageUrl;
        }

        await _dataService.UpdateCharacterAsync(input);
        await _dataService.UpdateCharacterGenresAsync(id, input.SelectedGenreIds);

        var newAvatar = await _dataService.GetRecordAsync<CharacterAvatar>("""SELECT * FROM "CharacterAvatars" WHERE "CharacterId" = @id""", new { id });
        var updatedCharacter = await _dataService.GetCharacterAsync(id);

        return Ok(new
        {
            message = "Character details saved successfully!",
            avatarUrl = _imageService.GetImageUrl(newAvatar?.AvatarImageUrl),
            cardUrl = _imageService.GetImageUrl(updatedCharacter?.CardImageUrl)
        });
    }

    [HttpGet("editor-lookups")]
    public async Task<IActionResult> GetEditorLookups()
    {
        var lookups = new
        {
            Genders = await _dataService.GetGendersAsync(),
            SexualOrientations = await _dataService.GetSexualOrientationsAsync(),
            Sources = await _dataService.GetCharacterSourcesAsync(),
            PostLengths = await _dataService.GetPostLengthsAsync(),
            LiteracyLevels = await _dataService.GetLiteracyLevelsAsync(),
            LfrpStatuses = await _dataService.GetLfrpStatusesAsync(),
            EroticaPreferences = await _dataService.GetEroticaPreferencesAsync(),
            Genres = await _dataService.GetGenresAsync()
        };
        return Ok(lookups);
    }

    [HttpPost("{id:int}/gallery/upload")]
    [RequestFormLimits(MultipartBodyLengthLimit = 104857600)]
    public async Task<IActionResult> UploadGalleryImages(int id, [FromForm] List<IFormFile> uploadedImages)
    {
        var userId = _userService.GetUserId(User);
        if (userId == 0) return Unauthorized();

        var character = await _dataService.GetCharacterForEditAsync(id, userId);
        if (character == null) return Forbid();

        var uploadedFileNames = new List<string>();
        foreach (var file in uploadedImages)
        {
            var storedPath = await _imageService.UploadImageAsync(file, userId, id, "gallery");
            if (storedPath != null)
            {
                await _dataService.AddImageAsync(storedPath, id, userId, false, false, "New gallery image");
                uploadedFileNames.Add(storedPath);
            }
        }

        return Ok(new { message = $"{uploadedFileNames.Count} image(s) uploaded successfully." });
    }

    [HttpPost("{id:int}/inlines/upload")]
    [RequestFormLimits(MultipartBodyLengthLimit = 5242880)]
    public async Task<IActionResult> UploadInlineImage(int id, [FromForm] string name, [FromForm] IFormFile file)
    {
        var userId = _userService.GetUserId(User);
        if (userId == 0) return Unauthorized();

        var character = await _dataService.GetCharacterForEditAsync(id, userId);
        if (character == null) return Forbid();

        if (file is null || file.Length == 0 || string.IsNullOrWhiteSpace(name))
        {
            return BadRequest(new { message = "An image file and a name are required." });
        }

        var storedPath = await _imageService.UploadImageAsync(file, userId, id, "inline");
        if (string.IsNullOrEmpty(storedPath))
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to upload image." });
        }

        await _dataService.AddInlineImageAsync(storedPath, id, userId, name);

        return Ok(new { location = _imageService.GetImageUrl(storedPath) });
    }

    [HttpDelete("{id:int}/inlines/{inlineId:int}")]
    public async Task<IActionResult> DeleteInlineImage(int id, int inlineId)
    {
        var userId = _userService.GetUserId(User);
        if (userId == 0) return Unauthorized();

        var character = await _dataService.GetCharacterForEditAsync(id, userId);
        if (character == null) return Forbid();

        var inline = await _dataService.GetInlineImageAsync(inlineId);
        if (inline == null) return NotFound();

        if (inline.UserId != userId) return Forbid();

        await _imageService.DeleteImageAsync(inline.InlineImageUrl);
        await _dataService.DeleteInlineImageRecordAsync(inlineId);

        return Ok(new { message = "Inline image deleted successfully." });
    }

    [HttpPut("{id:int}/gallery/update")]
    public async Task<IActionResult> UpdateGalleryImages(int id, [FromBody] ImageUpdateInputModel galleryInput)
    {
        var userId = _userService.GetUserId(User);
        if (userId == 0) return Unauthorized();

        var character = await _dataService.GetCharacterForEditAsync(id, userId);
        if (character is null) return Forbid();

        if (galleryInput.ImagesToDelete is not null)
        {
            foreach (var imageId in galleryInput.ImagesToDelete)
            {
                var image = await _dataService.GetImageAsync(imageId);
                if (image is not null && image.UserId == userId)
                {
                    await _imageService.DeleteImageAsync(image.CharacterImageUrl);
                    await _dataService.DeleteImageRecordAsync(imageId);
                }
            }
        }

        if (galleryInput.Images is not null)
        {
            foreach (var imageUpdate in galleryInput.Images)
            {
                var image = await _dataService.GetImageAsync(imageUpdate.ImageId);
                if (image is not null && image.UserId == userId)
                {
                    await _dataService.UpdateImageDetailsAsync(imageUpdate.ImageId, imageUpdate.ImageCaption ?? "", imageUpdate.IsPrimary);
                }
            }
        }

        return Ok(new { message = "Gallery updated successfully." });
    }

    [HttpPut("{id:int}/profile")]
    public async Task<IActionResult> UpdateCustomProfile(int id, [FromBody] UpdateProfileInput input)
    {
        var userId = _userService.GetUserId(User);
        if (userId == 0) return Unauthorized();

        var character = await _dataService.GetCharacterForEditAsync(id, userId);
        if (character is null) return Forbid();

        var sanitizedHtml = _htmlSanitizer.Sanitize(input.ProfileHTML);
        var sanitizedCss = _htmlSanitizer.Sanitize(input.ProfileCSS);

        await _dataService.UpdateCharacterCustomProfileAsync(id, sanitizedCss, sanitizedHtml, input.IsEnabled);
        return Ok(new { message = "Custom profile saved successfully!" });
    }
}