using System;

namespace BlogService.Core.Entities
{
    public class Tenant : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Identifier { get; set; } = string.Empty; // e.g., site1
        public bool IsActive { get; set; } = true;
    }
}
