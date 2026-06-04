using BlogService.Core.Entities;
using BlogService.Core.Interfaces;
using BlogService.Service.Interface;
using System;
using System.Threading.Tasks;
using System.Linq;
namespace BlogService.Service
{
    public class MediaService : IMediaService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITenantService _tenantService;

        public MediaService(IUnitOfWork unitOfWork, ITenantService tenantService)
        {
            _unitOfWork = unitOfWork;
            _tenantService = tenantService;
        }

        public async Task<Media?> UploadAsync(Guid blogId, string fileName, string contentType)
        {
            // Step 1: No TenantId is set yet, so the Blog query filter allows
            //         cross-tenant lookup (guarded by the IsNullOrEmpty check in DbContext).
            //         This is intentional — we need the blog to RESOLVE the tenant.
            var blog = await _unitOfWork.Repository<Blog>().GetByIdAsync(blogId);
            if (blog == null) return null;

            // Step 2: Now that we have the blog's TenantId, inject it into the
            //         tenant context so all subsequent DbContext operations
            //         (query filters + ApplyAuditAndTenantInformation) work correctly.
            _tenantService.SetTenantId(blog.TenantId);

            // Step 3: Build the media record. TenantId is set explicitly here
            //         AND will also be applied by ApplyAuditAndTenantInformation
            //         as a belt-and-suspenders guarantee.
            var media = new Media
            {
                Id = Guid.NewGuid(),
                BlogId = blogId,
                TenantId = blog.TenantId,
                FileName = fileName,
                ContentType = contentType,
                PublicUrl = $"https://mock-storage.blob.core.windows.net/images/{Guid.NewGuid()}.jpg"
            };

            await _unitOfWork.Repository<Media>().AddAsync(media);
            await _unitOfWork.SaveChangesAsync();

            return media;
        }
        public async Task<IEnumerable<Media>> GetMediaByBlogIdAsync(Guid blogId)
        {
            // TenantId is already set by TenantMiddleware from the request header.
            // DbContext query filter on Media automatically scopes results to the tenant.
            var allMedia = await _unitOfWork.Repository<Media>().GetAllAsync();
            return allMedia.Where(m => m.BlogId == blogId);
        }
        public async Task<bool> DeleteAsync(Guid mediaId)
        {

            // Use GetAllAsync so the Media query filter (IsNullOrEmpty guard) applies,
            // then match by Id — no TenantId header needed
            var allMedia = await _unitOfWork.Repository<Media>().GetAllAsync();
            var media = allMedia.FirstOrDefault(m => m.Id == mediaId);
            if (media == null) return false;

            _unitOfWork.Repository<Media>().Delete(media);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}