using System;

namespace RoleplayersGuild.Site.Utils
{
    public static class DateUtils
    {
        public static string TimeAgo(DateTime? date)
        {
            if (!date.HasValue)
                return "never";

            var timeSpan = DateTime.UtcNow - date.Value;

            if (timeSpan.TotalSeconds < 10) return "just now";
            if (timeSpan.TotalMinutes < 1) return $"{(int)timeSpan.TotalSeconds} seconds ago";
            if (timeSpan.TotalMinutes < 2) return "a minute ago";
            if (timeSpan.TotalHours < 1) return $"{(int)timeSpan.TotalMinutes} minutes ago";
            if (timeSpan.TotalHours < 2) return "an hour ago";
            if (timeSpan.TotalDays < 1) return $"{(int)timeSpan.TotalHours} hours ago";
            if (timeSpan.TotalDays < 2) return "yesterday";
            if (timeSpan.TotalDays < 30) return $"{(int)timeSpan.TotalDays} days ago";
            if (timeSpan.TotalDays < 60) return "about a month ago";
            if (timeSpan.TotalDays < 365) return $"{(int)timeSpan.TotalDays / 30} months ago";
            if (timeSpan.TotalDays < 730) return "about a year ago";

            return $"{(int)timeSpan.TotalDays / 365} years ago";
        }

        public static bool IsOnline(DateTime? lastAction)
        {
            // A user is considered "online" if their last action was within the last 15 minutes.
            return lastAction.HasValue && lastAction.Value > DateTime.UtcNow.AddMinutes(-15);
        }
    }
}