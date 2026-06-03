using BlogService.Core.Entities;
using System;
using System.Threading.Tasks;

namespace BlogService.Service.Interface
{
    public interface IMediaService
    {
        Task<Media?> UploadAsync(Guid blogId, string fileName, string contentType);
    }
}