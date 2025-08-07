using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System;

namespace RoleplayersGuild.Site.Directory.Community.Characters
{
    public class ViewCharacterModel : PageModel
    {
        private readonly IDataService _dataService;
        private readonly IUserService _userService;
        private readonly IBBCodeService _bbcodeService;
        private readonly IImageService _imageService;

        public ViewCharacterModel(IDataService dataService, IUserService userService, IBBCodeService bbcodeService, IImageService imageService)
        {
            _dataService = dataService;
            _userService = userService;
            _bbcodeService = bbcodeService;
            _imageService = imageService;
        }

        public CharacterWithDetails Character { get; set; } = new();
        public List<string> Genres { get; set; } = new();
        public IEnumerable<CharacterImage> Images { get; private set; } = Enumerable.Empty<CharacterImage>();
        public string MetaDescription { get; set; } = "A character profile on the Role-Players Guild.";
        public string CharacterBBFrameHtml { get; set; } = "";
        public bool IsLoggedIn { get; private set; }
        public bool IsOwner { get; private set; }
        public bool IsBlocked { get; private set; }
        public bool IsOnline { get; private set; }
        public bool UserCanViewMatureContent { get; private set; }

        [TempData]
        public string? Message { get; set; }
        [TempData]
        public string MessageType { get; set; } = "info";

        public async Task<IActionResult> OnGetAsync(int id)
        {
            await _dataService.ExecuteAsync("""UPDATE "Characters" SET "ViewCount" = "ViewCount" + 1 WHERE "CharacterId" = @id""", new { id });

            var characterDetails = await _dataService.GetCharacterWithDetailsAsync(id);
            if (characterDetails == null)
            {
                return NotFound();
            }

            var rawCharacter = await _dataService.GetCharacterAsync(id);
            if (rawCharacter != null)
            {
                characterDetails.DisplayImageUrl = _imageService.GetImageUrl(rawCharacter.CardImageUrl);
            }
            Character = characterDetails;

            // --- FIX IS HERE ---
            // 1. Fetch the raw image data from the service.
            var rawImages = await _dataService.GetCharacterImagesForGalleryAsync(id);

            // 2. Process each image to create the full URL before assigning it to the public property.
            Images = rawImages.Select(img =>
            {
                img.CharacterImageUrl = _imageService.GetImageUrl(img.CharacterImageUrl);
                return img;
            }).ToList();
            // --- END FIX ---

            var currentUserId = _userService.GetUserId(User);
            IsLoggedIn = currentUserId != 0;
            IsOwner = currentUserId == Character.UserId;

            if (IsLoggedIn && !IsOwner)
            {
                IsBlocked = await _dataService.IsUserBlockedAsync(Character.UserId, currentUserId);
            }

            IsOnline = Character.ShowWhenOnline && Character.LastAction.HasValue && Character.LastAction > DateTime.UtcNow.AddMinutes(-15);

            var currentUser = await _userService.GetCurrentUserAsync();
            UserCanViewMatureContent = currentUser?.ShowMatureContent ?? true;

            var genresData = await _dataService.GetCharacterGenresAsync(id);
            Genres = genresData.Select(g => g.GenreName).ToList();

            await PrepareContent();

            return Page();
        }

        public async Task<IActionResult> OnPostBlockAsync(int id)
        {
            var currentUserId = _userService.GetUserId(User);
            var characterOwnerId = await _dataService.GetScalarAsync<int>("""SELECT "UserId" FROM "Characters" WHERE "CharacterId" = @id""", new { id });

            if (currentUserId == 0 || currentUserId == characterOwnerId) return Forbid();

            await _dataService.BlockUserAsync(currentUserId, characterOwnerId);
            MessageType = "success";
            Message = "User has been blocked.";
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostUnblockAsync(int id)
        {
            var currentUserId = _userService.GetUserId(User);
            var characterOwnerId = await _dataService.GetScalarAsync<int>("""SELECT "UserId" FROM "Characters" WHERE "CharacterId" = @id""", new { id });
            if (currentUserId == 0) return Forbid();

            await _dataService.UnblockUserAsync(currentUserId, characterOwnerId);
            MessageType = "success";
            Message = "User has been unblocked.";
            return RedirectToPage(new { id });
        }

        private async Task PrepareContent()
        {
            if (!string.IsNullOrEmpty(Character.CharacterBBFrame))
            {
                CharacterBBFrameHtml = await _bbcodeService.ParseAsync(Character.CharacterBBFrame, Character.CharacterId);

                var plainTextBBFrame = Regex.Replace(Character.CharacterBBFrame, @"\[.*?\]", string.Empty);
                MetaDescription = plainTextBBFrame.Length > 160
                    ? plainTextBBFrame.Substring(0, 160).Trim() + "..."
                    : plainTextBBFrame.Trim();
            }
        }
    }
}