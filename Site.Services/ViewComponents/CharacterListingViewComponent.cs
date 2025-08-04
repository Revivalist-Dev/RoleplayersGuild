using Microsoft.AspNetCore.Mvc;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using System.Linq;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Services.ViewComponents
{
    public class CharacterListingViewComponent : ViewComponent
    {
        private readonly IDataService _dataService;
        private readonly IUserService _userService;

        // IConfiguration has been removed from the constructor
        public CharacterListingViewComponent(IDataService dataService, IUserService userService)
        {
            _dataService = dataService;
            _userService = userService;
        }

        public async Task<IViewComponentResult> InvokeAsync(
            string? displaySize,
            bool showFooter,
            string screenStatus,
            int recordCount)
        {
            var currentUserId = _userService.GetUserId(UserClaimsPrincipal);

            // 1. Get the character data. The DataService now handles all image URL processing.
            var characters = await _dataService.GetCharactersForListingAsync(screenStatus, recordCount, currentUserId);

            // 2. The entire loop that modified DisplayImageUrl has been removed.

            string title = screenStatus switch
            {
                "OnlineCharacters" => "Online Characters",
                "NewCharacters" => "New Characters",
                _ => "Characters"
            };

            var viewModel = new CharacterListingViewModel
            {
                Title = title,
                DisplaySize = string.IsNullOrEmpty(displaySize) ? "profile-card-vertical" : displaySize,
                ShowFooter = showFooter,
                Characters = characters.ToList() // Pass the data directly to the view
            };

            return View(viewModel);
        }
    }
}