namespace RoleplayersGuild.Site.Services
{
    public interface IJwtService
    {
        string GenerateToken(User user);
    }
}