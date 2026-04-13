using BlogService.Core.Interfaces;
using BlogService.Data;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace BlogService.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly BlogDbContext _context;
        private ConcurrentDictionary<Type, object>? _repositories;

        public UnitOfWork(BlogDbContext context)
        {
            _context = context;
        }

        public IRepository<T> Repository<T>() where T : class
        {
            _repositories ??= new ConcurrentDictionary<Type, object>();

            var type = typeof(T);

            if (!_repositories.ContainsKey(type))
            {
                var repositoryType = typeof(Repository<>);
                var repositoryInstance = Activator.CreateInstance(repositoryType.MakeGenericType(typeof(T)), _context);
                
                if (repositoryInstance != null)
                {
                    _repositories.TryAdd(type, repositoryInstance);
                }
            }

            return (IRepository<T>)_repositories[type]!;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
