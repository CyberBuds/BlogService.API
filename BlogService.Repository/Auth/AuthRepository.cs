using BlogService.Core.Entities;
using BlogService.Core.Interfaces;
using BlogService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text; 

namespace BlogService.Repository.Auth
{
    public class AuthRepository : ITokenRepository
    {
        private readonly BlogDbContext _context;
        private readonly IConfiguration _configuration;
        public AuthRepository(BlogDbContext context, IConfiguration configuration) 
        {
            _context = context;
            _configuration = configuration;
        }
        public async Task<string> Login(string username, string password)
        {
            var query = _context.Users
             
                .FirstOrDefault(u => u.Username == username);

            

            if (!BCrypt.Net.BCrypt.Verify(password, query.PasswordHash))
                return null;

            return GenerateJwtToken(query.Username);
        }



        public string GenerateJwtToken(string username)
        {
            var claims = new[]
            {
                    new Claim(ClaimTypes.Name, username)
                };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"])  // Read from config
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],      // Read from config
                audience: _configuration["JwtSettings:Audience"],  // Read from config
                claims: claims,
                expires: DateTime.Now.AddMinutes(60),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
      

    }
}