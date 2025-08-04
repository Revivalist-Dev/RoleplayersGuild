using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RoleplayersGuild.Site.Model
{
    // ----------------------------------------------------------------
    // Creative Content Input Models (Universes, Articles, Stories)
    // ----------------------------------------------------------------

    public class ArticleInputModel
    {
        public int ArticleId { get; set; }
        [Required(ErrorMessage = "You must enter a title for the article.")]
        [StringLength(50)]
        public string? ArticleTitle { get; set; }
        public string? ArticleContent { get; set; }
        public int? CategoryId { get; set; }
        public int? UniverseId { get; set; }
        public int? ContentRatingId { get; set; }
        public bool IsPrivate { get; set; }
        public bool DisableLinkify { get; set; }
        public List<int> SelectedGenreIds { get; set; } = new();

        public ArticleInputModel() { }
        public ArticleInputModel(ArticleWithDetails article)
        {
            ArticleId = article.ArticleId;
            ArticleTitle = article.ArticleTitle;
            ArticleContent = article.ArticleContent;
            CategoryId = article.CategoryId;
            UniverseId = article.UniverseId;
            ContentRatingId = article.ContentRatingId;
            IsPrivate = article.IsPrivate;
            DisableLinkify = article.DisableLinkify;
        }
    }

    public class UniverseInputModel
    {
        public int UniverseId { get; set; }

        [Required]
        [Display(Name = "Universe Name")]
        public string UniverseName { get; set; } = string.Empty;

        [Display(Name = "Universe Description")]
        public string? UniverseDescription { get; set; }

        [Display(Name = "Content Rating")]
        public int ContentRatingId { get; set; }

        [Display(Name = "Source Material")]
        public int SourceTypeId { get; set; }

        [Display(Name = "Character submissions require my approval")]
        public bool RequiresApprovalOnJoin { get; set; }

        [Display(Name = "Disable automatic links in description")]
        public bool DisableLinkify { get; set; }

        public List<int> SelectedGenreIds { get; set; } = new();

        public UniverseInputModel() { }
        public UniverseInputModel(Universe dbModel)
        {
            UniverseId = dbModel.UniverseId;
            UniverseName = dbModel.UniverseName ?? string.Empty;
            UniverseDescription = dbModel.UniverseDescription;
            ContentRatingId = dbModel.ContentRatingId ?? 0;
            SourceTypeId = dbModel.SourceTypeId ?? 0;
            RequiresApprovalOnJoin = dbModel.RequiresApprovalOnJoin;
            DisableLinkify = dbModel.DisableLinkify;
        }
    }

    public class StoryInputModel
    {
        public int StoryId { get; set; }
        [Required] public string? StoryTitle { get; set; }
        public string? StoryDescription { get; set; }
        public int? UniverseId { get; set; }
        public int? ContentRatingId { get; set; }
        public bool IsPrivate { get; set; }
        public List<int> SelectedGenreIds { get; set; } = new();

        public StoryInputModel() { }
        public StoryInputModel(StoryWithDetails s)
        {
            StoryId = s.StoryId;
            StoryTitle = s.StoryTitle;
            StoryDescription = s.StoryDescription;
            UniverseId = s.UniverseId;
            ContentRatingId = s.ContentRatingId;
            IsPrivate = s.IsPrivate;
        }
    }

    public class PostInputModel
    {
        public int StoryPostId { get; set; }
        public int StoryId { get; set; }
        [Required]
        public int CharacterId { get; set; }
        [Required]
        public string PostContent { get; set; } = string.Empty;
    }

    public class ArticleSearchInputModel
    {
        public string? SearchTerm { get; set; }
        public List<int> SelectedGenreIds { get; set; } = new();
    }
}