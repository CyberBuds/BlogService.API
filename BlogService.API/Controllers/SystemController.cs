using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace BlogService.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class SystemController : ControllerBase
    {
        private readonly IConfiguration _config;

        public SystemController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("/api/v1/health")]
        [AllowAnonymous]
        public IActionResult GetHealth()
        {
            return Ok(new { status = "Healthy", timestamp = System.DateTime.UtcNow });
        }

        [HttpGet("/api/v1/version")]
        [AllowAnonymous]
        public IActionResult GetVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
            return Ok(new { version = version ?? "1.0.0" });
        }

        [HttpGet("/api/v1/config")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetConfig()
        {
            // Just returning parts of the config safely, simulating a system config endpoint
            return Ok(new { environment = "Development", dbType = "SqlServer" });
        }
    }
}
