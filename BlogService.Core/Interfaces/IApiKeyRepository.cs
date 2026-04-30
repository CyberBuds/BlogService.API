using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogService.Core.Interfaces
{
    public  interface IApiKeyRepository
    {
        Task<string> GenerateApiKey(string name);

        Task<bool> ValidateApiKey(string apiKey);

        Task<bool> DeleteApiKey(string apiKey);
    }
}
