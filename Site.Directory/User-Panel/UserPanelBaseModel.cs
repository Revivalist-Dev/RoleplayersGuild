// In F:\Visual Studio\RoleplayersGuild\Site.Directory\User-Panel\UserPanelBaseModel.cs

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Services;
using RoleplayersGuild.Site.Services.DataServices;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.User_Panel
{
    public abstract class UserPanelBaseModel : PageModel
    {
        // Injected services are now protected for inheriting classes to use
        protected readonly ICharacterDataService _characterDataService;
        protected readonly ICommunityDataService _communityDataService;
        protected readonly IMiscDataService _miscDataService;
        protected readonly IUserService _userService;


        [TempData]
        public string? Message { get; set; }

        public int LoggedInUserId { get; private set; }

        // UPDATED: Constructor now uses IUserService
        protected UserPanelBaseModel(ICharacterDataService characterDataService, ICommunityDataService communityDataService, IMiscDataService miscDataService, IUserService userService)
        {
            _characterDataService = characterDataService;
            _communityDataService = communityDataService;
            _miscDataService = miscDataService;
            _userService = userService;
        }

        public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            // UPDATED: Get the user ID from the reliable ClaimsPrincipal
            LoggedInUserId = _userService.GetUserId(context.HttpContext.User);
            if (LoggedInUserId == 0)
            {
                // User is not authenticated, send to the home page.
                context.Result = RedirectToPage("/Index");
                return;
            }

            // NEW: Check if the user has created their first character.
            // The Dashboard is a special case; it should always be accessible, so we bypass the character check.
            if (context.ActionDescriptor.DisplayName != null && context.ActionDescriptor.DisplayName.Contains("Dashboard"))
            {
                await next.Invoke();
                return;
            }
            var characterCount = await _characterDataService.GetCharacterCountAsync(LoggedInUserId);
            if (characterCount == 0)
            {
                // User has no characters. They must be sent to the creation page,
                // unless they are already there.
                if (context.ActionDescriptor.DisplayName is not null && !context.ActionDescriptor.DisplayName.Contains("My-Characters/Edit"))
                {
                    context.Result = RedirectToPage("/User-Panel/My-Characters/Edit");
                    return;
                }
            }

            // If all checks pass, continue to the page handler.
            await next.Invoke();
        }
    }
}
