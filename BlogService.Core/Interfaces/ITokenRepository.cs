using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogService.Core.Interfaces
{
    public interface ITokenRepository
    {
        public Task<string> Login(string Email, string password);
       

    }
}
