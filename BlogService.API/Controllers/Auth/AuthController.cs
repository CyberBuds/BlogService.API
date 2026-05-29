using BlogService.Core.DTOs;
using BlogService.Core.Entities;
using BlogService.Data;
using BlogService.Service.Interface;
using Humanizer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;


namespace BlogService.API.Controllers.Auth
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly BlogDbContext _context;
        public AuthController(ITokenService tokenService, BlogDbContext context)
        {
            _tokenService = tokenService;
            _context = context;
        }

        // 1. POST /api/v1/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login(string Email, string password)
        {

            var res = await _tokenService.Login(Email, password);
            return Ok(res);
            
           
        }


        // 2. POST /api/v1/auth/register — FULLY IMPLEMENTED
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            // Step 1: Validate model (Required, EmailAddress, MinLength attributes)
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Step 2: Check duplicate email (case-insensitive)
            var emailExists = await _context.Users
                .AnyAsync(u => u.Email != null &&
                               u.Email.ToLower() == dto.Email.ToLower().Trim());

            if (emailExists)
                return Conflict(new { message = "This email is already registered." });

            // Step 3: Check duplicate username (case-insensitive)
            var usernameExists = await _context.Users
                .AnyAsync(u => u.Username != null &&
                               u.Username.ToLower() == dto.Username.ToLower().Trim());

            if (usernameExists)
                return Conflict(new { message = "This username is already taken." });

            // Step 4: Build User — match your exact nullable entity properties
            // ✅ TenantId is Guid? on User — left as null for public registration
            // ✅ CreatedAt/UpdatedAt are set automatically by DbContext SaveChanges
            // ✅ Role = "User" for public registrations (overrides "Viewer" default)
            var user = new User
            {
                Id = Guid.NewGuid(),          // User re-declares Id, so set explicitly
                Username = dto.Username.Trim(),
                Email = dto.Email.Trim().ToLower(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),  // ✅ Hashed,     // ⚠️ Plain text — swap BCrypt later
                Role = "User",
                TenantId = null                    // Public user — no tenant scope
            };

            // Step 5: Save — DbContext ApplyAuditAndTenantInformation sets CreatedAt automatically
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Step 6: Return 201 — never expose PasswordHash in response
            return StatusCode(201, new
            {
                message = "Registration successful.",
                userId = user.Id,
                username = user.Username,
                email = user.Email,
                role = user.Role
            });
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
