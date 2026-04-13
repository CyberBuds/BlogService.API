using System.Collections.Generic;

namespace BlogService.Core.Entities
{
    public class Tag : BaseEntity
    {
        public string TenantId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;

        public ICollection<Blog> Blogs { get; set; } = new List<Blog>();
    }
}
