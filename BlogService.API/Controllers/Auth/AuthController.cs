using BlogService.Service.Interface;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace BlogService.API.Controllers.Auth
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        public AuthController(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        // 1. POST /api/v1/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login(string username, string password)
        {

            var res = await _tokenService.Login(username, password);
            return Ok(res);
            
           
        }

      

        // 2. POST /api/v1/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register()
        {
            await Task.Delay(10);
            return Ok("Registered!");
        }

        // 3. POST /api/v1/auth/refresh-token
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            await Task.Delay(10);
            return Ok(new { Token = "dummy-new-jwt-token" });
        }

        // 4. POST /api/v1/auth/logout
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await Task.Delay(10);
            return Ok("Logged out");
        }

        // 5. POST /api/v1/auth/forgot-password
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword()
        {
            await Task.Delay(10);
            return Ok("Email sent");
        }

        // 6. POST /api/v1/auth/reset-password
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword()
        {
            await Task.Delay(10);
            return Ok("Password reset");
        }
    }
}
