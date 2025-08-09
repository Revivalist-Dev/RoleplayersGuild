using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RoleplayersGuild.Site.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _config;
        // Add a private field to hold the validated key
        private readonly string _jwtKey;

        public JwtService(IConfiguration config)
        {
            _config = config;
            // FIX: Validate the JWT key in the constructor.
            var key = _config["Jwt:Key"];
            if (string.IsNullOrEmpty(key))
            {
                throw new InvalidOperationException("JWT Key is not configured in appsettings.json or user secrets.");
            }
            _jwtKey = key;
        }

        public string GenerateToken(User user)
        {
            // Use the validated, non-null _jwtKey field here.
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username ?? user.EmailAddress ?? string.Empty),
                new Claim("UserTypeId", user.UserTypeId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(30),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}