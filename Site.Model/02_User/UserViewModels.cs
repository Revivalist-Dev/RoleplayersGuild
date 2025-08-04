using System.Collections.Generic;

namespace RoleplayersGuild.Site.Model
{
    // ----------------------------------------------------------------
    // User-Specific & Account-Related View Models
    // ----------------------------------------------------------------

    public class UserListingViewModel { public int UserId { get; set; } public string? Username { get; set; } }

    public class UserBadgeViewModel { public int UserBadgeId { get; set; } public string BadgeName { get; set; } = ""; public string BadgeImageUrl { get; set; } = ""; public string? BadgeDescription { get; set; } }

    public class UserNoteViewModel { public string NoteContent { get; set; } = ""; public string CreatedByUsername { get; set; } = ""; public System.DateTime NoteTimestamp { get; set; } }

    public class Badge { public int BadgeId { get; set; } public string BadgeName { get; set; } = ""; }

    public class UserNavViewModel
    {
        public bool IsAuthenticated { get; set; }
        public bool IsStaff { get; set; }
        public int UnreadThreadCount { get; set; }
        public int UnreadImageCommentCount { get; set; }
        public int AdminOpenItemCount { get; set; }
        public int TotalNotificationCount => UnreadThreadCount + UnreadImageCommentCount + (IsStaff ? AdminOpenItemCount : 0);
        public List<QuickLink> QuickLinks { get; set; } = new();
    }

    public class BadgeSelectionViewModel
    {
        public int UserBadgeId { get; set; }
        public string BadgeName { get; set; } = string.Empty;
        public string BadgeImageUrl { get; set; } = string.Empty;
        public bool IsDisplayed { get; set; }
    }

    public class ImageLimitResults
    {
        public int MaxSlots { get; set; }
        public int MaxSizeMb { get; set; }
    }
}