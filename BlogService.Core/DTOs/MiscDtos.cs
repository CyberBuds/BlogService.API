using System;

namespace BlogService.Core.DTOs
{
    public class CommentDto
    {
        public Guid Id { get; set; }
        public Guid BlogId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public bool IsApproved { get; set; }
    }

    public class CreateCommentDto
    {
        public Guid BlogId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string AuthorEmail { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    public class UserDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    public class GetCommentsByBlogQueryDto
    {
        public Guid BlogId { get; set; }
        public string TenantId { get; set; } = string.Empty;
        public bool? IsApproved { get; set; }   // null = all, true = approved, false = pending
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class PagedResult<T>
    {
        public IEnumerable<T> Data { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}
