using System.Threading;
using System.Threading.Tasks;
using BlogService.Core.DTOs;

namespace BlogService.Core.Interfaces
{
    public interface ICommentService
    {
        Task<PagedResult<CommentDto>> GetCommentsByBlogAsync(
            GetCommentsByBlogQueryDto query,
            CancellationToken cancellationToken = default);
    }
}