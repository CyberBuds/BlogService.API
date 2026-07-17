using BlogService.Core.DTOs;
using BlogService.Core.Entities;
using BlogService.Core.Interfaces;   // ← ICommentService lives here ✅
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BlogService.API.Controllers
{
    [ApiController]
    [Route("api/v1/comments")]
    public class CommentController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public CommentController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet("blog/{blogId}")]
        public async Task<IActionResult> GetByBlog(Guid blogId)
        {
            var comments = await _unitOfWork.Repository<Comment>().GetAllAsync();
            var approved = comments.Where(c => c.BlogId == blogId && c.IsApproved);
            return Ok(approved.Select(c => new CommentDto { Id = c.Id, BlogId = c.BlogId, AuthorName = c.AuthorName, Content = c.Content, IsApproved = c.IsApproved }));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCommentDto dto)
        {
            // ✅ Get TenantId from request header
            var tenantId = Request.Headers["TenantId"].ToString().Trim();

            if (string.IsNullOrEmpty(tenantId))
                return BadRequest("TenantId header is required.");

            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                BlogId = dto.BlogId,
                AuthorName = dto.AuthorName,
                AuthorEmail = dto.AuthorEmail,
                Content = dto.Content,
                IsApproved = false, // Requires moderation
                CreatedAt = DateTime.UtcNow,   // ✅ Add this
                TenantId = tenantId            // ✅ Add this 
            };
            await _unitOfWork.Repository<Comment>().AddAsync(comment);
            await _unitOfWork.SaveChangesAsync();
            return Ok("Comment submitted for moderation.");
        }

        [HttpPut("{id}")]
        [ApiExplorerSettings(GroupName = "admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CreateCommentDto dto)
        {
            var comment = await _unitOfWork.Repository<Comment>().GetByIdAsync(id);
            if (comment == null) return NotFound(new { message = "Comment not found." });

            comment.AuthorName = dto.AuthorName;
            comment.AuthorEmail = dto.AuthorEmail;
            comment.Content = dto.Content;

            _unitOfWork.Repository<Comment>().Update(comment);
            await _unitOfWork.SaveChangesAsync();

            // ✅ 200 OK with updated data instead of 204
            return Ok(new CommentDto
            {
                Id = comment.Id,
                BlogId = comment.BlogId,
                AuthorName = comment.AuthorName,
                Content = comment.Content,
                IsApproved = comment.IsApproved
            });
        }

        [HttpDelete("{id}")]
        [ApiExplorerSettings(GroupName = "admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var comment = await _unitOfWork.Repository<Comment>().GetByIdAsync(id);
            if (comment == null) return NotFound(new { message = "Comment not found." });

            // ✅ Soft delete — same as User
            comment.IsDeleted = true;
            _unitOfWork.Repository<Comment>().Delete(comment);
            await _unitOfWork.SaveChangesAsync();

            // ✅ 200 OK instead of 204
            return Ok(new { message = "Comment deleted successfully.", deletedId = id });
        }
    }

    [ApiController]
    [Route("api/v1/admin/comments")]
    [Authorize(Roles = "Admin,Editor,SuperAdmin,admin,superadmin,editor,user,User")]
    [ApiExplorerSettings(GroupName = "admin")]   // <-- added here
    public class AdminCommentController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommentService _commentService;          // 👈 ADD this line


        public AdminCommentController(IUnitOfWork unitOfWork, ICommentService commentService)
        {
            _unitOfWork = unitOfWork;
            _commentService = commentService;
        }

        // 👈 ADD this entire GET method
        [HttpGet]
        public async Task<IActionResult> GetComments(
            [FromQuery] Guid blogId,
            [FromQuery] string tenantId,
            [FromQuery] bool? isApproved = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            if (blogId == Guid.Empty)
                return BadRequest(new { message = "blogId is required." });

            if (string.IsNullOrWhiteSpace(tenantId))
                return BadRequest(new { message = "tenantId is required." });

            var result = await _commentService.GetCommentsByBlogAsync(
                new GetCommentsByBlogQueryDto
                {
                    BlogId = blogId,
                    TenantId = tenantId,
                    IsApproved = isApproved,
                    Page = page,
                    PageSize = pageSize
                }, cancellationToken);

            return Ok(result);
        }

        [HttpPatch("{id}/approve")]
        public async Task<IActionResult> Approve(Guid id)
        {
            var comment = await _unitOfWork.Repository<Comment>().GetByIdAsync(id);
            if (comment == null) return NotFound(new { message = "Comment not found." });

            comment.IsApproved = true;
            _unitOfWork.Repository<Comment>().Update(comment);
            await _unitOfWork.SaveChangesAsync();

            // ✅ 200 OK instead of 204
            return Ok(new { message = "Comment approved.", commentId = id, isApproved = true });
        }


        [HttpPatch("{id}/reject")]
        public async Task<IActionResult> Reject(Guid id)
        {
            var comment = await _unitOfWork.Repository<Comment>().GetByIdAsync(id);
            if (comment == null) return NotFound(new { message = "Comment not found." });

            comment.IsApproved = false;
            _unitOfWork.Repository<Comment>().Update(comment);
            await _unitOfWork.SaveChangesAsync();

            // ✅ 200 OK instead of 204
            return Ok(new { message = "Comment rejected.", commentId = id, isApproved = false });
        }
    }
}
