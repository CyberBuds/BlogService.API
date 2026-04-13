using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace BlogService.API.Controllers
{
    [ApiController]
    [Route("api/v1/analytics")]
    public class AnalyticsController : ControllerBase
    {
        [HttpGet("blog/{id}/views")]
        public async Task<IActionResult> GetViews(Guid id)
        {
            await Task.Delay(10);
            return Ok(new { BlogId = id, Views = 1250 });
        }

        [HttpPost("blog/{id}/view")]
        public async Task<IActionResult> RecordView(Guid id)
        {
            await Task.Delay(10);
            return Ok();
        }

        [HttpPost("blog/{id}/like")]
        public async Task<IActionResult> RecordLike(Guid id)
        {
            await Task.Delay(10);
            return Ok();
        }

        [HttpGet("blog/{id}/likes")]
        public async Task<IActionResult> GetLikes(Guid id)
        {
            await Task.Delay(10);
            return Ok(new { BlogId = id, Likes = 340 });
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            await Task.Delay(10);
            return Ok(new
            {
                TotalViews = 50000,
                TotalLikes = 15000,
                ActiveUsers = 450
            });
        }
    }
}
