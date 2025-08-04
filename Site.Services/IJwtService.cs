using RoleplayersGuild.Site.Model;
using System.Security.Claims;
using System.Collections.Generic;

namespace RoleplayersGuild.Site.Services
{
    public interface IJwtService
    {
        string GenerateToken(User user);
    }
}