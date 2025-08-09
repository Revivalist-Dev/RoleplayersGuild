using System;

namespace RoleplayersGuild.Site.Model
{
    // ----------------------------------------------------------------
    // Community-Interaction View Models (Chat, Threads, etc.)
    // ----------------------------------------------------------------

    public class ToDoItemViewModel
    {
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public string? ItemDescription { get; set; }
        public int VoteCount { get; set; }
        public bool HasVoted { get; set; }
    }

    public class DashboardChatRoom
    {
        public int ChatRoomId { get; set; }
        public string? ChatRoomName { get; set; }
        public string? ContentRating { get; set; }
        public DateTime? LastPostTime { get; set; }
    }

    public class DashboardProposal
    {
        public int ProposalId { get; set; }
        public string? Title { get; set; }
        public string? Username { get; set; }
        public string? ContentRating { get; set; }
        public int UserId { get; set; }
        public string? Description { get; set; }
        public int ContentRatingId { get; set; }
        public int StatusId { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime LastUpdated { get; set; }
        public int? UniverseId { get; set; }
        public bool IsPrivate { get; set; }
        public bool DisableLinkify { get; set; }
        public string? StatusName { get; set; }
    }

    public class ThreadMessageViewModel
    {
        public int ThreadMessageId { get; set; }
        public int ThreadId { get; set; }
        public int CreatorId { get; set; }
        public string MessageContent { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string? CharacterDisplayName { get; set; }
        public string? AvatarImageUrl { get; set; }
        public string? CharacterNameClass { get; set; }
    }

    public class ThreadParticipantViewModel
    {
        public int CharacterId { get; set; }
        public string CharacterDisplayName { get; set; } = string.Empty;
    }

    public class ChatCharacterViewModel
    {
        public int CharacterId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string CharacterDisplayName { get; set; } = string.Empty;
        public string AvatarImageUrl { get; set; } = string.Empty;
        public string CharacterFirstName { get; set; } = string.Empty;
        public string CharacterMiddleName { get; set; } = string.Empty;
        public string CharacterLastName { get; set; } = string.Empty;
        public string CharacterNameClass { get; set; } = string.Empty;
    }

    public class ChatParticipantViewModel
    {
        public int CharacterId { get; set; }
        public string? CharacterDisplayName { get; set; }
        public string? AvatarImageUrl { get; set; }
    }

    public class ChannelViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int UserCount { get; set; }
    }

    public class ChatMessageViewModel
    {
        public ChatCharacterViewModel Sender { get; set; } = new();
        public string Text { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Type { get; set; } = "normal";
    }
}
