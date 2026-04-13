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
    [Route("api/v1/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoriesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _unitOfWork.Repository<Category>().GetAllAsync();
            var dtos = categories.Select(c => new CategoryDto { Id = c.Id, Name = c.Name, Slug = c.Slug });
            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var category = await _unitOfWork.Repository<Category>().GetByIdAsync(id);
            if (category == null) return NotFound();
            
            return Ok(new CategoryDto { Id = category.Id, Name = category.Name, Slug = category.Slug });
        }
    }

    [ApiController]
    [Route("api/v1/admin/categories")]
    [Authorize(Roles = "Admin,Editor")]
    public class AdminCategoriesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public AdminCategoriesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
        {
            var category = new Category
            {
                Name = dto.Name,
                Slug = dto.Name.ToLower().Replace(" ", "-") // Basic slugification
            };

            await _unitOfWork.Repository<Category>().AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new CategoryDto { Id = category.Id, Name = category.Name, Slug = category.Slug });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryDto dto)
        {
            var category = await _unitOfWork.Repository<Category>().GetByIdAsync(id);
            if (category == null) return NotFound();

            category.Name = dto.Name;
            category.Slug = dto.Name.ToLower().Replace(" ", "-");

            _unitOfWork.Repository<Category>().Update(category);
            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var category = await _unitOfWork.Repository<Category>().GetByIdAsync(id);
            if (category == null) return NotFound();

            _unitOfWork.Repository<Category>().Delete(category);
            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }
    }
}
