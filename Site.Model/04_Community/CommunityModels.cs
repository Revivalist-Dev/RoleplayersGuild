using System;

namespace RoleplayersGuild.Site.Model
{
    // ----------------------------------------------------------------
    // Community Interaction Models (Chat, Threads, Proposals)
    // ----------------------------------------------------------------

    public class ChatRoom
    {
        public int ChatRoomId { get; set; }
        public int? SubmittedByUserId { get; set; }
        public string? ChatRoomName { get; set; }
        public int? ContentRatingId { get; set; }
        public int? UniverseId { get; set; }
        public int ChatRoomStatusId { get; set; }
        public string? ChatRoomDescription { get; set; }
        public bool IsPublic { get; set; }
    }

    public class ChatRoomWithDetails : ChatRoom
    {
        public string? ChatRoomStatusName { get; set; }
        public int? UniverseOwnerId { get; set; }
        public string? Username { get; set; }
        public string? UniverseName { get; set; }
        public string? ContentRating { get; set; }
        public string? ContentRatingDescription { get; set; }
        public DateTime? LastPostTime { get; set; }
    }

    public class ChatRoomPostsWithDetails
    {
        public int ChatPostId { get; set; }
        public int ChatRoomId { get; set; }
        public int UserId { get; set; }
        public string? PostContent { get; set; }
        public DateTime PostDateTime { get; set; }
        public int CharacterId { get; set; }
        public string? CharacterDisplayName { get; set; }
        public string? CharacterThumbnail { get; set; }
        public string? CharacterNameClass { get; set; }
    }

    public class Thread
    {
        public int ThreadId { get; set; }
        public string ThreadTitle { get; set; } = null!;
        public DateTime? LastMessage { get; set; }
        public int? CreatedBy { get; set; }
        public int? ForumId { get; set; }
        public DateTime DateCreated { get; set; }
    }

    public class ThreadMessage
    {
        public int ThreadMessageId { get; set; }
        public int ThreadId { get; set; }
        public int CreatorId { get; set; }
        public string MessageContent { get; set; } = null!;
        public DateTime Timestamp { get; set; }
        public DateTime? MessageLastEditDate { get; set; }
        public int? LastEditedBy { get; set; }
    }

    public class ThreadDetails
    {
        public int ThreadId { get; set; }
        public string ThreadTitle { get; set; } = null!;
        public DateTime? LastMessage { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public int? CreatorUserId { get; set; }
        public int UserId { get; set; }
        public int ReadStatusId { get; set; }
        public int CharacterId { get; set; }
        public string? CreatorUsername { get; set; }
        public string? LastMessageContent { get; set; }
        public string? LastMessageCharacterName { get; set; }
    }

    public class Proposal
    {
        public int ProposalId { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int ContentRatingId { get; set; }
        public int StatusId { get; set; }
        public bool IsPrivate { get; set; }
        public bool DisableLinkify { get; set; }
    }

    public class ProposalWithDetails : Proposal
    {
        public int? UniverseId { get; set; }
        public string? Username { get; set; }
        public string? ContentRating { get; set; }
        public string? StatusName { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}