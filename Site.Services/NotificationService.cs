using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MimeKit;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IConfiguration _config;
        private readonly IDataService _dataService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRazorViewToStringRenderer _viewRenderer;

        public NotificationService(
            IConfiguration config,
            IDataService dataService,
            IHttpContextAccessor httpContextAccessor,
            IRazorViewToStringRenderer viewRenderer)
        {
            _config = config;
            _dataService = dataService;
            _httpContextAccessor = httpContextAccessor;
            _viewRenderer = viewRenderer;
        }

        public async Task SendNotificationEmailAsync(string sendTo, string subject, string htmlBody)
        {
            var emailSettings = _config.GetSection("SmtpSettings");
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(emailSettings["FromName"], emailSettings["FromAddress"]));
            message.To.Add(MailboxAddress.Parse(sendTo));
            message.Subject = subject;
            message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

            using var client = new SmtpClient();

            // --- MODIFICATION START ---
            // Conditionally set the security option based on appsettings.
            var useSsl = emailSettings.GetValue<bool>("UseSsl");
            var secureSocketOptions = useSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;

            await client.ConnectAsync(
                emailSettings["Host"],
                emailSettings.GetValue<int>("Port"),
                secureSocketOptions);

            // Only authenticate if a username is provided (for production).
            if (!string.IsNullOrEmpty(emailSettings["Username"]))
            {
                await client.AuthenticateAsync(emailSettings["Username"], emailSettings["Password"]);
            }
            // --- MODIFICATION END ---

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        public async Task SendErrorEmailAsync(string errorBody)
        {
            var errorSettings = _config.GetSection("ErrorEmailSettings");
            if (!errorSettings.GetValue<bool>("IsEnabled")) return;

            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(errorSettings["FromAddress"]));
            message.To.Add(MailboxAddress.Parse(errorSettings["ToAddress"]));
            message.Subject = $"Error at RPG - {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            message.Body = new TextPart("plain") { Text = errorBody };

            using var client = new SmtpClient();
            await client.ConnectAsync(errorSettings["Host"], errorSettings.GetValue<int>("Port"), SecureSocketOptions.StartTlsWhenAvailable);
            await client.AuthenticateAsync(errorSettings["Username"], errorSettings["Password"]);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        public async Task NewItemAlertAsync(int itemId, int senderCharacterId, string contentType)
        {
            var senderCharacter = await _dataService.GetRecordAsync<Character>("""SELECT * FROM "CharactersWithDetails" WHERE "CharacterId" = @Id""", new { Id = senderCharacterId });
            if (senderCharacter is null) return;

            IEnumerable<User> recipients;
            if (contentType == "Message")
            {
                const string sql = """
                    SELECT U.* FROM "Users" U JOIN "ThreadUsers" TU ON U."UserId" = TU."UserId"
                    WHERE TU."ThreadId" = @Id AND U."ReceivesThreadNotifications" = TRUE
                    """;
                recipients = await _dataService.GetRecordsAsync<User>(sql, new { Id = itemId });
            }
            else if (contentType == "Image Comment")
            {
                const string sql = """
                    SELECT U.* FROM "Users" U
                    JOIN "Characters" C ON U."UserId" = C."UserId"
                    JOIN "CharacterImages" CI ON C."CharacterId" = CI."CharacterId"
                    WHERE CI."CharacterImageId" = @Id AND U."ReceivesImageCommentNotifications" = TRUE
                    """;
                recipients = await _dataService.GetRecordsAsync<User>(sql, new { Id = itemId });
            }
            else
            {
                return; // Unknown content type
            }

            var request = _httpContextAccessor.HttpContext?.Request;
            var baseUrl = $"{request?.Scheme}://{request?.Host}";

            foreach (var recipient in recipients.Where(r => r.UserId != senderCharacter.UserId))
            {
                if (string.IsNullOrEmpty(recipient.EmailAddress)) continue;

                var model = new NewItemEmailModel
                {
                    SenderName = senderCharacter.CharacterDisplayName ?? "A user",
                    ContentType = contentType,
                    UnsubscribeUrl = $"{baseUrl}/Account-Panel/Email/Unsubscribe?email={Uri.EscapeDataString(recipient.EmailAddress)}"
                };

                var emailBody = await _viewRenderer.RenderViewToStringAsync("/Shared/EmailTemplates/NewItemMessage.cshtml", model);
                var subject = $"[RPG] New {contentType} from {model.SenderName}!";

                await SendNotificationEmailAsync(recipient.EmailAddress, subject, emailBody);
            }
        }

        public async Task SendPasswordResetEmailAsync(User user)
        {
            if (string.IsNullOrEmpty(user.EmailAddress)) return;

            const string tokenSql = """INSERT INTO "RecoveryAttempts" ("UserId") VALUES (@UserId) RETURNING "RecoveryKey";""";
            var token = await _dataService.GetScalarAsync<Guid>(tokenSql, new { UserId = user.UserId });

            var request = _httpContextAccessor.HttpContext?.Request;
            var baseUrl = $"{request?.Scheme}://{request?.Host}";

            var model = new PasswordRecoveryEmailModel
            {
                RecoveryUrl = $"{baseUrl}/Account-Panel/Password/Reset-Password?token={token}",
                UnsubscribeUrl = $"{baseUrl}/Account-Panel/Email/Unsubscribe?email={Uri.EscapeDataString(user.EmailAddress)}"
            };

            // FINAL FIX: Reverting to the full path from the project root.
            var emailBody = await _viewRenderer.RenderViewToStringAsync("/Site.Directory/Shared/EmailTemplates/PasswordRecovery.cshtml", model);
            await SendNotificationEmailAsync(user.EmailAddress, "[RPG] Password Recovery", emailBody);
        }

        public async Task SendMessageToStaffAsync(string threadTitle, string messageContent)
        {
            var staffUsers = await _dataService.GetRecordsAsync<User>("""SELECT * FROM "Users" WHERE "UserTypeId" IN (2, 3, 4)""");
            var systemCharacterId = _config.GetValue<int>("SystemSettings:SystemCharacterId");

            foreach (var staffUser in staffUsers)
            {
                // Each staff member gets their own private thread with the system
                // CORRECTED: Pass the System User ID (1) as the creator of the thread.
                var threadId = await _dataService.CreateNewThreadAsync(threadTitle, 1);
                await _dataService.InsertMessageAsync(threadId, systemCharacterId, messageContent);

                // Add the staff member to the thread using their current "send as" character
                await _dataService.InsertThreadUserAsync(staffUser.UserId, threadId, 2, staffUser.CurrentSendAsCharacter, 1); // 2 = Unread
            }
        }

        public async Task NotifyStoryOwnerOfNewPostAsync(int storyId, int storyOwnerId, int postingCharacterId)
        {
            var postingCharacter = await _dataService.GetCharacterAsync(postingCharacterId);
            if (postingCharacter is null) return;

            // Don't send a notification if the owner is posting in their own story
            if (postingCharacter.UserId == storyOwnerId) return;

            var threadTitle = "[RPG] - New Post in Your Story";
            var message = $"<div class=\"ThreadAlert alert-info\"><p><strong>{postingCharacter.CharacterDisplayName}</strong> has posted in your story. <a href=\"/Stories/View/{storyId}\">Click here to see the new post</a>.</p></div>";

            // CORRECTED: Pass the System User ID (1) as the creator of the thread.
            var threadId = await _dataService.CreateNewThreadAsync(threadTitle, 1);

            var systemCharacterId = _config.GetValue<int>("SystemSettings:SystemCharacterId");
            await _dataService.InsertMessageAsync(threadId, systemCharacterId, message);

            // Get the owner's "send as" character to add them to the thread
            var ownerCharacterId = await _dataService.GetScalarAsync<int>("""SELECT "CurrentSendAsCharacter" FROM "Users" WHERE "UserId" = @UserId""", new { UserId = storyOwnerId });
            if (ownerCharacterId > 0)
            {
                await _dataService.InsertThreadUserAsync(storyOwnerId, threadId, 2, ownerCharacterId, 1); // 2 = Unread
            }
        }
    }
}