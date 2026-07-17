using BlogService.Core.DTOs;
using BlogService.Core.Entities;
using BlogService.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BlogService.API.Controllers
{
    [ApiController]
    [Route("api/v1/admin/tenants")]
    [Authorize(Roles = "Admin,SuperAdmin,superadmin,admin,User,user")]
    [ApiExplorerSettings(GroupName = "admin")]   // <-- added here
    public class TenantController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public TenantController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }



        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var repo = _unitOfWork.Repository<Tenant>();

            // 2. ✅ Accept both PascalCase and lowercase versions of the superadmin claim
            if (User.IsInRole("SuperAdmin") || User.IsInRole("superadmin"))
            {
                var allTenants = await repo.GetAllAsync();
                return Ok(allTenants);
            }
            // 🔒 Tenant Admins flow
            var tenants = await repo.GetAllAsync();
            return Ok(tenants);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var tenant = await _unitOfWork.Repository<Tenant>().GetByIdAsync(id);
            if (tenant == null) return NotFound();
            return Ok(tenant);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTenantDto tenantDto) // 👈 Changed from Tenant to CreateTenantDto
        {
            // Map DTO fields to the Tenant domain entity
            var tenant = new Tenant
            {
                Name = tenantDto.Name,
                Identifier = tenantDto.Identifier,
                IsActive = tenantDto.IsActive
                // Id, CreatedAt, UpdatedAt, and IsDeleted are managed automatically by BaseEntity/Database
            };

            await _unitOfWork.Repository<Tenant>().AddAsync(tenant);

            // ✅ Link this tenant to the user who created it. Without this, there is
            // no server-side way to know "which tenant does this logged-in user
            // belong to" — the frontend was left guessing via localStorage, which
            // breaks on any other browser/device/session.
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out var userId))
            {
                var currentUser = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
                if (currentUser != null)
                {
                    currentUser.TenantId = tenant.Id.ToString();
                }
            }

            await _unitOfWork.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = tenant.Id }, tenant);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTenantDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var tenant = await _unitOfWork.Repository<Tenant>().GetByIdAsync(id);
            if (tenant == null) return NotFound(new { message = "Tenant not found." });

            tenant.Name = dto.Name.Trim();
            tenant.Identifier = dto.Identifier.Trim().ToLower();
            tenant.IsActive = dto.IsActive;
            tenant.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<Tenant>().Update(tenant);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new
            {
                tenantId = tenant.Id,
                name = tenant.Name,
                identifier = tenant.Identifier,
                isActive = tenant.IsActive,
                updatedAt = tenant.UpdatedAt
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var tenant = await _unitOfWork.Repository<Tenant>().GetByIdAsync(id);
            if (tenant == null) return NotFound(new { message = "Tenant not found." });

            // ✅ Soft delete — same as User
            tenant.IsDeleted = true;
            _unitOfWork.Repository<Tenant>().Delete(tenant);
            await _unitOfWork.SaveChangesAsync();

            // ✅ 200 OK instead of 204
            return Ok(new { message = "Tenant deleted successfully.", deletedId = id });
        }
    }
}
