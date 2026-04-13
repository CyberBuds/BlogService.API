using System;

namespace BlogService.Core.Entities
{
    public class PageView : BaseEntity
    {
        public string TenantId { get; set; } = string.Empty;
        public Guid BlogId { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;

        public Blog Blog { get; set; } = null!;
    }
}
