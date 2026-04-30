using BlogService.Core.Entities;
using BlogService.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BlogService.Data
{
    public class BlogDbContext : DbContext
    {
        private readonly ITenantService _tenantService;

        public BlogDbContext(DbContextOptions<BlogDbContext> options, ITenantService tenantService)
            : base(options)
        {
            _tenantService = tenantService;
        }

        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<ApiKey> ApiKeys { get; set; }
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Media> Media { get; set; }
        public DbSet<PageView> PageViews { get; set; }
        public DbSet<Like> Likes { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            string tenantId = _tenantService.GetTenantId();

            // Global Query Filters for Multi-Tenancy
            // This ensures we never accidentally bleed data across tenants.
            // The tenantId is dynamically evaluated when queries are executed.
            modelBuilder.Entity<Blog>().HasQueryFilter(e => EF.Property<string>(e, "TenantId") == _tenantService.GetTenantId());
            modelBuilder.Entity<Category>().HasQueryFilter(e => EF.Property<string>(e, "TenantId") == _tenantService.GetTenantId());
            modelBuilder.Entity<Tag>().HasQueryFilter(e => EF.Property<string>(e, "TenantId") == _tenantService.GetTenantId());
            modelBuilder.Entity<Comment>().HasQueryFilter(e => EF.Property<string>(e, "TenantId") == _tenantService.GetTenantId());
            modelBuilder.Entity<Media>().HasQueryFilter(e => EF.Property<string>(e, "TenantId") == _tenantService.GetTenantId());
            modelBuilder.Entity<PageView>().HasQueryFilter(e => EF.Property<string>(e, "TenantId") == _tenantService.GetTenantId());
            modelBuilder.Entity<Like>().HasQueryFilter(e => EF.Property<string>(e, "TenantId") == _tenantService.GetTenantId());
            // User entity isolation strategy (some systems share users, some don't. We'll isolate).
           // Many-to-Many Relationships
            modelBuilder.Entity<Blog>()
                .HasMany(b => b.Categories)
                .WithMany(c => c.Blogs)
                .UsingEntity(j => j.ToTable("BlogCategories"));

            modelBuilder.Entity<Blog>()
                .HasMany(b => b.Tags)
                .WithMany(t => t.Blogs)
                .UsingEntity(j => j.ToTable("BlogTags"));

            // Unique Slugs per Tenant
            modelBuilder.Entity<Blog>()
                .HasIndex(b => new { b.TenantId, b.Slug })
                .IsUnique();

            modelBuilder.Entity<Category>()
                .HasIndex(c => new { c.TenantId, c.Slug })
                .IsUnique();

            modelBuilder.Entity<Tag>()
                .HasIndex(t => new { t.TenantId, t.Slug })
                .IsUnique();
                
            modelBuilder.Entity<Tenant>()
                .HasIndex(t => t.Identifier)
                .IsUnique();
        }

        public override int SaveChanges()
        {
            ApplyAuditAndTenantInformation();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyAuditAndTenantInformation();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void ApplyAuditAndTenantInformation()
        {
            var entries = ChangeTracker.Entries<BaseEntity>();
            var tenantId = _tenantService.GetTenantId();

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = DateTime.UtcNow;

                    // Automatically attach TenantId to entities that belong to a tenant
                    var tenantProperty = entry.Entity.GetType().GetProperty("TenantId");
                    if (tenantProperty != null)
                    {
                        var currentTenantId = tenantProperty.GetValue(entry.Entity)?.ToString();
                        if (string.IsNullOrEmpty(currentTenantId) && !string.IsNullOrEmpty(tenantId))
                        {
                            tenantProperty.SetValue(entry.Entity, tenantId);
                        }
                    }
                }

                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                }
            }
        }
    }
}
