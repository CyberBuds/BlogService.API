using BlogService.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlogService.API.Controllers 
{
    [ApiController]
    [Route("api/v1/apikeys")]
    [Authorize]  // User must be logged in
    public class ApiKeyController : ControllerBase
    {
        private readonly IApiKeyService _apiKeyService;

        public ApiKeyController(IApiKeyService apiKeyService)
        {
            _apiKeyService = apiKeyService;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateApiKey(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return BadRequest("Name is required");
            }

            // Get current user ID from JWT token
            var userEmail = User.FindFirst(ClaimTypes.Name)?.Value;

            // You need to get userId from email - modify based on your setup
            // For now, using a placeholder - you should fetch from database
            Guid userId = Guid.NewGuid(); // Replace with actual user lookup

            var apiKey = await _apiKeyService.GenerateApiKey(name);

            return Ok(new
            {
                ApiKey = apiKey,
                Message = "API Key generated successfully. Keep it safe!"
            });
        }
        [HttpDelete]
        public async Task<IActionResult> DeleteApiKey(string apiKey)
        {
            var result = await _apiKeyService.DeleteApiKey(apiKey);

            if (result)
            {
                return Ok("API Key deleted successfully");
            }
            else
            {
                return NotFound("API Key not found");
            }
        }
    }
}
