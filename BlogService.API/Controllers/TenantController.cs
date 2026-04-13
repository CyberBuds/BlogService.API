using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using BlogService.Core.Entities;
using BlogService.Core.Interfaces;

namespace BlogService.API.Controllers
{
    [ApiController]
    [Route("api/v1/admin/tenants")]
    [Authorize(Roles = "Admin")]
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
        public async Task<IActionResult> Create([FromBody] Tenant tenant)
        {
            await _unitOfWork.Repository<Tenant>().AddAsync(tenant);
            await _unitOfWork.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = tenant.Id }, tenant);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] Tenant updateDto)
        {
            var tenant = await _unitOfWork.Repository<Tenant>().GetByIdAsync(id);
            if (tenant == null) return NotFound();

            tenant.Name = updateDto.Name;
            tenant.Identifier = updateDto.Identifier;
            tenant.IsActive = updateDto.IsActive;

            _unitOfWork.Repository<Tenant>().Update(tenant);
            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var tenant = await _unitOfWork.Repository<Tenant>().GetByIdAsync(id);
            if (tenant == null) return NotFound();

            _unitOfWork.Repository<Tenant>().Delete(tenant);
            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }
    }
}
