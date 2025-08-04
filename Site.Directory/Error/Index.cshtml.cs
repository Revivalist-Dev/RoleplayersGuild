using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Services;

namespace RoleplayersGuild.Site.Directory.Error
{
    public class IndexErrorModel : PageModel
    {
        private readonly ICookieService _cookieService;

        public string ErrorMessage { get; private set; } = string.Empty;
        public bool ShowMarketing { get; private set; }

        [BindProperty(SupportsGet = true)]
        public string? ErrorType { get; set; }

        public IndexErrorModel(ICookieService cookieService)
        {
            _cookieService = cookieService;
        }

        public void OnGet()
        {
            switch (ErrorType?.ToLower())
            {
                case "nocharacter":
                    ErrorMessage = "<p>Sorry, but that character doesn't exist. If you came here by clicking a link it's possible that the character linked was deleted at some point.</p>";
                    break;
                case "nogallery":
                    ErrorMessage = "<p>Sorry, but that character gallery doesn't exist. If you came here by clicking a link it's possible that the character gallery linked was deleted at some point.</p>";
                    break;
                default:
                    ErrorMessage = "<p>Oh no! There seems to have been some error. This is obviously our fault. We are sorry this happened; we will be receiving an email on our end telling us about this error very soon and we promise we'll work on fixing it as soon as possible. Meanwhile, if you'd like to help out, you could <a href=\"/Report/\">reach out to the staff</a> and tell us what, exactly, you were doing when this error happened. This is not, however, required.</p>";
                    break;
            }

            ShowMarketing = _cookieService.GetUserId() == 0;
        }
    }
}