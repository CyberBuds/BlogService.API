using BlogService.API.Attribute;
using Microsoft.AspNetCore.Mvc;

namespace BlogService.API.Controllers
{
    [ApiController]
    [Route("api/v1/public")]
    [ApiKey]  // This requires API key
    public class PublicController : ControllerBase
    {
        [HttpGet("data")]
        public IActionResult GetData()
        {
            return Ok("This is protected by API Key");
        }
    }
}
