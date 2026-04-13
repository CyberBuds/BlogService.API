using System;

namespace BlogService.Core.Entities
{
    public class Media : BaseEntity
    {
        public string TenantId { get; set; } = string.Empty;
        public Guid? BlogId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string PublicUrl { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;

        public Blog? Blog { get; set; }
    }
}
