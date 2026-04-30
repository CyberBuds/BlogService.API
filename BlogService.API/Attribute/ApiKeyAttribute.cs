using BlogService.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using BlogService.Core.Interfaces;

namespace BlogService.API.Attribute
{
    public class ApiKeyAttribute : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Get API key from header
            if (!context.HttpContext.Request.Headers.TryGetValue("X-API-Key", out var apiKey))
            {
                context.Result = new UnauthorizedObjectResult("API Key is missing");
                return;
            }

            // Get service
            var apiKeyService = context.HttpContext.RequestServices.GetService(typeof(IApiKeyService)) as IApiKeyService;

            // Validate API key
            var isValid = await apiKeyService.ValidateApiKey(apiKey);

            if (!isValid)
            {
                context.Result = new UnauthorizedObjectResult("Invalid API Key");
                return;
            }

            await next();
        }
    }
}
