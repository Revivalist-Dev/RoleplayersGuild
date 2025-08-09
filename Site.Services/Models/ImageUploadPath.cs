using System;
using System.IO;

namespace RoleplayersGuild.Site.Services.Models
{
    public class ImageUploadPath
    {
        public string? Path { get; }

        public ImageUploadPath(string? path)
        {
            Path = path;
        }

        public override string ToString() => Path ?? "";

        public static implicit operator string?(ImageUploadPath? imagePath) => imagePath?.Path;

        public static explicit operator ImageUploadPath(string? path) => new(path);
    }
}