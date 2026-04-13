using BlogService.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace BlogService.API.Middleware
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TenantMiddleware> _logger;

        public TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, ITenantService tenantService)
        {
            // Try to get TenantId from HTTP headers
            if (context.Request.Headers.TryGetValue("TenantId", out var tenantIdValues))
            {
                var tenantId = tenantIdValues.ToString();
                tenantService.SetTenantId(tenantId);
            }
            else
            {
                // Note: For some public endpoints (like System health), TenantId might not be strictly required.
                // But generally for most API operations it should be provided.
                _logger.LogWarning("TenantId header is missing.");
            }

            await _next(context);
        }
    }
}
