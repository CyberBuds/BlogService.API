using BlogService.Core.Interfaces;
using BlogService.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BlogService.API.Middleware
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TenantMiddleware> _logger;

        private static readonly string[] _tenantOptionalPathPrefixes =
        {
            "/api/v1/system",
            "/api/v1/health",
            "/api/v1/admin/tenants",
            "/api/v1/auth"
        };

        public TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, ITenantService tenantService, BlogDbContext dbContext)
        {
            var path = context.Request.Path.Value ?? string.Empty;
            var isTenantOptional = _tenantOptionalPathPrefixes.Any(
                prefix => path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

            if (!context.Request.Headers.TryGetValue("x-api-key", out var apiKeyValues) ||
     string.IsNullOrWhiteSpace(apiKeyValues))
            {
                if (isTenantOptional)
                {
                    await _next(context);
                    return;
                }

                await WriteProblemAsync(
                    context,
                    StatusCodes.Status401Unauthorized,
                    "API Key Required",
                    "The 'x-api-key' header is required.");
                return;
            }

            var apiKey = apiKeyValues.ToString().Trim();

            var apiKeyEntity = await dbContext.ApiKeys
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Key == apiKey &&
                    x.IsActive);

            if (apiKeyEntity == null)
            {
                await WriteProblemAsync(
                    context,
                    StatusCodes.Status401Unauthorized,
                    "Invalid API Key",
                    "API Key is invalid.");
                return;
            }

            // Convert TenantId string to Guid
            if (!Guid.TryParse(apiKeyEntity.TenantId, out Guid tenantGuid))
            {
                await WriteProblemAsync(
                    context,
                    StatusCodes.Status400BadRequest,
                    "Invalid Tenant",
                    "TenantId stored in API Key is not a valid GUID.");
                return;
            }

            var tenant = await dbContext.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t =>
                    !t.IsDeleted &&
                    t.Id == tenantGuid);

            if (tenant == null)
            {
                await WriteProblemAsync(
                    context,
                    StatusCodes.Status404NotFound,
                    "Tenant Not Found",
                    "No tenant is associated with this API key.");
                return;
            }

            tenantService.SetTenantId(tenant.Id.ToString());

            await _next(context);
        }

        private static async Task WriteProblemAsync(HttpContext context, int statusCode, string title, string detail)
        {
            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail
            };

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsJsonAsync(problemDetails);
        }
    }
}