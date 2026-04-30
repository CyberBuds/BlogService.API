using BlogService.Core.Entities;
using BlogService.Core.Interfaces;
using BlogService.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BlogService.Repository 
{
    public class ApiKeyRepository : IApiKeyRepository 
    {
        private readonly BlogDbContext _context;

        public ApiKeyRepository(BlogDbContext context)
        {
            _context = context;
        }

        public async Task<string> GenerateApiKey(string name)
        {
            //Generate a random API key
            string apiKey = GenerateRandomKey();

            var newapiKey = new ApiKey
            {
                Id = Guid.NewGuid(),
                Key = apiKey,
                Name = name,
                IsActive = true,
                CreatedAt = DateTime.Now,
                ExpiresAt = null,
            };
            _context.ApiKeys.Add(newapiKey);
            await _context.SaveChangesAsync();

            return apiKey;
        }

        public async Task<bool> ValidateApiKey(string apiKey)
        {
            var key = _context.ApiKeys
                .FirstOrDefault(k => k.Key == apiKey && k.IsActive);

            if (key == null)
                return false;

            // Check if expired
            if (key.ExpiresAt.HasValue && key.ExpiresAt.Value < DateTime.UtcNow)
                return false;

            return true;
        }

        public async Task<bool> DeleteApiKey(string apiKey)
        {
            var key = _context.ApiKeys.FirstOrDefault(k => k.Key == apiKey);

            if (key == null)
                return false;

            _context.ApiKeys.Remove(key);
            await _context.SaveChangesAsync();

            return true;
        }

        private string GenerateRandomKey() 
        {
            // Generate a 32-byte random key and convert to Base64
            byte[] randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes)
                .Replace("/", "")
                .Replace("+", "")
                .Substring(0, 40);
        }
    }
}
