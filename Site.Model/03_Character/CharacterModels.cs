using System;

namespace RoleplayersGuild.Site.Model
{
    // ----------------------------------------------------------------
    // Character Profile & Asset Models
    // ----------------------------------------------------------------

    public class Character
    {
        public int CharacterId { get; set; }
        public int UserId { get; set; }
        public string? CharacterDisplayName { get; set; }
        public string? CharacterFirstName { get; set; }
        public string? CharacterMiddleName { get; set; }
        public string? CharacterLastName { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsApproved { get; set; }
        public string? ProfileCss { get; set; }
        public DateTime? LastUpdated { get; set; }
        public DateTime? DateSubmitted { get; set; }
        public int? SubmittedBy { get; set; }
        public bool IsPrivate { get; set; }
        public string? ProfileHtml { get; set; }
        public int LfrpStatus { get; set; }
        public bool DisableLinkify { get; set; }
        public string? CharacterBio { get; set; }
        public int? CharacterGender { get; set; }
        public int? LiteracyLevel { get; set; }
        public int? PostLengthMax { get; set; }
        public int? PostLengthMin { get; set; }
        public bool MatureContent { get; set; }
        public int? SexualOrientation { get; set; }
        public int? EroticaPreferences { get; set; }
        public int? CharacterSourceId { get; set; }
        public int CharacterStatusId { get; set; }
        public int TypeId { get; set; }
        public bool CustomProfileEnabled { get; set; }
        public int? UniverseId { get; set; }
        public string? RecentEvents { get; set; }
        public string? OtherInfo { get; set; }
        public int ViewCount { get; set; }
        public string? CardImageUrl { get; set; }
    }

    public class CharacterWithDetails : Character
    {
        public string? Username { get; set; }
        public string? EmailAddress { get; set; }
        public DateTime? LastAction { get; set; }
        public bool ShowWhenOnline { get; set; }
        public bool IsAdmin { get; set; }
        public bool ShowWriterLinkOnCharacters { get; set; }
        public DateTime? LastLogin { get; set; }
        public string? Gender { get; set; }
        public new string? LiteracyLevel { get; set; }
        public new string? PostLengthMax { get; set; }
        public new string? PostLengthMin { get; set; }
        public new string? SexualOrientation { get; set; }
        public new string? EroticaPreferences { get; set; }
        public string? CharacterStatus { get; set; }
        public string? LfrpStatusName { get; set; }
        public string? CharacterSource { get; set; }
        public string? DisplayImageUrl { get; set; }
        public int? LiteracyLevelId { get; set; }
        public int? PostLengthMaxId { get; set; }
        public int? PostLengthMinId { get; set; }
        public string? UniverseName { get; set; }
        public string? CharacterNameClass { get; set; }
        public string? CharacterType { get; set; }
        public string? AvatarImageUrl { get; set; }
        public int BookmarkCount { get; set; }
        public string? Timezone { get; set; }
    }

    public class CharacterInline
    {
        public int InlineId { get; set; }
        public int CharacterId { get; set; }
        public int UserId { get; set; } // <-- ADD THIS LINE
        public string InlineName { get; set; } = string.Empty;
        public string InlineImageUrl { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
    }

    public class CharacterImage
    {
        public int CharacterImageId { get; set; }
        public int CharacterId { get; set; }
        public string CharacterImageUrl { get; set; } = null!;
        public bool IsPrimary { get; set; }
        public bool IsMature { get; set; }
        public string? ImageCaption { get; set; }
        public int UserId { get; set; }
    }

    public class CharacterAvatar
    {
        public int AvatarId { get; set; }
        public int CharacterId { get; set; }
        public string AvatarImageUrl { get; set; } = null!;
        public DateTime DateCreated { get; set; }
    }

    public class CharacterSearchResult
    {
        public int CharacterId { get; set; }
        public string CharacterDisplayName { get; set; } = null!;
    }
}