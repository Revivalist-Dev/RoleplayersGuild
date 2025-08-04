// F:\Visual Studio\RoleplayersGuild\Site.Directory\User-Panel\My-Characters\Edit.cshtml.cs

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.User_Panel.My_Characters
{
    public class EditMyCharactersModel : UserPanelBaseModel
    {
        private readonly IDataService _dataService;
        private readonly IUserService _userService;
        private readonly IImageService _imageService;
        private readonly IBBCodeService _bbcodeService;

        [BindProperty] public CharacterInputModel Input { get; set; } = new();
        [BindProperty] public ImageUpdateInputModel GalleryInput { get; set; } = new();
        [BindProperty] public ImageUploadInputModel UploadInput { get; set; } = new();
        [BindProperty] public InlineUploadInputModel InlineUpload { get; set; } = new();

        public List<CharacterImage> ExistingImages { get; set; } = new();
        public List<CharacterInline> ExistingInlines { get; set; } = new();
        public string? CurrentAvatarUrl { get; set; }
        public string? CurrentCardImageUrl { get; set; }
        public int AvailableSlots => MaxSlots - UsedSlots;
        public int MaxFileSizeMb { get; private set; }
        public SelectList Genders { get; set; } = new(Enumerable.Empty<SelectListItem>());
        public SelectList SexualOrientations { get; set; } = new(Enumerable.Empty<SelectListItem>());
        public SelectList Sources { get; set; } = new(Enumerable.Empty<SelectListItem>());
        public SelectList PostLengths { get; set; } = new(Enumerable.Empty<SelectListItem>());
        public SelectList LiteracyLevels { get; set; } = new(Enumerable.Empty<SelectListItem>());
        public SelectList LfrpStatuses { get; set; } = new(Enumerable.Empty<SelectListItem>());
        public SelectList EroticaPreferences { get; set; } = new(Enumerable.Empty<SelectListItem>());
        public List<GenreSelectionViewModel> Genres { get; set; } = new();
        public bool IsNewCharacter => Input.CharacterId == 0;
        public bool ShowWelcomeMessage { get; private set; }
        public int UsedSlots { get; private set; }
        public int MaxSlots { get; private set; }

        public EditMyCharactersModel(IDataService dataService, IUserService userService, IImageService imageService, IBBCodeService bbcodeService)
            : base(dataService, userService)
        {
            _dataService = dataService;
            _userService = userService;
            _imageService = imageService;
            _bbcodeService = bbcodeService;
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            var userId = _userService.GetUserId(User);
            if (userId == 0) return RedirectToPage("/");

            if (id.HasValue)
            {
                var character = await _dataService.GetCharacterForEditAsync(id.Value, userId);
                if (character is null) return Forbid();
                Input = new CharacterInputModel { CharacterId = character.CharacterId };
            }
            else
            {
                Input = new CharacterInputModel();
                ShowWelcomeMessage = await _dataService.GetCharacterCountAsync(userId) == 0;
            }
            return Page();
        }

        #region --- Partial Page Handlers ---
        public async Task<IActionResult> OnGetDetailsAsync(int? id)
        {
            if (id.HasValue) Input.CharacterId = id.Value;
            await LoadCommonData(id);
            return Partial("_EditDetails", this);
        }

        public async Task<IActionResult> OnGetBBFrameAsync(int id)
        {
            Input.CharacterId = id;
            await LoadCommonData(id);
            return Partial("_EditBBFrame", this);
        }

        public async Task<IActionResult> OnGetGalleryAsync(int id)
        {
            Input.CharacterId = id;
            await LoadCommonData(id);
            return Partial("_EditGallery", this);
        }
        #endregion

        #region --- Form Post Handlers (Updated for AJAX) ---
        public async Task<IActionResult> OnPostMainAsync()
        {
            var userId = _userService.GetUserId(User);
            if (userId == 0) return Forbid();

            ModelState.Remove(nameof(GalleryInput));
            ModelState.Remove(nameof(UploadInput));
            ModelState.Remove(nameof(InlineUpload));

            if (!ModelState.IsValid)
            {
                await LoadCommonData(Input.CharacterId);
                return Partial("_EditDetails", this);
            }

            if (IsNewCharacter)
            {
                var newCharacterId = await _dataService.CreateNewCharacterAsync(userId);
                Input.CharacterId = newCharacterId;
                await _dataService.AwardBadgeIfNotExistingAsync(27, userId);
            }

            if (Input.AvatarImage is not null)
            {
                var fileName = await _imageService.UploadImageAsync(Input.AvatarImage, "avatars");
                if (fileName is not null)
                {
                    await _dataService.ExecuteAsync(
                        """
                        INSERT INTO "CharacterAvatars" ("CharacterId", "AvatarImageUrl") VALUES (@CharacterId, @AvatarImageUrl)
                        ON CONFLICT ("CharacterId") DO UPDATE SET "AvatarImageUrl" = EXCLUDED."AvatarImageUrl";
                        """, new { Input.CharacterId, AvatarImageUrl = fileName });
                }
            }

            if (Input.CardImage is not null)
            {
                var fileName = await _imageService.UploadImageAsync(Input.CardImage, "cards");
                Input.CardImageUrl = fileName;
            }

            await _dataService.UpdateCharacterAsync(Input);
            await _dataService.UpdateCharacterGenresAsync(Input.CharacterId, Input.SelectedGenreIds);

            return new JsonResult(new { success = true, message = "Character details saved successfully!" });
        }

        public async Task<IActionResult> OnPostUploadInlineAsync()
        {
            var userId = _userService.GetUserId(User);
            if (userId == 0) return Forbid();

            var character = await _dataService.GetCharacterForEditAsync(Input.CharacterId, userId);
            if (character is null) return Forbid();

            ModelState.Remove(nameof(Input));
            ModelState.Remove(nameof(GalleryInput));
            ModelState.Remove(nameof(UploadInput));

            if (InlineUpload.Image == null || string.IsNullOrWhiteSpace(InlineUpload.Name))
            {
                ModelState.AddModelError("InlineUpload.Name", "Both a name and an image file are required.");
            }

            if (!ModelState.IsValid)
            {
                await LoadCommonData(Input.CharacterId);
                return Partial("_EditBBFrame", this);
            }

            var fileName = await _imageService.UploadImageAsync(InlineUpload.Image, "inlines");
            if (fileName is not null)
            {
                await _dataService.ExecuteAsync(
                    """INSERT INTO "CharacterInlines" ("CharacterId", "InlineName", "InlineImageUrl") VALUES (@CharacterId, @Name, @URL)""",
                    new { Input.CharacterId, InlineUpload.Name, URL = fileName });
            }
            else
            {
                ModelState.AddModelError("InlineUpload.Image", "There was an error uploading your image.");
                await LoadCommonData(Input.CharacterId);
                return Partial("_EditBBFrame", this);
            }

            return new JsonResult(new { success = true, message = "Inline image uploaded successfully." });
        }
        [RequestFormLimits(MultipartBodyLengthLimit = 104857600)]
        public async Task<IActionResult> OnPostGalleryAsync()
        {
            var userId = _userService.GetUserId(User);
            if (userId == 0) return Forbid();

            ModelState.Remove(nameof(Input));
            ModelState.Remove(nameof(UploadInput));
            ModelState.Remove(nameof(InlineUpload));

            if (!ModelState.IsValid)
            {
                await LoadCommonData(Input.CharacterId);
                return Partial("_EditGallery", this);
            }

            if (GalleryInput.ImagesToDelete is not null)
            {
                foreach (var imageId in GalleryInput.ImagesToDelete)
                {
                    var image = await _dataService.GetImageAsync(imageId);
                    if (image is not null && image.UserId == userId)
                    {
                        await _imageService.DeleteImageAsync(image.CharacterImageUrl);
                        await _dataService.DeleteImageRecordAsync(imageId);
                    }
                }
            }
            if (GalleryInput.Images is not null)
            {
                foreach (var imageUpdate in GalleryInput.Images)
                {
                    var image = await _dataService.GetImageAsync(imageUpdate.ImageId);
                    if (image is not null && image.UserId == userId)
                    {
                        if (imageUpdate.IsPrimary)
                        {
                            await _dataService.RemoveDefaultFlagFromImagesAsync(Input.CharacterId);
                        }
                        await _dataService.UpdateImageDetailsAsync(imageUpdate.ImageId, imageUpdate.ImageCaption ?? "", imageUpdate.IsPrimary);
                    }
                }
            }
            return new JsonResult(new { success = true, message = "Gallery updated successfully." });
        }
        [RequestFormLimits(MultipartBodyLengthLimit = 104857600)]
        public async Task<IActionResult> OnPostUploadAsync()
        {
            var userId = _userService.GetUserId(User);
            if (userId == 0) return Forbid();

            ModelState.Remove(nameof(Input));
            ModelState.Remove(nameof(GalleryInput));
            ModelState.Remove(nameof(InlineUpload));

            await PopulatePageDataAsync(userId);

            if (UploadInput.UploadedImages == null || !UploadInput.UploadedImages.Any())
            {
                ModelState.AddModelError("UploadInput.UploadedImages", "Please select at least one image to upload.");
            }
            else
            {
                if (UploadInput.UploadedImages.Count > AvailableSlots)
                    ModelState.AddModelError("UploadInput.UploadedImages", $"You tried to upload {UploadInput.UploadedImages.Count} images, but you only have {AvailableSlots} slots available.");

                foreach (var imageFile in UploadInput.UploadedImages)
                {
                    if (imageFile.Length > (long)MaxFileSizeMb * 1024 * 1024)
                        ModelState.AddModelError("UploadInput.UploadedImages", $"The file '{imageFile.FileName}' exceeds your upload limit of {MaxFileSizeMb} MB.");
                }
            }

            if (!ModelState.IsValid)
            {
                await LoadCommonData(Input.CharacterId);
                return Partial("_EditGallery", this);
            }

            foreach (var imageFile in UploadInput.UploadedImages)
            {
                var fileName = await _imageService.UploadImageAsync(imageFile, "images");
                if (fileName is not null)
                    await _dataService.AddImageAsync(fileName, Input.CharacterId, false, false, "New gallery image");
            }
            return new JsonResult(new { success = true, message = "Images uploaded successfully!" });
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var userId = _userService.GetUserId(User);
            var character = await _dataService.GetCharacterForEditAsync(id, userId);
            if (character is null) return Forbid();

            await _dataService.DeleteCharacterAsync(id);
            return new JsonResult(new { success = true, message = "Character deleted successfully.", redirectUrl = Url.Page("./Index") });
        }

        public async Task<IActionResult> OnPostBBFrameAsync()
        {
            var userId = _userService.GetUserId(User);
            if (userId == 0) return Forbid();
            await _dataService.ExecuteAsync(
                """UPDATE "Characters" SET "CharacterBio" = @Bio, "LastUpdated" = NOW() AT TIME ZONE 'UTC' WHERE "CharacterId" = @Id AND "UserId" = @UserId""",
                new { Input.CharacterBio, Id = Input.CharacterId, UserId = userId });
            return new JsonResult(new { success = true, message = "BBFrame content saved successfully!" });
        }

        public async Task<IActionResult> OnPostDeleteInlineAsync(int inlineId)
        {
            var userId = _userService.GetUserId(User);
            if (userId == 0) return Forbid();
            var inline = await _dataService.GetRecordAsync<CharacterInline>("""SELECT * FROM "CharacterInlines" WHERE "InlineId" = @inlineId""", new { inlineId });
            if (inline is null) return NotFound();
            var character = await _dataService.GetCharacterForEditAsync(inline.CharacterId, userId);
            if (character is null) return Forbid();

            await _imageService.DeleteImageAsync(inline.InlineImageUrl);
            await _dataService.ExecuteAsync("""DELETE FROM "CharacterInlines" WHERE "InlineId" = @inlineId""", new { inlineId });
            return new JsonResult(new { success = true, message = "Inline image deleted successfully." });
        }

        public async Task<IActionResult> OnPostPreviewAsync([FromBody] PreviewRequest data)
        {
            if (data == null || string.IsNullOrEmpty(data.Text))
                return new JsonResult(new { html = "" });
            var html = await _bbcodeService.ParseAsync(data.Text, data.CharacterId);
            return new JsonResult(new { html });
        }
        #endregion

        #region --- Private Helper Methods ---
        private async Task LoadCommonData(int? id)
        {
            // THE FIX: Clear model state and reset lists to ensure we get fresh data
            ModelState.Clear();
            ExistingImages = new();
            ExistingInlines = new();

            var userId = _userService.GetUserId(User);
            if (id.HasValue && id > 0)
            {
                var character = await _dataService.GetCharacterForEditAsync(id.Value, userId);
                if (character != null)
                {
                    Input = new CharacterInputModel(character);
                    Input.SelectedGenreIds = (await _dataService.GetCharacterGenresAsync(id.Value)).Select(g => g.GenreId).ToList();
                    var avatar = await _dataService.GetRecordAsync<CharacterAvatar>("""SELECT * FROM "CharacterAvatars" WHERE "CharacterId" = @Id""", new { Id = id.Value });
                    CurrentAvatarUrl = _imageService.GetImageUrl(avatar?.AvatarImageUrl, "avatar");
                    CurrentCardImageUrl = _imageService.GetImageUrl(character.CardImageUrl, "card");
                    await PopulateGalleryDataAsync(userId, id.Value);
                    await PopulateInlinesAsync(id.Value);
                }
            }
            await PopulateSelectListsAsync();
        }

        private async Task PopulateSelectListsAsync()
        {
            Genders = new SelectList(await _dataService.GetGendersAsync(), "GenderId", "GenderName", Input.CharacterGender);
            SexualOrientations = new SelectList(await _dataService.GetSexualOrientationsAsync(), "SexualOrientationId", "OrientationName", Input.SexualOrientation);
            Sources = new SelectList(await _dataService.GetCharacterSourcesAsync(), "SourceId", "SourceName", Input.CharacterSourceId);
            PostLengths = new SelectList(await _dataService.GetPostLengthsAsync(), "PostLengthId", "Length");
            LiteracyLevels = new SelectList(await _dataService.GetLiteracyLevelsAsync(), "LiteracyLevelId", "LevelName", Input.LiteracyLevel);
            LfrpStatuses = new SelectList(await _dataService.GetLfrpStatusesAsync(), "LfrpStatusId", "StatusName", Input.LfrpStatus);
            EroticaPreferences = new SelectList(await _dataService.GetEroticaPreferencesAsync(), "EroticaPreferenceId", "PreferenceName", Input.EroticaPreferences);
            var allGenres = await _dataService.GetGenresAsync();
            Genres = allGenres.Select(g => new GenreSelectionViewModel
            {
                GenreId = g.GenreId,
                GenreName = g.GenreName,
                IsSelected = Input.SelectedGenreIds.Contains(g.GenreId)
            }).ToList();
        }

        private async Task PopulateInlinesAsync(int characterId)
        {
            var inlines = await _dataService.GetRecordsAsync<CharacterInline>("""SELECT * FROM "CharacterInlines" WHERE "CharacterId" = @Id ORDER BY "InlineName" """, new { Id = characterId });
            foreach (var inline in inlines)
            {
                // FIX: Use null-coalescing operator to provide a default value if GetImageUrl returns null.
                inline.InlineImageUrl = _imageService.GetImageUrl(inline.InlineImageUrl, "inline") ?? string.Empty;
            }
            ExistingInlines = inlines.ToList();
        }

        private async Task PopulateGalleryDataAsync(int userId, int characterId)
        {
            await PopulatePageDataAsync(userId);
            var images = await _dataService.GetRecordsAsync<CharacterImage>("""SELECT * FROM "CharacterImages" WHERE "CharacterId" = @characterId ORDER BY "IsPrimary" DESC""", new { characterId });
            ExistingImages = images.Select(img => {
                // FIX: Use null-coalescing operator to provide a default value if GetImageUrl returns null.
                img.CharacterImageUrl = _imageService.GetImageUrl(img.CharacterImageUrl, "gallery") ?? string.Empty;
                return img;
            }).ToList();
        }

        private async Task PopulatePageDataAsync(int userId)
        {
            var limits = await _userService.GetImageLimitsAsync();
            UsedSlots = await _dataService.GetScalarAsync<int>("""SELECT COUNT("CharacterImageId") FROM "CharacterImages" ci JOIN "Characters" c ON ci."CharacterId" = c."CharacterId" WHERE c."UserId" = @userId""", new { userId });

            // Check if 'limits' is null before using it.
            if (limits != null)
            {
                MaxSlots = limits.MaxSlots;
                MaxFileSizeMb = limits.MaxSizeMb;
            }
            else
            {
                // Assign default values if the limits can't be fetched.
                MaxSlots = 0;
                MaxFileSizeMb = 0;
            }
        }
        #endregion
    }
}