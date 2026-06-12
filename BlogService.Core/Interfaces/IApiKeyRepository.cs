using BlogService.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogService.Core.Interfaces
{
    public  interface IApiKeyRepository
    {
        Task<IEnumerable<ApiKey>> GetAllApiKeys();
        Task<string?> UpdateApiKey(Guid id, string? name, bool isActive); // ✅ string? not bool
        Task<string> GenerateApiKey(string name);
        Task<bool> ValidateApiKey(string apiKey);
        Task<bool> DeleteApiKey(string apiKey);
    }
}
