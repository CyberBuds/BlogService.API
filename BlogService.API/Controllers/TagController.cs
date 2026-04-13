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
    public class TagsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public TagsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tags = await _unitOfWork.Repository<Tag>().GetAllAsync();
            var dtos = tags.Select(t => new TagDto { Id = t.Id, Name = t.Name, Slug = t.Slug });
            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var tag = await _unitOfWork.Repository<Tag>().GetByIdAsync(id);
            if (tag == null) return NotFound();
            
            return Ok(new TagDto { Id = tag.Id, Name = tag.Name, Slug = tag.Slug });
        }
    }

    [ApiController]
    [Route("api/v1/admin/tags")]
    [Authorize(Roles = "Admin,Editor")]
    public class AdminTagsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public AdminTagsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTagDto dto)
        {
            var tag = new Tag
            {
                Name = dto.Name,
                Slug = dto.Name.ToLower().Replace(" ", "-") // Basic slugification
            };

            await _unitOfWork.Repository<Tag>().AddAsync(tag);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new TagDto { Id = tag.Id, Name = tag.Name, Slug = tag.Slug });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTagDto dto)
        {
            var tag = await _unitOfWork.Repository<Tag>().GetByIdAsync(id);
            if (tag == null) return NotFound();

            tag.Name = dto.Name;
            tag.Slug = dto.Name.ToLower().Replace(" ", "-");

            _unitOfWork.Repository<Tag>().Update(tag);
            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var tag = await _unitOfWork.Repository<Tag>().GetByIdAsync(id);
            if (tag == null) return NotFound();

            _unitOfWork.Repository<Tag>().Delete(tag);
            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }
    }
}
