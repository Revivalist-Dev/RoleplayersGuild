using Microsoft.AspNetCore.Mvc;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace RoleplayersGuild.Site.Services.ViewComponents
{
    public class FilterNavViewComponent : ViewComponent
    {
        private readonly IDataService _dataService;

        public FilterNavViewComponent(IDataService dataService)
        {
            _dataService = dataService;
        }

        public async Task<IViewComponentResult> InvokeAsync(List<int> selectedGenreIds)
        {
            var allGenres = await _dataService.GetGenresAsync();

            var model = new FilterNavViewModel
            {
                AllGenres = allGenres.Select(g => new GenreSelectionViewModel
                {
                    GenreId = g.GenreId,
                    // Corrected: The property is named GenreName
                    GenreName = g.GenreName,
                    IsSelected = selectedGenreIds?.Contains(g.GenreId) ?? false
                }).ToList()
            };

            return View(model);
        }
    }
}