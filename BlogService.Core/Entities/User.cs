using System;

namespace BlogService.Core.Entities
{
    public class User : BaseEntity
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "Viewer"; // Admin, Editor, Viewer
        
        // Multi-tenant isolation for users as well (optional, but good for SaaS)
        public string TenantId { get; set; } = string.Empty;
    }
}
