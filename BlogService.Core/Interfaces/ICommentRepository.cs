using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BlogService.Core.Entities;

namespace BlogService.Core.Interfaces
{
    public interface ICommentRepository
    {
        Task<(IEnumerable<Comment> Comments, int TotalCount)> GetByBlogAndTenantAsync(
            Guid blogId,
            string tenantId,
            bool? isApproved,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default);
    }
}