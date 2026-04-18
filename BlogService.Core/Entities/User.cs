using System;

namespace BlogService.Core.Entities
{
    public class User : BaseEntity
    {
        public Guid Id { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }
        public string? Role { get; set; } = "Viewer";


        public string? TenantId { get; set; }
    }
}
