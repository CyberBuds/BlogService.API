using BlogService.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BlogService.API.Controllers 
{
    public class UploadMediaRequest
    {
        /// <summary>Required. The blog this media belongs to.</summary>
        public Guid BlogId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
    }

    [ApiController]
    [Route("api/v1/[controller]")]
    public class MediaController : ControllerBase
    {
        private readonly IMediaService _mediaService;

        public MediaController(IMediaService mediaService)
        {
            _mediaService = mediaService;
        }

        /// <summary>
        /// Upload media for a blog. TenantId is resolved automatically from the BlogId.
        /// Do NOT pass TenantId — it is ignored even if provided.
        /// </summary>
        [HttpPost("upload")]
        [ApiExplorerSettings(GroupName = "admin")]

        public async Task<IActionResult> Upload([FromBody] UploadMediaRequest request)
        {
            if (request.BlogId == Guid.Empty)
                return BadRequest(new { error = "BlogId is required." });

            var result = await _mediaService.UploadAsync(
                request.BlogId,
                request.FileName,
                request.ContentType
            );

            if (result == null)
                return NotFound(new { error = $"Blog '{request.BlogId}' not found. TenantId could not be resolved." });

            return Ok(new
            {
                url = result.PublicUrl,
                fileName = result.FileName,
                contentType = result.ContentType,
                createdAt = result.CreatedAt
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            await Task.Delay(10);
            return Ok(new { Id = id, Url = "https://mock-storage.blob.core.windows.net/images/mock.jpg" });
        }

        [HttpDelete("{id}")]
        [ApiExplorerSettings(GroupName = "admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _mediaService.DeleteAsync(id);
            if (!deleted)
                return NotFound(new { error = $"Media '{id}' not found." });

            return Ok(new { message = "Media deleted successfully." }); // ← changed from NoContent() to Ok()
        }

        [HttpGet("blog/{blogId}")]
        public async Task<IActionResult> GetByBlog(Guid blogId)
        {
            var mediaList = await _mediaService.GetMediaByBlogIdAsync(blogId);
            var result = mediaList.Select(m => new
            {
                id = m.Id,
                url = m.PublicUrl,
                fileName = m.FileName,
                contentType = m.ContentType,
                createdAt = m.CreatedAt
            });
            return Ok(result);
        }
    }
}