namespace RoleplayersGuild.Project.Configuration
{
    public class ImageSettings
    {
        // Local Physical Storage Paths (for saving files)
        public string LocalImagesStoragePath { get; set; } = string.Empty;
        public string LocalInlinesStoragePath { get; set; } = string.Empty;
        public string LocalAvatarsStoragePath { get; set; } = string.Empty;
        public string LocalCardsStoragePath { get; set; } = string.Empty;

        // Web Display Paths (for <img src="...">)
        public string DisplayCharacterImagesFolder { get; set; } = string.Empty;
        public string DisplayCharacterInlinesFolder { get; set; } = string.Empty;
        public string DisplayCharacterAvatarsFolder { get; set; } = string.Empty;
        public string DisplayCharacterCardsFolder { get; set; } = string.Empty;

        // Membership Limits & File Sizes
        public int MaxPerCharacter { get; set; }
        public int BronzeMemberMax { get; set; }
        public int SilverMemberMax { get; set; }
        public int GoldMemberMax { get; set; }
        public int PlatinumMemberMax { get; set; }
        public int MaxFileSizeMb { get; set; }
        public int BronzeMaxFileSizeMb { get; set; }
        public int SilverMaxFileSizeMb { get; set; } // Corrected
        public int GoldMaxFileSizeMb { get; set; }
        public int PlatinumMaxFileSizeMb { get; set; }
    }
}