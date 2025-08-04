// In F:\...\Site.Services\ReCaptchaResult.cs

using System.Collections.Generic;
using System.Linq;

namespace RoleplayersGuild.Site.Services
{
    public class ReCaptchaResult
    {
        public bool Success { get; }
        public double Score { get; }
        public string Action { get; }
        public string Hostname { get; }
        public IEnumerable<string> ErrorCodes { get; }

        public ReCaptchaResult(bool success, double score = 0, string action = "", string hostname = "", IEnumerable<string>? errorCodes = null)
        {
            Success = success;
            Score = score;
            Action = action;
            Hostname = hostname;
            ErrorCodes = errorCodes ?? Enumerable.Empty<string>();
        }

        /// <summary>
        /// A helper method to fully validate the result based on v3 best practices.
        /// </summary>
        /// <param name="expectedAction">The action name you sent from the frontend (e.g., "register").</param>
        /// <param name="expectedHostname">Your site's hostname (e.g., "www.roleplayersguild.com").</param>
        /// <param name="minScore">The minimum acceptable score, typically 0.5.</param>
        /// <returns>True if all checks pass, otherwise false.</returns>
        public bool IsSuccess(string expectedAction, string expectedHostname, double minScore = 0.5)
        {
            // Normalize both hostnames by removing "www." before comparing
            var actualHostname = Hostname.Replace("www.", "");
            var expected = expectedHostname.Replace("www.", "");

            return Success && Score >= minScore && Action == expectedAction && actualHostname == expected;
        }
    }
}