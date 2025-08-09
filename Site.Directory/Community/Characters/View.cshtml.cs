using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using RoleplayersGuild.Site.Services.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RoleplayersGuild.Site.Services.DataServices;
using System;
using System.Text.Json;

namespace RoleplayersGuild.Site.Directory.Community.Characters
{
    public class ViewCharacterModel : PageModel
    {
        private readonly ICharacterDataService _characterDataService;
        private readonly IUserDataService _userDataService;
        private readonly IUserService _userService;
        private readonly IBBCodeService _bbcodeService;
        private readonly IUrlProcessingService _urlProcessingService;
        public readonly IViteManifestService ViteAssets;

        public ViewCharacterModel(ICharacterDataService characterDataService, IUserDataService userDataService, IUserService userService, IBBCodeService bbcodeService, IUrlProcessingService urlProcessingService, IViteManifestService viteAssets)
        {
            _characterDataService = characterDataService;
            _userDataService = userDataService;
            _userService = userService;
            _bbcodeService = bbcodeService;
            _urlProcessingService = urlProcessingService;
            ViteAssets = viteAssets;
        }

        public CharacterWithDetails Character { get; set; } = new();
        public List<string> Genres { get; set; } = new();
        public IEnumerable<CharacterImage> Images { get; private set; } = Enumerable.Empty<CharacterImage>();
        public string ImagesJson { get; private set; } = "[]";
        public string CharacterJson { get; private set; } = "{}";
        public string GenresJson { get; private set; } = "[]";
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
            await _characterDataService.IncrementCharacterViewCountAsync(id);

            var characterDetails = await _characterDataService.GetCharacterWithDetailsAsync(id);
            if (characterDetails == null)
            {
                return NotFound();
            }

            var rawCharacter = await _characterDataService.GetCharacterAsync(id);
            if (rawCharacter != null)
            {
                characterDetails.CardImageUrl = _urlProcessingService.GetCharacterImageUrl((ImageUploadPath)rawCharacter.CardImageUrl);
            }
            Character = characterDetails;

            var rawImages = await _characterDataService.GetCharacterImagesForGalleryAsync(id);
            Images = rawImages.Select(img => 
            {
                img.CharacterImageUrl = _urlProcessingService.GetCharacterImageUrl((ImageUploadPath)img.CharacterImageUrl);
                return img;
            }).ToList();

            var serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            ImagesJson = JsonSerializer.Serialize(Images, serializerOptions);
            CharacterJson = JsonSerializer.Serialize(Character, serializerOptions);
            GenresJson = JsonSerializer.Serialize(Genres, serializerOptions);

            var currentUserId = _userService.GetUserId(User);
            IsLoggedIn = currentUserId != 0;
            IsOwner = currentUserId == Character.UserId;

            if (IsLoggedIn && !IsOwner)
            {
                var blockRecordId = await _userDataService.GetBlockRecordIdAsync(Character.UserId, currentUserId);
                IsBlocked = blockRecordId > 0;
            }

            IsOnline = Character.ShowWhenOnline && Character.LastAction.HasValue && Character.LastAction > DateTime.UtcNow.AddMinutes(-15);

            UserCanViewMatureContent = await _userService.GetUserPrefersMatureAsync(User);

            var genresData = await _characterDataService.GetCharacterGenresAsync(id);
            Genres = genresData.Select(g => g.GenreName).ToList();

            await PrepareContent();

            return Page();
        }

        public async Task<IActionResult> OnPostBlockAsync(int id)
        {
            var currentUserId = _userService.GetUserId(User);
            var characterOwnerId = await _userDataService.GetUserIdFromCharacterAsync(id);

            if (currentUserId == 0 || currentUserId == characterOwnerId) return Forbid();

            await _userDataService.BlockUserAsync(currentUserId, characterOwnerId);
            MessageType = "success";
            Message = "User has been blocked.";
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostUnblockAsync(int id)
        {
            var currentUserId = _userService.GetUserId(User);
            var characterOwnerId = await _userDataService.GetUserIdFromCharacterAsync(id);
            if (currentUserId == 0) return Forbid();

            await _userDataService.UnblockUserAsync(currentUserId, characterOwnerId);
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
