namespace RoleplayersGuild.Site.Model
{
    public class CharacterRecipient
    {
        public int UserId { get; set; }
        public int CharacterId { get; set; }
        public string? CharacterDisplayName { get; set; }
    }

    public class UserRecipient
    {
        public int UserId { get; set; }
        public string? Username { get; set; }
        public int CurrentSendAsCharacter { get; set; }
    }
}
