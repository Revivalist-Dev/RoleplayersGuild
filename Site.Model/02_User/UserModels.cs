using System;

namespace RoleplayersGuild.Site.Model
{
    // ----------------------------------------------------------------
    // User Account & Profile Models
    // ----------------------------------------------------------------

    public class User
    {
        public int UserId { get; set; }
        public string? EmailAddress { get; set; }
        public string Password { get; set; } = null!;
        public string? Username { get; set; }
        public string? AboutMe { get; set; }
        public DateTime? LastAction { get; set; }
        public bool ShowWhenOnline { get; set; }
        public bool ShowMatureContent { get; set; }
        public bool ReceivesThreadNotifications { get; set; }
        public bool ReceivesImageCommentNotifications { get; set; }
        public bool ReceivesWritingCommentNotifications { get; set; }
        public bool ReceivesDevEmails { get; set; }
        public bool ReceivesErrorFixEmails { get; set; }
        public bool ReceivesGeneralEmailBlasts { get; set; }
        public bool IsAdmin { get; set; }
        public bool ReceiveAdminAnnouncements { get; set; }
        public DateTime? LastHalloweenBadge { get; set; }
        public DateTime? LastChristmasBadge { get; set; }
        public int CurrentSendAsCharacter { get; set; }
        public int MembershipTypeId { get; set; }
        public bool ShowWriterLinkOnCharacters { get; set; }
        public DateTime? LastLogin { get; set; }
        public int UserTypeId { get; set; }
        public bool HideStream { get; set; }
        public bool UseDarkTheme { get; set; }
        public DateTime MemberJoinedDate { get; set; }
        public int? ReferredBy { get; set; }
        public string? Timezone { get; set; }
    }

    public class RecoveryAttempt
    {
        public int RecoveryAttemptId { get; set; }
        public int UserId { get; set; }
        public string? IpAddress { get; set; }
        public string? AttemptedEmail { get; set; }
        public DateTime AttemptTimestamp { get; set; }
        public Guid RecoveryKey { get; set; }
        public bool RecoveryKeyUsed { get; set; }
    }

    public class QuickLink
    {
        public int QuickLinkId { get; set; }
        public int UserId { get; set; }
        public string LinkName { get; set; } = string.Empty;
        public string LinkAddress { get; set; } = string.Empty;
        public int OrderNumber { get; set; }
    }
}