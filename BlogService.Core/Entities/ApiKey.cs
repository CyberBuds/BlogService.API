using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogService.Core.Entities
{
    public class ApiKey : BaseEntity 
    {
        public string? Key { get; set; } 
        public string? Name { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? ExpiresAt { get; set; } 
    }
}
