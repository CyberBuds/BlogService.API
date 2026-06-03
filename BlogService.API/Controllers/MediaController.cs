using BlogService.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using System;
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

            return Ok(new { url = result.PublicUrl });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            await Task.Delay(10);
            return Ok(new { Id = id, Url = "https://mock-storage.blob.core.windows.net/images/mock.jpg" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await Task.Delay(10);
            return NoContent();
        }

        [HttpGet("blog/{blogId}")]
        public async Task<IActionResult> GetByBlog(Guid blogId)
        {
            await Task.Delay(10);
            return Ok(new[] { new { Id = Guid.NewGuid(), Url = "mock.jpg" } });
        }
    }
}