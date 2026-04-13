using System;

namespace BlogService.Core.Entities
{
    public class Comment : BaseEntity
    {
        public string TenantId { get; set; } = string.Empty;
        public Guid BlogId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string AuthorEmail { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public bool IsApproved { get; set; } = false;

        public Blog Blog { get; set; } = null!;
    }
}
