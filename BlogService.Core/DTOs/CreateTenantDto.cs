using System.ComponentModel.DataAnnotations;

namespace BlogService.Core.DTOs
{
    public class CreateTenantDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Identifier { get; set; } = string.Empty; // e.g., site1 

        public bool IsActive { get; set; } = true; 
    }
}