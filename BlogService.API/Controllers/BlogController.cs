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
    [Authorize(Roles = "Admin,SuperAdmin,admin,superadmin")]
    public class AdminBlogsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public AdminBlogsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] BlogQueryDto query)
        {
            var blogs = await _unitOfWork.Repository<Blog>().GetAllAsync();

            // Filtering
            if (!string.IsNullOrWhiteSpace(query.Search))
                blogs = blogs.Where(b => b.Title.Contains(query.Search, StringComparison.OrdinalIgnoreCase));

            if (query.IsPublished.HasValue)
                blogs = blogs.Where(b => b.IsPublished == query.IsPublished.Value);

            // Pagination
            var totalCount = blogs.Count();
            var pagedBlogs = blogs
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(b => new BlogDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    Slug = b.Slug,
                    IsPublished = b.IsPublished,
                    Content = b.Content,
                   
                })
                .ToList();

            return Ok(new
            {
                totalCount,
                page = query.Page,
                pageSize = query.PageSize,
                totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize),
                data = pagedBlogs
            });
        }



        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBlogDto dto)
        {
            // Fix 1: Trim TenantId to strip accidental whitespace from header
            var tenantId = Request.Headers["TenantId"].FirstOrDefault()?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(tenantId))
                return BadRequest(new { message = "TenantId header is required." });

            // ✅ THIS GUARD — prevents the FK 500 with a readable 400 message
            var author = await _unitOfWork.Repository<User>().GetByIdAsync(dto.AuthorId);
            if (author == null)
                return BadRequest(new
                {
                    message = $"AuthorId '{dto.AuthorId}' does not exist in Users table.",
                    hint = "Use a real User Id. Valid Ids: dbb9fc0b-... or fbb3ae66-..."
                });

            // Fix 3: Prevent duplicate slug constraint violation
            var slug = dto.Title.ToLower().Replace(" ", "-").Replace(".", "");
            var existingBlogs = await _unitOfWork.Repository<Blog>().GetAllAsync();
            if (existingBlogs.Any(b => b.Slug == slug && b.TenantId == tenantId))
                slug = $"{slug}-{DateTime.UtcNow.Ticks}"; // make it unique

            var blog = new Blog
            {
                Title = dto.Title,
                Slug = slug,
                Content = dto.Content,
                AuthorId = dto.AuthorId,
                TenantId = tenantId,
                IsPublished = false
            };

            if (dto.CategoryIds?.Any() == true)
            {
                var allCategories = await _unitOfWork.Repository<Category>().GetAllAsync();
                foreach (var categoryId in dto.CategoryIds)
                {
                    var category = allCategories.FirstOrDefault(c => c.Id == categoryId);
                    if (category != null)
                        blog.Categories.Add(category);
                }
            }

            if (dto.TagIds?.Any() == true)
            {
                var allTags = await _unitOfWork.Repository<Tag>().GetAllAsync();
                foreach (var tagId in dto.TagIds)
                {
                    var tag = allTags.FirstOrDefault(t => t.Id == tagId);
                    if (tag != null)
                        blog.Tags.Add(tag);
                }
            }
            try
            {
                await _unitOfWork.Repository<Blog>().AddAsync(blog);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            {
                return StatusCode(500, new
                {
                    error = "Database save failed",
                    detail = ex.InnerException?.Message ?? ex.Message
                });
            }

            return Ok(new BlogDto
            {
                Id = blog.Id,
                Title = blog.Title,
                Slug = blog.Slug,
                AuthorId = blog.AuthorId,
                IsPublished = false,
                CreatedAt = blog.CreatedAt
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBlogDto dto)
        {

            // ✅ Read and validate TenantId header (same pattern as Create)
            var tenantId = Request.Headers["TenantId"].FirstOrDefault()?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(tenantId))
                return BadRequest(new { message = "TenantId header is required." });
         
            // ✅ Query by BOTH id AND tenantId — fixes the 404
            var blogs = await _unitOfWork.Repository<Blog>().GetAllAsync();
            var blog = blogs.FirstOrDefault(b => b.Id == id && b.TenantId == tenantId);

            if (blog == null) 
            return NotFound(new { message = $"Blog '{id}' not found for tenant '{tenantId}'." });

            blog.Title = dto.Title;
            blog.Content = dto.Content;
            blog.Slug = dto.Title.ToLower().Replace(" ", "-").Replace(".", "");

            _unitOfWork.Repository<Blog>().Update(blog);
            await _unitOfWork.SaveChangesAsync();

            // ✅ Return 200 OK with updated blog data instead of 204 No Content
            return Ok(new BlogDto
            {
                Id = blog.Id,
                Title = blog.Title,
                Slug = blog.Slug,
                Content = blog.Content,
                AuthorId = blog.AuthorId,
                IsPublished = blog.IsPublished,
                CreatedAt = blog.CreatedAt
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var tenantId = Request.Headers["TenantId"].FirstOrDefault()?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(tenantId))
                return BadRequest(new { message = "TenantId header is required." });

            var blogs = await _unitOfWork.Repository<Blog>().GetAllAsync();
            var blog = blogs.FirstOrDefault(b => b.Id == id && b.TenantId == tenantId);
            if (blog == null)
                return NotFound(new { message = $"Blog '{id}' not found for tenant '{tenantId}'." });

            // ✅ Soft delete — same as User
            blog.IsDeleted = true;
            _unitOfWork.Repository<Blog>().Delete(blog);
            await _unitOfWork.SaveChangesAsync();

            // ✅ 200 OK with deleted blog info
            return Ok(new
            {
                message = "Blog deleted successfully.",
                deletedBlogId = blog.Id,
                title = blog.Title
            });
        }

        [HttpPatch("{id}/publish")]
        public async Task<IActionResult> Publish(Guid id)
        {
            var blogs = await _unitOfWork.Repository<Blog>().GetAllAsync();
            var blog = blogs.FirstOrDefault(b => b.Id == id);
            if (blog == null) return NotFound(new { message = $"Blog '{id}' not found." });

            blog.IsPublished = true;
            _unitOfWork.Repository<Blog>().Update(blog);
            await _unitOfWork.SaveChangesAsync();

            // ✅ 200 OK with updated blog data
            return Ok(new BlogDto
            {
                Id = blog.Id,
                Title = blog.Title,
                Slug = blog.Slug,
                Content = blog.Content,
                AuthorId = blog.AuthorId,
                IsPublished = blog.IsPublished,
                CreatedAt = blog.CreatedAt
            });
        }

        [HttpPatch("{id}/unpublish")]
        public async Task<IActionResult> Unpublish(Guid id)
        {
            var blogs = await _unitOfWork.Repository<Blog>().GetAllAsync();
            var blog = blogs.FirstOrDefault(b => b.Id == id);
            if (blog == null) return NotFound(new { message = $"Blog '{id}' not found." });

            blog.IsPublished = false;
            _unitOfWork.Repository<Blog>().Update(blog);
            await _unitOfWork.SaveChangesAsync();

            // ✅ 200 OK with updated blog data (was NoContent)
            return Ok(new BlogDto
            {
                Id = blog.Id,
                Title = blog.Title,
                Slug = blog.Slug,
                Content = blog.Content,
                AuthorId = blog.AuthorId,
                IsPublished = blog.IsPublished,
                CreatedAt = blog.CreatedAt
            });
        }
    }
}
