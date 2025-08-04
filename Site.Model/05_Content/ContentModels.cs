using System;
using System.Collections.Generic;

namespace RoleplayersGuild.Site.Model
{
    // ----------------------------------------------------------------
    // Creative Content Models (Universes, Stories, Articles)
    // ----------------------------------------------------------------

    public class Universe
    {
        public int UniverseId { get; set; }
        public string? UniverseName { get; set; }
        public string? UniverseDescription { get; set; }
        public int UniverseOwnerId { get; set; }
        public int? SubmittedById { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ContentRatingId { get; set; }
        public int? SourceTypeId { get; set; }
        public int? StatusId { get; set; }
        public bool RequiresApprovalOnJoin { get; set; }
        public bool DisableLinkify { get; set; }
    }

    public class UniverseWithDetails : Universe
    {
        public string? SourceType { get; set; }
        public string? ContentRating { get; set; }
        public int CharacterCount { get; set; }
        public int ChatRoomCount { get; set; }
        public List<string> Genres { get; set; } = new();
    }

    public class Article
    {
        public int ArticleId { get; set; }
        public int OwnerUserId { get; set; }
        public int? CategoryId { get; set; }
        public string? ArticleTitle { get; set; }
        public string? ArticleContent { get; set; }
        public DateTime DateSubmitted { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public bool IsPublished { get; set; }
        public int? ContentRatingId { get; set; }
        public bool IsPrivate { get; set; }
        public bool DisableLinkify { get; set; }
        public int? UniverseId { get; set; }
    }

    public class ArticleWithDetails : Article
    {
        public string? CategoryName { get; set; }
        public string? AuthorUsername { get; set; }
        public string? OwnerUsername { get; set; }
        public string? ContentRating { get; set; }
    }

    public class Story
    {
        public int StoryId { get; set; }
        public int UserId { get; set; }
        public string? StoryTitle { get; set; }
        public string? StoryContent { get; set; }
        public string? StoryDescription { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? LastUpdated { get; set; }
        public int? ContentRatingId { get; set; }
        public int? UniverseId { get; set; }
        public bool IsPrivate { get; set; }
    }

    public class StoryWithDetails : Story
    {
        public string? AuthorUsername { get; set; }
        public string? UniverseName { get; set; }
        public string? StoryRating { get; set; }
        public string? ContentRating { get; set; }
    }

    public class StoryPost
    {
        public int StoryPostId { get; set; }
        public int StoryId { get; set; }
        public int CharacterId { get; set; }
        public string? PostContent { get; set; }
        public DateTime PostDateTime { get; set; }
    }
}