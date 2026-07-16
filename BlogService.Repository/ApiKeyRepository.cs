using BlogService.Core.Entities;
using BlogService.Core.Interfaces;
using BlogService.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace BlogService.Repository
{
    public class ApiKeyRepository : IApiKeyRepository
    {
        private readonly BlogDbContext _context;
        private readonly ITenantService _tenantService;

        public ApiKeyRepository(BlogDbContext context, ITenantService tenantService)
        {
            _context = context;
            _tenantService = tenantService;
        }

        // ✅ Keep only this:
        public async Task<string?> UpdateApiKey(Guid id, string? name, bool isActive)
        {
            var key = await _context.ApiKeys.FirstOrDefaultAsync(k => k.Id == id);
            if (key == null) return null;

            if (!string.IsNullOrEmpty(name))
                key.Name = name;

            key.IsActive = isActive;
            key.Key = GenerateRandomKey(); // ✅ Regenerate new key
            key.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return key.Key; // ✅ Return new key
        }

        public async Task<string> GenerateApiKey(string name)
        {
            string apiKey = GenerateRandomKey();
            var tenantId = _tenantService.GetTenantId();
            Console.WriteLine($"TenantService Value = {tenantId}");

            var newApiKey = new ApiKey
            {
                Id = Guid.NewGuid(),
                Key = apiKey,
                Name = name.Trim(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ExpiresAt = null,
                TenantId = tenantId
            };

            try
            {
                _context.ApiKeys.Add(newApiKey);
                await _context.SaveChangesAsync();
                return apiKey;
            }
            catch (Exception ex)
            {
                // ✅ Shows exact error in Visual Studio Output window
                Console.WriteLine("=== REAL ERROR ===");
                Console.WriteLine($"Message:           {ex.Message}");
                Console.WriteLine($"Inner:             {ex.InnerException?.Message}");
                Console.WriteLine($"Inner Inner:       {ex.InnerException?.InnerException?.Message}");
                Console.WriteLine($"TenantId was:      {tenantId}");
                Console.WriteLine($"ApiKey Name was:   {name}");
                Console.WriteLine("==================");
                throw;
            }
        }

        public async Task<bool> ValidateApiKey(string apiKey)
        {
            var key = await _context.ApiKeys
                .FirstOrDefaultAsync(k => k.Key == apiKey && k.IsActive);

            if (key == null)
                return false;

            // Check if expired
            if (key.ExpiresAt.HasValue && key.ExpiresAt.Value < DateTime.UtcNow)
                return false;

            return true;
        }

        public async Task<bool> DeleteApiKey(string apiKey)
        {
            var key = await _context.ApiKeys
                .FirstOrDefaultAsync(k => k.Key == apiKey);

            if (key == null)
                return false;

            _context.ApiKeys.Remove(key);
            await _context.SaveChangesAsync();

            return true;
        }

        private string GenerateRandomKey()
        {
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

        public async Task<IEnumerable<ApiKey>> GetAllApiKeys()
        {
            return await _context.ApiKeys.ToListAsync();
            Console.WriteLine($"Current Tenant = {_tenantService.GetTenantId()}");
        }
    }
}