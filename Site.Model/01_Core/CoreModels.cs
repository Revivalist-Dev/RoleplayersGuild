using System;

namespace RoleplayersGuild.Site.Model
{
    // ----------------------------------------------------------------
    // Core Site-Wide & Lookup Models
    // ----------------------------------------------------------------

    public class Ad
    {
        public int AdId { get; set; }
        public int AdTypeId { get; set; }
        public string AdName { get; set; } = null!;
        public string AdImageUrl { get; set; } = null!;
        public string AdLink { get; set; } = null!;
        public bool IsActive { get; set; }
    }

    public class Category
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
    }

    public class ContentRating
    {
        public int ContentRatingId { get; set; }
        public string ContentRatingName { get; set; } = null!;
    }

    public class Genre
    {
        public int GenreId { get; set; }
        public string GenreName { get; set; } = null!;
    }

    public class Gender
    {
        public int GenderId { get; set; }
        public string GenderName { get; set; } = "";
    }

    public class SexualOrientation
    {
        public int SexualOrientationId { get; set; }
        public string OrientationName { get; set; } = "";
    }

    public class CharacterSource
    {
        public int SourceId { get; set; }
        public string SourceName { get; set; } = "";
    }

    public class PostLength
    {
        public int PostLengthId { get; set; }
        public string PostLengthName { get; set; } = "";
    }

    public class LiteracyLevel
    {
        public int LiteracyLevelId { get; set; }
        public string LevelName { get; set; } = "";
    }

    public class LfrpStatus
    {
        public int LfrpStatusId { get; set; }
        public string StatusName { get; set; } = "";
    }

    public class EroticaPreference
    {
        public int EroticaPreferenceId { get; set; }
        public string PreferenceName { get; set; } = "";
    }

    public class ProposalStatus
    {
        public int StatusId { get; set; }
        public string StatusName { get; set; } = null!;
    }

    public class CharacterType
    {
        public int TypeId { get; set; }
        public string? TypeName { get; set; }
    }

    public class AssignableBadge
    {
        public int UserBadgeId { get; set; }
        public string BadgeName { get; set; } = "";
    }
}