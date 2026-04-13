using System;

namespace BlogService.Core.Entities
{
    public class Like : BaseEntity
    {
        public string TenantId { get; set; } = string.Empty;
        public Guid BlogId { get; set; }
        public Guid? UserId { get; set; } // Null if anonymous like
        public string IpAddress { get; set; } = string.Empty;

        public Blog Blog { get; set; } = null!;
        public User? User { get; set; }
    }
}
