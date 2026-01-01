namespace VersionLifecycle.Infrastructure.Data;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VersionLifecycle.Core.Entities;
using VersionLifecycle.Core.Interfaces;
using VersionLifecycle.Infrastructure.Multitenancy;

/// <summary>
/// Entity Framework Core DbContext for the Version Lifecycle Management system.
/// </summary>
/// <remarks>
/// Initializes a new instance of the AppDbContext class.
/// </remarks>
public class AppDbContext(DbContextOptions<AppDbContext> options, ITenantContext tenantContext) : IdentityDbContext<IdentityUser>(options)
{

    /// <summary>
    /// Tenants DbSet.
    /// </summary>
    public DbSet<Tenant> Tenants { get; set; } = null!;

    /// <summary>
    /// Applications DbSet.
    /// </summary>
    public DbSet<Application> Applications { get; set; } = null!;

    /// <summary>
    /// Versions DbSet.
    /// </summary>
    public DbSet<Version> Versions { get; set; } = null!;

    /// <summary>
    /// Environments DbSet.
    /// </summary>
    public DbSet<Environment> Environments { get; set; } = null!;

    /// <summary>
    /// Deployments DbSet.
    /// </summary>
    public DbSet<Deployment> Deployments { get; set; } = null!;

    /// <summary>
    /// Deployment Events DbSet.
    /// </summary>
    public DbSet<DeploymentEvent> DeploymentEvents { get; set; } = null!;

    /// <summary>
    /// Webhooks DbSet.
    /// </summary>
    public DbSet<Webhook> Webhooks { get; set; } = null!;

    /// <summary>
    /// Webhook Events DbSet.
    /// </summary>
    public DbSet<WebhookEvent> WebhookEvents { get; set; } = null!;

    /// <summary>
    /// API Tokens DbSet.
    /// </summary>
    public DbSet<ApiToken> ApiTokens { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        
        // Enable foreign keys for SQLite
        optionsBuilder.ConfigureWarnings(warnings => 
            warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.AmbientTransactionWarning));
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure domain entities
        ConfigureApplications(builder);
        ConfigureVersions(builder);
        ConfigureEnvironments(builder);
        ConfigureDeployments(builder);
        ConfigureWebhooks(builder);
        ConfigureApiTokens(builder);
        ConfigureTenants(builder);

        // Add global query filter for multi-tenancy
        AddTenantQueryFilters(builder);
    }

    /// <summary>
    /// Configures the Application entity.
    /// </summary>
    private void ConfigureApplications(ModelBuilder builder)
    {
        builder.Entity<Application>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.RepositoryUrl).HasMaxLength(500);
            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);

            entity.HasIndex(e => new { e.TenantId, e.Name }).IsUnique();
            entity.HasIndex(e => e.TenantId);

            entity.HasMany(e => e.Versions)
                .WithOne(v => v.Application)
                .HasForeignKey(v => v.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Deployments)
                .WithOne(d => d.Application)
                .HasForeignKey(d => d.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    /// <summary>
    /// Configures the Version entity.
    /// </summary>
    private void ConfigureVersions(ModelBuilder builder)
    {
        builder.Entity<Version>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.VersionNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Status).HasConversion<int>();
            entity.Property(e => e.ReleaseNotes).HasMaxLength(5000);
            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);

            entity.HasIndex(e => new { e.ApplicationId, e.VersionNumber }).IsUnique();
            entity.HasIndex(e => e.TenantId);

            entity.HasMany(e => e.Deployments)
                .WithOne(d => d.Version)
                .HasForeignKey(d => d.VersionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    /// <summary>
    /// Configures the Environment entity.
    /// </summary>
    private void ConfigureEnvironments(ModelBuilder builder)
    {
        builder.Entity<Environment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Config).HasMaxLength(5000);
            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);

            entity.HasIndex(e => new { e.TenantId, e.Name }).IsUnique();
            entity.HasIndex(e => e.TenantId);

            entity.HasMany(e => e.Deployments)
                .WithOne(d => d.Environment)
                .HasForeignKey(d => d.EnvironmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    /// <summary>
    /// Configures the Deployment entity.
    /// </summary>
    private void ConfigureDeployments(ModelBuilder builder)
    {
        builder.Entity<Deployment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasConversion<int>();
            entity.Property(e => e.Notes).HasMaxLength(2000);
            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);

            entity.HasIndex(e => new { e.ApplicationId, e.VersionId, e.EnvironmentId }).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.DeployedAt);

            entity.HasMany(e => e.Events)
                .WithOne(de => de.Deployment)
                .HasForeignKey(de => de.DeploymentId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    /// <summary>
    /// Configures the Webhook entity.
    /// </summary>
    private void ConfigureWebhooks(ModelBuilder builder)
    {
        builder.Entity<Webhook>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Url).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Secret).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Events).IsRequired().HasMaxLength(500);
            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);

            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.ApplicationId);

            entity.HasMany(e => e.Events_History)
                .WithOne(we => we.Webhook)
                .HasForeignKey(we => we.WebhookId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    /// <summary>
    /// Configures the ApiToken entity.
    /// </summary>
    private void ConfigureApiTokens(ModelBuilder builder)
    {
        builder.Entity<ApiToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.TokenHash).IsRequired().HasMaxLength(500);
            entity.Property(e => e.TokenPrefix).IsRequired().HasMaxLength(20);
            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Metadata).HasMaxLength(4000);

            entity.HasIndex(e => e.TokenHash).IsUnique();
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => new { e.TenantId, e.IsActive, e.IsDeleted });
        });
    }

    /// <summary>
    /// Configures the Tenant entity.
    /// </summary>
    private void ConfigureTenants(ModelBuilder builder)
    {
        builder.Entity<Tenant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.SubscriptionPlan).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(100);

            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.Code);

            entity.HasMany(e => e.Applications)
                .WithOne()
                .HasForeignKey(a => a.TenantId)
                .HasPrincipalKey(t => t.Id)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Environments)
                .WithOne()
                .HasForeignKey(env => env.TenantId)
                .HasPrincipalKey(t => t.Id)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
    /// <summary>
    /// Adds global query filter for multi-tenancy.
    /// </summary>
    private void AddTenantQueryFilters(ModelBuilder builder)
    {
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(AppDbContext).GetMethod(nameof(GetTenantFilterLambda), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (method != null)
                {
                    var genericMethod = method.MakeGenericMethod(entityType.ClrType);
                    var filter = genericMethod.Invoke(this, null);
                    
                    var parameter = builder.Model.FindEntityType(entityType.ClrType);
                    if (parameter != null && filter != null)
                    {
                        parameter.SetQueryFilter((System.Linq.Expressions.LambdaExpression)filter);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Creates a lambda expression for tenant filtering.
    /// </summary>
    private System.Linq.Expressions.LambdaExpression? GetTenantFilterLambda<T>() where T : BaseEntity
    {
        // Use the tenant context property directly so the current tenant is evaluated per request
        System.Linq.Expressions.Expression<System.Func<T, bool>> filter = e => e.TenantId == tenantContext.CurrentTenantId;
        return filter;
    }

    public override int SaveChanges()
    {
        UpdateAuditProperties();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditProperties();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Updates audit properties before saving.
    /// </summary>
    private void UpdateAuditProperties()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.TenantId = tenantContext.CurrentTenantId;
                entry.Entity.CreatedBy = tenantContext.CurrentUserId ?? "system";
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.ModifiedAt = DateTime.UtcNow;
                entry.Entity.ModifiedBy = tenantContext.CurrentUserId ?? "system";
            }
        }
    }
}
