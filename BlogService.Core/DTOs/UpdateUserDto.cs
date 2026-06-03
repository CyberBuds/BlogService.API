using System.ComponentModel.DataAnnotations;

namespace BlogService.Core.DTOs
{
    public class UpdateUserDto
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        // Optional — only update password if provided
        public string? Password { get; set; }

        // Optional — only update role if provided
        public string? Role { get; set; }
    }
}