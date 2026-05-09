using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BlogService.Core.DTOs;
using BlogService.Core.Interfaces;

namespace BlogService.Service
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;

        public CommentService(ICommentRepository commentRepository)
        {
            _commentRepository = commentRepository;
        }

        public async Task<PagedResult<CommentDto>> GetCommentsByBlogAsync(
            GetCommentsByBlogQueryDto query,
            CancellationToken cancellationToken = default)
        {
            // Safety limits
            query.PageSize = Math.Min(query.PageSize, 100);
            query.Page = Math.Max(query.Page, 1);

            var (comments, totalCount) = await _commentRepository.GetByBlogAndTenantAsync(
                query.BlogId,
                query.TenantId,
                query.IsApproved,
                query.Page,
                query.PageSize,
                cancellationToken);

            // Map entity → existing CommentDto (matches your fields exactly)
            var dtos = comments.Select(c => new CommentDto
            {
                Id = c.Id,
                BlogId = c.BlogId,
                AuthorName = c.AuthorName,
                Content = c.Content,
                IsApproved = c.IsApproved,
                CreatedAt = c.CreatedAt
            });

            return new PagedResult<CommentDto>
            {
                Data = dtos,
                TotalCount = totalCount,
                Page = query.Page,
                PageSize = query.PageSize
            };
        }
    }
}