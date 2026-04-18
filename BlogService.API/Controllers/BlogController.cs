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
    public class BlogsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public BlogsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() 
        {
            var blogs = await _unitOfWork.Repository<Blog>().GetAllAsync();  
            var dtos = blogs.Where(b => b.IsPublished).Select(b => new BlogDto { 
                Id = b.Id, Title = b.Title, Slug = b.Slug, Content = b.Content, AuthorId = b.AuthorId, IsPublished = b.IsPublished, CreatedAt = b.CreatedAt 
            });
            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var blog = await _unitOfWork.Repository<Blog>().GetByIdAsync(id);
            if (blog == null) return NotFound();
            
            return Ok(new BlogDto { Id = blog.Id, Title = blog.Title, Slug = blog.Slug, Content = blog.Content, AuthorId = blog.AuthorId, IsPublished = blog.IsPublished, CreatedAt = blog.CreatedAt });
        }

        [HttpGet("slug/{slug}")]
        public async Task<IActionResult> GetBySlug(string slug)
        {
            var blogs = await _unitOfWork.Repository<Blog>().GetAllAsync();
            var blog = blogs.FirstOrDefault(b => b.Slug == slug);
            if (blog == null) return NotFound();
            
            return Ok(new BlogDto { Id = blog.Id, Title = blog.Title, Slug = blog.Slug, Content = blog.Content, AuthorId = blog.AuthorId, IsPublished = blog.IsPublished, CreatedAt = blog.CreatedAt });
        }

        [HttpGet("tenant/{tenantId}")]
        public async Task<IActionResult> GetByTenant(string tenantId)
        {
            // Usually tenant filter is automatic. We demonstrate custom logic if needed.
            var blogs = await _unitOfWork.Repository<Blog>().GetAllAsync();
            var filtered = blogs.Where(b => b.TenantId == tenantId && b.IsPublished);
            return Ok(filtered);
        }

        [HttpGet("author/{authorId}")]
        public async Task<IActionResult> GetByAuthor(Guid authorId)
        {
            var blogs = await _unitOfWork.Repository<Blog>().GetAllAsync();
            var filtered = blogs.Where(b => b.AuthorId == authorId && b.IsPublished);
            return Ok(filtered);
        }

        [HttpGet("popular")]
        public async Task<IActionResult> GetPopular()
        {
            // Will integrate Redis here later. Returning recent 5 for now.
            var blogs = await _unitOfWork.Repository<Blog>().GetAllAsync();
            return Ok(blogs.Where(b => b.IsPublished).OrderByDescending(b => b.CreatedAt).Take(5));
        }

        [HttpGet("recent")]
        public async Task<IActionResult> GetRecent()
        {
            var blogs = await _unitOfWork.Repository<Blog>().GetAllAsync();
            return Ok(blogs.Where(b => b.IsPublished).OrderByDescending(b => b.CreatedAt).Take(10));
        }
    }

    [ApiController]
    [Route("api/v1/admin/blogs")]
    [Authorize(Roles = "Admin,Editor")]
    public class AdminBlogsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public AdminBlogsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBlogDto dto)
        {
            var blog = new Blog
            {
                Title = dto.Title,
                Slug = dto.Title.ToLower().Replace(" ", "-").Replace(".", ""),
                Content = dto.Content,
                AuthorId = dto.AuthorId,
                IsPublished = false
            };

            await _unitOfWork.Repository<Blog>().AddAsync(blog);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new BlogDto { Id = blog.Id, Title = blog.Title, Slug = blog.Slug, IsPublished = false });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBlogDto dto)
        {
            var blog = await _unitOfWork.Repository<Blog>().GetByIdAsync(id);
            if (blog == null) return NotFound();

            blog.Title = dto.Title;
            blog.Content = dto.Content;
            blog.Slug = dto.Title.ToLower().Replace(" ", "-").Replace(".", "");

            _unitOfWork.Repository<Blog>().Update(blog);
            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var blog = await _unitOfWork.Repository<Blog>().GetByIdAsync(id);
            if (blog == null) return NotFound();

            _unitOfWork.Repository<Blog>().Delete(blog);
            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }

        [HttpPatch("{id}/publish")]
        public async Task<IActionResult> Publish(Guid id)
        {
            var blog = await _unitOfWork.Repository<Blog>().GetByIdAsync(id);
            if (blog == null) return NotFound();

            blog.IsPublished = true;
            _unitOfWork.Repository<Blog>().Update(blog);
            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }

        [HttpPatch("{id}/unpublish")]
        public async Task<IActionResult> Unpublish(Guid id)
        {
            var blog = await _unitOfWork.Repository<Blog>().GetByIdAsync(id);
            if (blog == null) return NotFound();

            blog.IsPublished = false;
            _unitOfWork.Repository<Blog>().Update(blog);
            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }
    }
}
