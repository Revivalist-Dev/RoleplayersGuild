using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RoleplayersGuild.Site.Model
{
    // ----------------------------------------------------------------
    // Character Creation, Editing & Management Input Models
    // ----------------------------------------------------------------

    public class CharacterInputModel
    {
        public int CharacterId { get; set; }

        [Required(ErrorMessage = "A Display Name is required.")]
        [StringLength(50)]
        public string? CharacterDisplayName { get; set; }

        [Required(ErrorMessage = "A First Name is required.")]
        [StringLength(50)]
        public string? CharacterFirstName { get; set; }

        [StringLength(50)]
        public string? CharacterMiddleName { get; set; }

        [StringLength(50)]
        public string? CharacterLastName { get; set; }

        public string? CharacterBio { get; set; }

        [Required(ErrorMessage = "Please select a gender.")]
        public int? CharacterGender { get; set; }

        public int? SexualOrientation { get; set; }
        public int? CharacterSourceId { get; set; }
        public int? PostLengthMin { get; set; }
        public int? PostLengthMax { get; set; }
        public int? LiteracyLevel { get; set; }

        [Required(ErrorMessage = "Please select a Contact Preference.")]
        public int? LfrpStatus { get; set; }

        public int? EroticaPreferences { get; set; }
        public bool MatureContent { get; set; }
        public bool IsPrivate { get; set; }
        public bool DisableLinkify { get; set; }
        public string? CardImageUrl { get; set; }
        public List<int> SelectedGenreIds { get; set; } = new();
        public int AssignedUserBadgeId { get; set; }

        public CharacterInputModel() { }
        public CharacterInputModel(Character c)
        {
            CharacterId = c.CharacterId;
            CharacterDisplayName = c.CharacterDisplayName;
            CharacterFirstName = c.CharacterFirstName;
            CharacterMiddleName = c.CharacterMiddleName;
            CharacterLastName = c.CharacterLastName;
            CharacterBio = c.CharacterBio;
            CharacterGender = c.CharacterGender;
            SexualOrientation = c.SexualOrientation;
            CharacterSourceId = c.CharacterSourceId;
            PostLengthMin = c.PostLengthMin;
            PostLengthMax = c.PostLengthMax;
            LiteracyLevel = c.LiteracyLevel;
            LfrpStatus = c.LfrpStatus;
            EroticaPreferences = c.EroticaPreferences;
            MatureContent = c.MatureContent;
            IsPrivate = c.IsPrivate;
            DisableLinkify = c.DisableLinkify;
            CardImageUrl = c.CardImageUrl;
        }
    }

    public class ProfileInputModel
    {
        [Required]
        public int CharacterId { get; set; }
        public string? ProfileCSS { get; set; }
        public string? ProfileHTML { get; set; }
        public bool IsEnabled { get; set; }
    }

    public class ImageUpdateInputModel
    {
        public List<ImageUpdateItem> Images { get; set; } = new();
        public List<int>? ImagesToDelete { get; set; }
    }

    public class ImageUpdateItem
    {
        public int ImageId { get; set; }
        public string? ImageCaption { get; set; }
        public bool IsPrimary { get; set; }
    }

    public class SearchInputModel
    {
        public string? SearchTerm { get; set; }
        public int? GenderId { get; set; }
        public int SortOrder { get; set; }
        public List<int> SelectedGenreIds { get; set; } = new();
        public string? Name { get; set; }
    }
}