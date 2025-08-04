using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.Community.Articles
{
    public class ViewArticleModel : PageModel
    {
        private readonly IDataService _dataService;
        private readonly ICookieService _cookieService;

        public ArticleWithDetails? Article { get; set; }
        public List<string> Genres { get; set; } = new();
        public string? Message { get; set; }
        public bool IsAdmin { get; set; }

        public ViewArticleModel(IDataService dataService, ICookieService cookieService)
        {
            _dataService = dataService;
            _cookieService = cookieService;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Article = await _dataService.GetArticleWithDetailsAsync(id);
            var currentUserId = _cookieService.GetUserId();

            if (Article is null)
            {
                return NotFound();
            }

            IsAdmin = _cookieService.GetIsStaff();

            if (currentUserId != 0)
            {
                var ownerId = Article.OwnerUserId;
                var viewerIsBlocked = await _dataService.GetBlockRecordIdAsync(currentUserId, ownerId) > 0;
                var ownerIsBlocked = await _dataService.GetBlockRecordIdAsync(ownerId, currentUserId) > 0;

                if ((viewerIsBlocked || ownerIsBlocked) && !IsAdmin)
                {
                    Message = "You cannot view this article due to a block between you and the author.";
                    Article = null;
                    return Page();
                }
            }

            if (Article.IsPrivate && Article.OwnerUserId != currentUserId && !IsAdmin)
            {
                return NotFound();
            }

            var genreIds = await _dataService.GetArticleGenresAsync(id);
            if (genreIds.Any())
            {
                var allGenres = await _dataService.GetGenresAsync();
                // Corrected: Property access is now g.GenreId
                Genres = allGenres.Where(g => genreIds.Contains(g.GenreId)).Select(g => g.GenreName).ToList();
            }

            return Page();
        }
    }
}