namespace RoleplayersGuild.Site.Services.Models
{
    /// <summary>
    /// For the NewItemMessage.cshtml template.
    /// </summary>
    public class NewItemEmailModel
    {
        public string SenderName { get; set; } = "A user";
        public string ContentType { get; set; } = "item";
        public string UnsubscribeUrl { get; set; } = "";
    }

    /// <summary>
    /// For the PasswordRecovery.cshtml template.
    /// </summary>
    public class PasswordRecoveryEmailModel
    {
        public string RecoveryUrl { get; set; } = "";
        public string UnsubscribeUrl { get; set; } = "";
    }

    /// <summary>
    /// For the NewMembershipSubscription.cshtml template.
    /// </summary>
    public class MembershipEmailModel
    {
        public string MembershipType { get; set; } = "Valued";
    }

    /// <summary>
    /// For the PaymentReceivedMessage.cshtml template.
    /// </summary>
    public class PaymentReceivedEmailModel
    {
        public string PurchasedItem { get; set; } = "Purchase";
        public string PaymentAmount { get; set; } = "$0.00";
        public string TransactionId { get; set; } = "N/A";
    }

    /// <summary>
    /// For the PaymentFailedMessage.cshtml template.
    /// </summary>
    public class PaymentFailedEmailModel
    {
        public string TransactionId { get; set; } = "N/A";
    }
}