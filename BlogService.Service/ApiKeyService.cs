using BlogService.Core.Entities;
using BlogService.Core.Interfaces;
using BlogService.Service.Interface;
using System;
using System.Threading.Tasks;

namespace BlogService.Service
{
    public class ApiKeyService : IApiKeyService   // ✅ Make sure class name is ApiKeyService
    {
        private readonly IApiKeyRepository _apiKeyRepository;

        public ApiKeyService(IApiKeyRepository apiKeyRepository)
        {
            _apiKeyRepository = apiKeyRepository;
        }

        public async Task<string> GenerateApiKey(string name)
        {
            return await _apiKeyRepository.GenerateApiKey(name);
        }

        public async Task<bool> ValidateApiKey(string apiKey)
        {
            return await _apiKeyRepository.ValidateApiKey(apiKey);
        }

        public async Task<bool> DeleteApiKey(string apiKey)
        {
            return await _apiKeyRepository.DeleteApiKey(apiKey);
        }

        public async Task<IEnumerable<ApiKey>> GetAllApiKeys()
        {
            return await _apiKeyRepository.GetAllApiKeys();
        }

        // ✅ Keep only this:
        public async Task<string?> UpdateApiKey(Guid id, string? name, bool isActive)
        {
            return await _apiKeyRepository.UpdateApiKey(id, name, isActive);
        }
    }
}