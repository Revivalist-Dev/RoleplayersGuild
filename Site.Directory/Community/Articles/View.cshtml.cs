using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using RoleplayersGuild.Site.Services.DataServices;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.Community.Articles
{
    public class ViewArticleModel : PageModel
    {
        private readonly IContentDataService _contentDataService;
        private readonly IUserDataService _userDataService;
        private readonly IMiscDataService _miscDataService;
        private readonly IUserService _userService;

        public ArticleWithDetails? Article { get; set; }
        public List<string> Genres { get; set; } = new();
        public string? Message { get; set; }

        public ViewArticleModel(IContentDataService contentDataService, IUserDataService userDataService, IMiscDataService miscDataService, IUserService userService)
        {
            _contentDataService = contentDataService;
            _userDataService = userDataService;
            _miscDataService = miscDataService;
            _userService = userService;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Article = await _contentDataService.GetArticleWithDetailsAsync(id);
            var currentUserId = _userService.GetUserId(User);

            if (Article is null)
            {
                return NotFound();
            }


            if (currentUserId != 0)
            {
                var ownerId = Article.OwnerUserId;
                var viewerIsBlocked = await _userDataService.GetBlockRecordIdAsync(currentUserId, ownerId) > 0;
                var ownerIsBlocked = await _userDataService.GetBlockRecordIdAsync(ownerId, currentUserId) > 0;

                if ((viewerIsBlocked || ownerIsBlocked) && !await _userService.IsCurrentUserStaffAsync())
                {
                    Message = "You cannot view this article due to a block between you and the author.";
                    Article = null;
                    return Page();
                }
            }

            if (Article.IsPrivate && Article.OwnerUserId != currentUserId && !await _userService.IsCurrentUserStaffAsync())
            {
                return NotFound();
            }

            var genreIds = await _contentDataService.GetArticleGenresAsync(id);
            if (genreIds.Any())
            {
                var allGenres = await _miscDataService.GetGenresAsync();
                // Corrected: Property access is now g.GenreId
                Genres = allGenres.Where(g => genreIds.Contains(g.GenreId)).Select(g => g.GenreName).ToList();
            }

            return Page();
        }
    }
}
