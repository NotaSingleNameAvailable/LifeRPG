using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LifeRPG.API.Services
{
    public class JwtService
    {
        private readonly IConfiguration _config;

        public JwtService(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateToken(string userId, string username)
        {
            // Claims are pieces of data embedded inside the token
            // The client can't forge these because the token is signed
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),  // user's ID
                new Claim(ClaimTypes.Name, username)            // user's username
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

            var credentials = new SigningCredentials(
                key, SecurityAlgorithms.HmacSha256);

            var expiryDays = int.Parse(_config["Jwt:ExpiryDays"] ?? "7");

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(expiryDays),
                signingCredentials: credentials
            );

            // Returns a string like
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}