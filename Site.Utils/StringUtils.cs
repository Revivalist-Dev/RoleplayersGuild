using System;
using System.Security.Cryptography;
using System.Text;

namespace RoleplayersGuild.Site.Utils
{
    /// <summary>
    /// A static utility class for common string manipulation and formatting tasks.
    /// </summary>
    public static class StringUtils
    {
        // The DisplayImageString method has been removed.

        /// <summary>
        /// Generates a cryptographically secure random string of a specified length.
        /// </summary>
        /// <param name="length">The desired length of the string.</param>
        /// <param name="allowedChars">The set of characters to use for generating the string.</param>
        /// <returns>A random string.</returns>
        public static string GenerateRandomString(int length, string allowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890")
        {
            var result = new StringBuilder(length);
            var allowedCharsLength = allowedChars.Length;

            for (int i = 0; i < length; i++)
            {
                // Use the cryptographically secure random number generator for unpredictability.
                int randomIndex = RandomNumberGenerator.GetInt32(allowedCharsLength);
                result.Append(allowedChars[randomIndex]);
            }

            return result.ToString();
        }

        /// <summary>
        /// Converts a DateTime into a friendly "time ago" format.
        /// </summary>
        /// <param name="dateTime">The DateTime to format. Should be in UTC for accurate comparison.</param>
        /// <returns>A string like "5 minutes ago" or "Just now".</returns>
    }
}