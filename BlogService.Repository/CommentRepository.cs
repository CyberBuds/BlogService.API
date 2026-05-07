using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BlogService.Core.Entities;
using BlogService.Core.Interfaces;
using BlogService.Data;
using Microsoft.EntityFrameworkCore;

namespace BlogService.Repository
{
    public class CommentRepository : ICommentRepository
    {
        private readonly BlogDbContext _context;

        public CommentRepository(BlogDbContext context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<Comment> Comments, int TotalCount)> GetByBlogAndTenantAsync(
            Guid blogId,
            string tenantId,
            bool? isApproved,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            // Match your existing entity — TenantId is string, IsApproved is bool
            var query = _context.Comments
                .Where(c => c.BlogId == blogId && c.TenantId == tenantId);

            // Only filter if caller passed true or false — null means return all
            if (isApproved.HasValue)
                query = query.Where(c => c.IsApproved == isApproved.Value);

            var totalCount = await query.CountAsync(cancellationToken);

            var comments = await query
                .OrderByDescending(c => c.CreatedAt)   // from your BaseEntity
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return (comments, totalCount);
        }
    }
}