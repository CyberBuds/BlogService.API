using BlogService.Core.Entities;
using BlogService.Core.Interfaces;
using BlogService.Service;
using BlogService.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlogService.API.Controllers
{
    [ApiController]
    [Route("api/v1/apikeys")]
    [Authorize]  // User must be logged in
    [ApiExplorerSettings(GroupName = "admin")]
    public class ApiKeyController : ControllerBase
    {
        private readonly IApiKeyService _apiKeyService;
        private readonly ITenantService _tenantService; // ✅ correct type
        public ApiKeyController(IApiKeyService apiKeyService, ITenantService tenantService)
        {
            _apiKeyService = apiKeyService;
            _tenantService = tenantService;
        }


        // ── 1. GET ──────────────────────────────────
        [HttpGet]
        [HttpGet, Produces("application/json")]
        public async Task<IActionResult> GetAllApiKeys()
        {
            var tenantId = _tenantService.GetTenantId();
            if (string.IsNullOrEmpty(tenantId))
            {
                return BadRequest(new
                {
                    status = 400,
                    message = "TenantId is required. Please provide a valid TenantId in the request header."
                });
            }
            var apiKeys = await _apiKeyService.GetAllApiKeys();
            return Ok(apiKeys);
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateApiKey(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return BadRequest("Name is required");
            }

            // ✅ Validate TenantId — must be present and non-empty
            var tenantId = _tenantService.GetTenantId();
            if (string.IsNullOrEmpty(tenantId))
            {
                return BadRequest(new
                {
                    status = 400,
                    message = "TenantId is required. Please provide a valid TenantId in the request header."
                });
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

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateApiKey(Guid id, [FromBody] UpdateApiKeyRequest request)
        {
            if (request == null)
                return BadRequest("Request body is required");

            var tenantId = _tenantService.GetTenantId();
            if (string.IsNullOrEmpty(tenantId))
            {
                return BadRequest(new
                {
                    status = 400,
                    message = "TenantId is required. Please provide a valid TenantId in the request header."
                });
            }

            // ✅ Returns the new regenerated key
            var newKey = await _apiKeyService.UpdateApiKey(id, request.Name, request.IsActive);
            if (newKey == null)
                return NotFound(new { status = 404, message = "API Key not found" });

            return Ok(new
            {
                status = 200,
                message = "API Key regenerated successfully. Keep it safe!",
                newApiKey = newKey  // ✅ Return new key to user
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

        // Request model for Update
        // ─────────────────────────────────────────────
        public class UpdateApiKeyRequest
        {
            public string? Name { get; set; }
            public bool IsActive { get; set; }
        }
    }
}
