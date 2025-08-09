using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace RoleplayersGuild.Site.Model
{
    // ----------------------------------------------------------------
    // Character-Specific View Models
    // ----------------------------------------------------------------

    public class CharactersForListing
    {
        public int CharacterId { get; set; }
        public int UserId { get; set; }
        public string? CharacterDisplayName { get; set; }
        public string? CharacterFirstName { get; set; }
        public string? CharacterMiddleName { get; set; }
        public string? CharacterLastName { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsApproved { get; set; }
        public DateTime? LastAction { get; set; }
        public bool ShowWhenOnline { get; set; }
        public string? CardImageUrl { get; set; }
        public string? AvatarImageUrl { get; set; }
        public string? CharacterNameClass { get; set; }
        public int TypeId { get; set; }
        public int LfrpStatus { get; set; }
        public bool IsPrivate { get; set; }
        public int UserTypeId { get; set; }
        public int? GenderId { get; set; }
        public int CharacterStatusId { get; set; }
        public int? CharacterSourceId { get; set; }
        public int? SexualOrientationId { get; set; }
        public int? EroticaPreferenceId { get; set; }
        public int? PostLengthMinId { get; set; }
        public int? PostLengthMaxId { get; set; }
        public int? LiteracyLevelId { get; set; }
        public bool IsOnline { get; set; }
        public DateTime? DateSubmitted { get; set; }
    }

    public class CharacterListingViewModel
    {
        public List<CharactersForListing> Characters { get; set; } = new();
        public string? Title { get; set; }
        public string? DisplaySize { get; set; }
        public bool ShowFooter { get; set; }
    }

    public class CharacterImageWithDetails : CharacterImage
    {
        public string CharacterDisplayName { get; set; } = string.Empty;
    }

    public class ImageDetailViewModel
    {
        public int CharacterImageId { get; set; }
        public int CharacterId { get; set; }
        public int UserId { get; set; }
        public string CharacterDisplayName { get; set; } = string.Empty;
        public string CharacterImageUrl { get; set; } = string.Empty;
        public string? ImageCaption { get; set; }
        public bool IsMature { get; set; }
    }

    public class ImageCommentViewModel
    {
        public int ImageCommentId { get; set; }
        public int ImageId { get; set; }
        public int CommenterCharacterId { get; set; }
        public string? CommenterUsername { get; set; }
        public string CommentText { get; set; } = string.Empty;
        public DateTime CommentTimestamp { get; set; }
        public int ImageOwnerUserId { get; set; }
        public string? CharacterNameClass { get; set; }
        public string? CharacterImageUrl { get; set; }
    }

    public class ProfileFrameViewModel
    {
        public string FrameName { get; set; } = string.Empty;
        public string ContentHtml { get; set; } = string.Empty;
    }

    public class CharacterSimpleViewModel
    {
        public int CharacterId { get; set; }
        public string CharacterDisplayName { get; set; } = string.Empty;
    }
}
