using BlogService.Core.DTOs;
using BlogService.Core.Entities;
using BlogService.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BlogService.API.Controllers
{
    [ApiController]
    [Route("api/v1/admin/users")]
    [Authorize(Roles = "Admin,SuperAdmin")]  // ✅ Fixed — SuperAdmin added
    public class UserController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _unitOfWork.Repository<User>().GetAllAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserDto dto)  // ✅ dto parameter
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = dto.Username.Trim(),
                Email = dto.Email.Trim().ToLower(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = dto.Role ?? "User",
                TenantId = null
            };

            await _unitOfWork.Repository<User>().AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = user.Id }, new
            {
                userId = user.Id,
                username = user.Username,
                email = user.Email,
                role = user.Role
            });
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] User updateDto)
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(id);
            if (user == null) return NotFound(new { message = "User not found." });

            user.Username = updateDto.Username;
            user.Email = updateDto.Email;

            _unitOfWork.Repository<User>().Update(user);
            await _unitOfWork.SaveChangesAsync();

            // ✅ 200 OK instead of 204
            return Ok(user);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(id);
            if (user == null) return NotFound(new { message = "User not found." });


            user.IsDeleted = true; // ✅ Soft delete flag

            _unitOfWork.Repository<User>().Update(user); // ✅ Update NOT Delete
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { message = "User deleted successfully.", deletedId = id });
        }

        // ✅ UpdateRole — updates role only
        [HttpPatch("{id}/role")]
        public async Task<IActionResult> UpdateRole(Guid id, [FromBody] string role)
        {
            // ✅ IgnoreQueryFilters() — needed to find soft-deleted records
            // ✅ Get all users first, then filter in memory
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(id);
            if (user == null) return NotFound(new { message = "User not found." });

            user.Role = role; // ✅ Only role is updated

            _unitOfWork.Repository<User>().Update(user);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { message = "User role updated.", userId = id, role });
        }

        // ✅ Restore — brings back a soft-deleted user
        [HttpPatch("{id}/restore")]
        public async Task<IActionResult> Restore(Guid id)
        {
            var allUsers = await _unitOfWork.Repository<User>().GetAllAsync();

            var user = allUsers.FirstOrDefault(u => u.Id == id && u.IsDeleted);
            if (user == null) return NotFound(new { message = "Deleted user not found." });

            user.IsDeleted = false; // ✅ Restore flag

            _unitOfWork.Repository<User>().Update(user);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { message = "User restored successfully.", userId = id });
        }
    }
}
