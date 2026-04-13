using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace BlogService.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class MediaController : ControllerBase
    {
        [HttpPost("upload")]
        public async Task<IActionResult> Upload()
        {
            await Task.Delay(10);
            return Ok(new { Url = "https://mock-storage.blob.core.windows.net/images/mock.jpg" });
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
