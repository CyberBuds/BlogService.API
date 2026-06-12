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
        private readonly ICurrentUserService _currentUserService; // ✅ ADD THIS
        public BlogDbContext(DbContextOptions<BlogDbContext> options, ITenantService tenantService, ICurrentUserService currentUserService) // ✅ ADD THIS PARAMETER
            : base(options)
        {
            _tenantService = tenantService;
            _currentUserService = currentUserService; // ✅ ADD THIS
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

            // ❌ REMOVED: string tenantId = _tenantService.GetTenantId();
            // Wrong to call GetTenantId() at model-build time

            // ─────────────────────────────────────────────────────────────
            // Global Query Filters for Multi-Tenancy
            // tenantId is dynamically evaluated when queries are executed.
            // ─────────────────────────────────────────────────────────────

            // ✅ ApiKey — null-safe filter (NEWLY ADDED)
            modelBuilder.Entity<ApiKey>().HasQueryFilter(e =>
                string.IsNullOrEmpty(_tenantService.GetTenantId()) ||
                EF.Property<string>(e, "TenantId") == _tenantService.GetTenantId());

            // ✅ Blog — already null-safe (no change)
            modelBuilder.Entity<Blog>().HasQueryFilter(e =>
                string.IsNullOrEmpty(_tenantService.GetTenantId()) ||
                EF.Property<string>(e, "TenantId") == _tenantService.GetTenantId());

            // ✅ Category — FIXED: added null-safe check
            modelBuilder.Entity<Category>().HasQueryFilter(e =>
                string.IsNullOrEmpty(_tenantService.GetTenantId()) ||
                EF.Property<string>(e, "TenantId") == _tenantService.GetTenantId());

            // ✅ Tag — FIXED: added null-safe check
            modelBuilder.Entity<Tag>().HasQueryFilter(e =>
                string.IsNullOrEmpty(_tenantService.GetTenantId()) ||
                EF.Property<string>(e, "TenantId") == _tenantService.GetTenantId());

            // ✅ Comment — FIXED: added null-safe check
            modelBuilder.Entity<Comment>().HasQueryFilter(e =>
                string.IsNullOrEmpty(_tenantService.GetTenantId()) ||
                EF.Property<string>(e, "TenantId") == _tenantService.GetTenantId());

            // ✅ Media — already null-safe (no change)
            modelBuilder.Entity<Media>().HasQueryFilter(e =>
                string.IsNullOrEmpty(_tenantService.GetTenantId()) ||
                EF.Property<string>(e, "TenantId") == _tenantService.GetTenantId());

            // ✅ PageView — FIXED: added null-safe check
            modelBuilder.Entity<PageView>().HasQueryFilter(e =>
                string.IsNullOrEmpty(_tenantService.GetTenantId()) ||
                EF.Property<string>(e, "TenantId") == _tenantService.GetTenantId());

            // ✅ Like — FIXED: added null-safe check
            modelBuilder.Entity<Like>().HasQueryFilter(e =>
                string.IsNullOrEmpty(_tenantService.GetTenantId()) ||
                EF.Property<string>(e, "TenantId") == _tenantService.GetTenantId());

            // ✅ User — hides soft-deleted records from ALL queries automatically
            modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);

            // ─────────────────────────────────────────────────────────────
            // Many-to-Many Relationships
            // ─────────────────────────────────────────────────────────────

            modelBuilder.Entity<Blog>()
                .HasMany(b => b.Categories)
                .WithMany(c => c.Blogs)
                .UsingEntity(j => j.ToTable("BlogCategories"));

            modelBuilder.Entity<Blog>()
                .HasMany(b => b.Tags)
                .WithMany(t => t.Blogs)
                .UsingEntity(j => j.ToTable("BlogTags"));

            // ─────────────────────────────────────────────────────────────
            // Unique Indexes
            // ─────────────────────────────────────────────────────────────

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

            // Unique Tenant Identifier
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
            var currentUser = _currentUserService.GetCurrentUser(); // ✅ ADD THIS

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;

                    // ✅ ADD THIS BLOCK — set CreatedBy only on insert, never overwrite
                    if (entry.Entity.CreatedBy == null && currentUser != null)
                    {
                        entry.Entity.CreatedBy = currentUser;
                    }

                    // Automatically attach TenantId to entities that have the property
                    var tenantProperty = entry.Entity.GetType().GetProperty("TenantId");
                    if (tenantProperty != null && !string.IsNullOrEmpty(tenantId))
                    {
                        var currentTenantValue = tenantProperty.GetValue(entry.Entity);
                        var propertyType = tenantProperty.PropertyType;

                        // Check if current value is already set
                        bool isEmpty = currentTenantValue == null ||
                                       currentTenantValue.Equals(Guid.Empty) ||
                                       currentTenantValue.Equals(string.Empty);

                        if (isEmpty)
                        {
                            // ✅ If property is Guid or Guid?, parse the string tenantId
                            if (propertyType == typeof(Guid) || propertyType == typeof(Guid?))
                            {
                                if (Guid.TryParse(tenantId, out var tenantGuid))
                                    tenantProperty.SetValue(entry.Entity, tenantGuid);
                            }
                            // ✅ If property is string, set directly
                            else if (propertyType == typeof(string))
                            {
                                tenantProperty.SetValue(entry.Entity, tenantId);
                            }
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