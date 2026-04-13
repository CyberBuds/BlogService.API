using System;

namespace BlogService.Core.DTOs
{
    public class TagDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
    }

    public class CreateTagDto
    {
        public string Name { get; set; } = string.Empty;
    }

    public class UpdateTagDto
    {
        public string Name { get; set; } = string.Empty;
    }
}
