using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogService.Service.Interface
{
    public interface ITokenService
    {
        public Task<string> Login(string username, string password);
    }
}
