using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RoleplayersGuild.Site.Services;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Directory.Account_Panel.Email
{
    public class UnsubscribeModel : PageModel
    {
        private readonly IDataService _dataService;

        public UnsubscribeModel(IDataService dataService)
        {
            _dataService = dataService;
        }

        public string Message { get; private set; } = "An error occurred. The link may be invalid or expired.";

        // Correctly accepts the 'email' parameter from the URL
        public async Task OnGetAsync(string? email)
        {
            if (string.IsNullOrEmpty(email))
            {
                // The message is already set to a default error state.
                return;
            }

            // Find the user by their email address
            var user = await _dataService.GetUserByEmailAsync(email);

            if (user != null)
            {
                // Unsubscribe them and set a success message
                await _dataService.UnsubscribeUserFromNotificationsAsync(user.UserId);
                Message = $"The address {user.EmailAddress} has been successfully unsubscribed.";
            }
            else
            {
                // Handle cases where the email doesn't exist in the database
                Message = "The email address provided was not found in our system.";
            }
        }
    }
}