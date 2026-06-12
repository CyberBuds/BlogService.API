using BlogService.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace BlogService.Service
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? GetCurrentUser()
        {
            //var user = _httpContextAccessor.HttpContext?.User;

            //if (user == null || user.Identity?.IsAuthenticated != true)
            //    return null;

            //var email = user.FindFirstValue(ClaimTypes.Email)
            //         ?? user.FindFirstValue("email");

            //if (!string.IsNullOrEmpty(email))
            //    return email;

            //return user.FindFirstValue(ClaimTypes.Name)
            //    ?? user.FindFirstValue("username")
            //    ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _httpContextAccessor.HttpContext?.User;

            if (user == null || user.Identity?.IsAuthenticated != true)
                return null;

            return user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? user.FindFirstValue("sub")
                ?? user.FindFirstValue("id");
        }
    }
}