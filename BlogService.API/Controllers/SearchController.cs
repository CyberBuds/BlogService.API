using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BlogService.API.Controllers
{
    [ApiController]
    [Route("api/v1")]
    public class SearchController : ControllerBase
    {
        [HttpGet("search/blogs")]
        public async Task<IActionResult> SearchBlogs([FromQuery] string q)
        {
            await Task.Delay(10);
            return Ok(new[] { new { Title = $"Result for {q}" } });
        }

        [HttpGet("blogs/filter")]
        public async Task<IActionResult> FilterBlogs([FromQuery] string? category, [FromQuery] string? tag, [FromQuery] string? date)
        {
            await Task.Delay(10);
            return Ok(new[] { new { Title = "Filtered Blog" } });
        }

        [HttpGet("blogs/paginated")]
        public async Task<IActionResult> GetPaginatedBlogs([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            await Task.Delay(10);
            return Ok(new
            {
                Page = page,
                PageSize = pageSize,
                TotalRecords = 100,
                Data = new[] { new { Title = "Paginated Blog" } }
            });
        }
    }
}
