namespace RoleplayersGuild.Project.Configuration
{
    public class ImageSettings
    {
        public string ImageHandling { get; set; } = string.Empty;

        // Membership Limits & File Sizes
        public int MaxPerCharacter { get; set; }
        public int BronzeMemberMax { get; set; }
        public int SilverMemberMax { get; set; }
        public int GoldMemberMax { get; set; }
        public int PlatinumMemberMax { get; set; }
        public int MaxFileSizeMb { get; set; }
        public int BronzeMaxFileSizeMb { get; set; }
        public int SilverMaxFileSizeMb { get; set; }
        public int GoldMaxFileSizeMb { get; set; }
        public int PlatinumMaxFileSizeMb { get; set; }
    }
}