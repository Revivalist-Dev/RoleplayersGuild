// In F:\Visual Studio\RoleplayersGuild\Site.Services\UserActivityMiddleware.cs

using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Services
{
    public class UserActivityMiddleware
    {
        private readonly RequestDelegate _next;

        public UserActivityMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IDataService dataService, IUserService userService)
        {
            var userId = userService.GetUserId(context.User);

            if (userId != 0 && !IsStaticFileRequest(context.Request.Path))
            {
                var now = DateTime.UtcNow;
                var lastUpdateString = context.Session.GetString("LastActivityUpdate");

                if (!DateTime.TryParse(lastUpdateString, out var lastUpdate) || (now - lastUpdate).TotalSeconds >= 60)
                {
                    await dataService.ExecuteAsync(
                        """UPDATE "Users" SET "LastAction" = NOW() AT TIME ZONE 'UTC' WHERE "UserId" = @UserId""",
                        new { UserId = userId });

                    context.Session.SetString("LastActivityUpdate", now.ToString("o"));
                }
            }

            // The character creation check is removed from here.
            await _next(context);
        }

        private static bool IsStaticFileRequest(PathString path)
        {
            var extension = Path.GetExtension(path.Value)?.ToLowerInvariant();
            return extension switch
            {
                ".css" or ".js" or ".png" or ".jpg" or ".jpeg" or ".gif" or ".ico" or ".svg" or ".woff" or ".woff2" => true,
                _ => false,
            };
        }
    }
}