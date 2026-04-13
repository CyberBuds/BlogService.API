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
            var comment = new Comment
            {
                BlogId = dto.BlogId,
                AuthorName = dto.AuthorName,
                AuthorEmail = dto.AuthorEmail,
                Content = dto.Content,
                IsApproved = false // Requires moderation
            };
            await _unitOfWork.Repository<Comment>().AddAsync(comment);
            await _unitOfWork.SaveChangesAsync();
            return Ok("Comment submitted for moderation.");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CreateCommentDto dto)
        {
            await Task.Delay(10);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await Task.Delay(10);
            return NoContent();
        }
    }

    [ApiController]
    [Route("api/v1/admin/comments")]
    [Authorize(Roles = "Admin,Editor")]
    public class AdminCommentController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public AdminCommentController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpPatch("{id}/approve")]
        public async Task<IActionResult> Approve(Guid id)
        {
            var comment = await _unitOfWork.Repository<Comment>().GetByIdAsync(id);
            if (comment == null) return NotFound();
            
            comment.IsApproved = true;
            _unitOfWork.Repository<Comment>().Update(comment);
            await _unitOfWork.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("{id}/reject")]
        public async Task<IActionResult> Reject(Guid id)
        {
            var comment = await _unitOfWork.Repository<Comment>().GetByIdAsync(id);
            if (comment == null) return NotFound();
            
            comment.IsApproved = false;
            _unitOfWork.Repository<Comment>().Update(comment);
            await _unitOfWork.SaveChangesAsync();
            return NoContent();
        }
    }
}
