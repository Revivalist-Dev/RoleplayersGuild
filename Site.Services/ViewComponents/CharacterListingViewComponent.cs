using Microsoft.AspNetCore.Mvc;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using RoleplayersGuild.Site.Services.DataServices;
using System.Linq;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Services.ViewComponents
{
    public class CharacterListingViewComponent : ViewComponent
    {
        private readonly ICharacterDataService _characterDataService;
        private readonly IUserService _userService;
        private readonly IUrlProcessingService _urlProcessingService;

        public CharacterListingViewComponent(ICharacterDataService characterDataService, IUserService userService, IUrlProcessingService urlProcessingService)
        {
            _characterDataService = characterDataService;
            _userService = userService;
            _urlProcessingService = urlProcessingService;
        }

        public async Task<IViewComponentResult> InvokeAsync(
            string? displaySize,
            bool showFooter,
            string screenStatus,
            int recordCount)
        {
            var currentUserId = _userService.GetUserId(UserClaimsPrincipal);

            var characters = (await _characterDataService.GetCharactersForListingAsync(screenStatus, recordCount, currentUserId)).ToList();

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
                Characters = characters
            };

            return View(viewModel);
        }
    }
}
