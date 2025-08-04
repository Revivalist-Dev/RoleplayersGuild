using RoleplayersGuild.Site.Model;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Services
{
    public interface INotificationService
    {
        Task SendNotificationEmailAsync(string sendTo, string subject, string htmlBody);
        Task SendErrorEmailAsync(string errorBody);
        Task NewItemAlertAsync(int itemId, int senderCharacterId, string contentType);
        // The 'token' is now handled internally, so we just need the user object
        Task SendPasswordResetEmailAsync(User user);
        Task SendMessageToStaffAsync(string threadTitle, string messageContent);
        Task NotifyStoryOwnerOfNewPostAsync(int storyId, int storyOwnerId, int postingCharacterId);
    }
}