using BlogService.Core.Entities;
using BlogService.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
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
        public async Task<IActionResult> Create([FromBody] User user)
        {
            await _unitOfWork.Repository<User>().AddAsync(user);
            await _unitOfWork.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
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

            _unitOfWork.Repository<User>().Delete(user);
            await _unitOfWork.SaveChangesAsync();

            // ✅ 200 OK instead of 204
            return Ok(new { message = "User deleted successfully.", deletedId = id });
        }

        [HttpPatch("{id}/role")]
        public async Task<IActionResult> UpdateRole(Guid id, [FromBody] string role)
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(id);
            if (user == null) return NotFound(new { message = "User not found." });

            user.Role = role;

            _unitOfWork.Repository<User>().Update(user);
            await _unitOfWork.SaveChangesAsync();

            // ✅ 200 OK instead of 204
            return Ok(new { message = "User role updated.", userId = id, role });
        }
    }
}
