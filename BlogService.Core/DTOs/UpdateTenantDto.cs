using System.ComponentModel.DataAnnotations;

namespace BlogService.Core.DTOs
{
    public class UpdateTenantDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Identifier { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}