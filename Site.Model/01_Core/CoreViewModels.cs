using System;
using System.Collections.Generic;
using System.Linq;

namespace RoleplayersGuild.Site.Model
{
    // ----------------------------------------------------------------
    // Core Site-Wide & Reusable View Models
    // ----------------------------------------------------------------

    public class GenreSelectionViewModel
    {
        public int GenreId { get; set; }
        public string GenreName { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }

    public class FilterNavViewModel
    {
        public List<GenreSelectionViewModel> AllGenres { get; set; } = new();
    }

    public class DashboardFunding
    {
        public decimal CurrentFundingAmount { get; set; }
        public decimal GoalAmount { get; set; }
        public int ProgressPercentage { get; set; }
    }

    public class DashboardItemViewModel
    {
        public string Url { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string DetailLeft { get; set; } = string.Empty; // e.g., Rating
        public string DetailRight { get; set; } = string.Empty; // e.g., Author or Time
    }

    public class PreviewRequest
    {
        public string? Text { get; set; }
        public int CharacterId { get; set; }
    }

    public abstract class PagedResultBase
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;
    }

    public class PagedResult<T> : PagedResultBase
    {
        public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    }

    public class SiteFooterViewModel
    {
        public int CurrentYear { get; set; }
    }
}