using BlogService.Core.Interfaces;
using BlogService.Data;
using BlogService.Service.Interface;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogService.Service
{
    public class TokenService : ITokenService  
    {
        private readonly ITokenRepository _tokenRepository;

        public TokenService(ITokenRepository tokenRepository) 
        {
            _tokenRepository = tokenRepository;
        }
        public async Task<string> Login(string Email, string password)
        {
   
          var res = await _tokenRepository.Login(Email, password);
          return res;
        }

    }
}
