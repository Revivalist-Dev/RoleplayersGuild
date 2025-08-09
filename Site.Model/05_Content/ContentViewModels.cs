using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RoleplayersGuild.Site.Model
{
    // ----------------------------------------------------------------
    // Creative Content View Models (Articles, Stories, etc.)
    // ----------------------------------------------------------------

    public class ArticleForListingViewModel
    {
        public int ArticleId { get; set; }
        public string? ArticleTitle { get; set; }
        public string? ContentRating { get; set; }
        public List<string> Genres { get; set; } = new();
        public string? CategoryName { get; set; }
        public int OwnerUserId { get; set; }
        public bool IsPrivate { get; set; }
        public bool IsPublished { get; set; }
        public DateTime DateSubmitted { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public int? CategoryId { get; set; }
        public int? UniverseId { get; set; }
        public string? Username { get; set; }
    }

    public class StoryForListingViewModel
    {
        public int StoryId { get; set; }
        public string? StoryTitle { get; set; }
        public string? ContentRating { get; set; }
        public List<string> Genres { get; set; } = new();
        public int UserId { get; set; }
        public string? StoryDescription { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? LastUpdated { get; set; }
        public int? ContentRatingId { get; set; }
        public bool IsPrivate { get; set; }
        public int? UniverseId { get; set; }
        public string? AuthorUsername { get; set; }
        public string? UniverseName { get; set; }
    }

    public class StoryPostViewModel
    {
        public int StoryPostId { get; set; }
        public int CharacterId { get; set; }
        public int CharacterOwnerUserId { get; set; }
        public string? PostContent { get; set; }
        public System.DateTime DatePosted { get; set; }
        public string? CharacterDisplayName { get; set; }
        public string? CharacterNameClass { get; set; }
        public string? CardImageUrl { get; set; }
        public bool ShowWhenOnline { get; set; }
        public System.DateTime? LastAction { get; set; }
    }

    public class DashboardStory
    {
        public int StoryId { get; set; }
        public string? StoryTitle { get; set; }
        public string? ContentRating { get; set; }
        public int UserId { get; set; }
        public DateTime? LastPostDateTime { get; set; }
        public string? StoryContent { get; set; }
        public long PostCount { get; set; }
    }

    public class DashboardArticle
    {
        public int ArticleId { get; set; }
        public string? ArticleTitle { get; set; }
        public string? Username { get; set; }
        public string? CategoryName { get; set; }
        public string? ContentRating { get; set; }
        public int OwnerUserId { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public bool IsPrivate { get; set; }
    }

    public class ArticleViewModel
    {
        public int ArticleId { get; set; }
        public string? ArticleTitle { get; set; }
        public string? CategoryName { get; set; }
        public string? ContentRating { get; set; }
        public List<string> Genres { get; set; } = new();
    }

    public class ArticleEditModel
    {
        public int ArticleId { get; set; }
        [Required, MaxLength(50)]
        public string? ArticleTitle { get; set; }
        public string? ArticleContent { get; set; }
        public int CategoryId { get; set; }
        public int ContentRatingId { get; set; }
        public bool DisableLinkify { get; set; }
        public int OwnerUserId { get; set; }
        public string? OwnerUserName { get; set; }
    }
}
