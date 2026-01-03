namespace VersionLifecycle.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using VersionLifecycle.Core.Entities;
using VersionLifecycle.Core.Interfaces;
using VersionLifecycle.Infrastructure.Data;

/// <summary>
/// Generic repository implementation for all entities.
/// </summary>
public class GenericRepository<T>(AppDbContext context) : IRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext _context = context;
    protected readonly DbSet<T> _dbSet = context.Set<T>();

    /// <summary>
    /// Gets all non-deleted entities.
    /// </summary>
    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.Where(e => !e.IsDeleted).AsNoTracking().ToListAsync();
    }

    /// <summary>
    /// Gets an entity by internal ID.
    /// </summary>
    public virtual async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
    }

    /// <summary>
    /// Gets an entity by external ID.
    /// </summary>
    public virtual async Task<T?> GetByExternalIdAsync(Guid externalId)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.ExternalId == externalId && !e.IsDeleted);
    }

    /// <summary>
    /// Adds a new entity.
    /// </summary>
    public virtual async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    public virtual async Task<T> UpdateAsync(T entity)
    {
        // Check if entity is already tracked
        var trackedEntity = _context.ChangeTracker.Entries<T>()
            .FirstOrDefault(e => e.Entity.Id == entity.Id);

        if (trackedEntity != null)
        {
            // Entity is already tracked, just update the tracked instance
            _context.Entry(trackedEntity.Entity).CurrentValues.SetValues(entity);
        }
        else
        {
            // Entity is not tracked, attach it and mark as modified
            _context.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        await _context.SaveChangesAsync();
        return entity;
    }

    /// <summary>
    /// Soft deletes an entity by external ID.
    /// </summary>
    public virtual async Task<bool> DeleteAsync(Guid externalId)
    {
        var entity = await GetByExternalIdAsync(externalId);
        if (entity == null)
            return false;

        entity.IsDeleted = true;
        await UpdateAsync(entity);
        return true;
    }

    /// <summary>
    /// Checks if an entity exists by external ID.
    /// </summary>
    public virtual async Task<bool> ExistsAsync(Guid externalId)
    {
        return await _dbSet.AnyAsync(e => e.ExternalId == externalId && !e.IsDeleted);
    }

    /// <summary>
    /// Gets count of all non-deleted entities.
    /// </summary>
    public virtual async Task<int> CountAsync()
    {
        return await _dbSet.Where(e => !e.IsDeleted).CountAsync();
    }
}

/// <summary>
/// Specific repository for Application entity.
/// </summary>
public class ApplicationRepository(AppDbContext context) : GenericRepository<Application>(context)
{
    public async Task<Application?> GetByNameAsync(string name)
    {
        return await _dbSet
            .FirstOrDefaultAsync(a => a.Name == name && !a.IsDeleted);
    }

    public async Task<IEnumerable<Application>> GetWithVersionsAsync()
    {
        return await _dbSet
            .Where(a => !a.IsDeleted)
            .Include(a => a.Versions)
            .AsNoTracking()
            .ToListAsync();
    }
}

/// <summary>
/// Specific repository for Version entity.
/// </summary>
public class VersionRepository(AppDbContext context) : GenericRepository<Version>(context)
{
    public async Task<IEnumerable<Version>> GetByApplicationIdAsync(int applicationId)
    {
        return await _dbSet
            .Where(v => v.ApplicationId == applicationId && !v.IsDeleted)
            .OrderByDescending(v => v.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Version?> GetByVersionNumberAsync(int applicationId, string versionNumber)
    {
        return await _dbSet
            .FirstOrDefaultAsync(v => v.ApplicationId == applicationId && 
                                      v.VersionNumber == versionNumber && 
                                      !v.IsDeleted);
    }
}

/// <summary>
/// Specific repository for Deployment entity.
/// </summary>
public class DeploymentRepository(AppDbContext context) : GenericRepository<Deployment>(context)
{
    public override async Task<Deployment?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Where(d => d.Id == id && !d.IsDeleted)
            .Include(d => d.Application)
            .Include(d => d.Version)
            .Include(d => d.Environment)
            .Include(d => d.Events)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public override async Task<Deployment?> GetByExternalIdAsync(Guid externalId)
    {
        return await _dbSet
            .Where(d => d.ExternalId == externalId && !d.IsDeleted)
            .Include(d => d.Application)
            .Include(d => d.Version)
            .Include(d => d.Environment)
            .Include(d => d.Events)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Deployment>> GetAllWithNavigationAsync()
    {
        return await _dbSet
            .Where(d => !d.IsDeleted)
            .Include(d => d.Application)
            .Include(d => d.Version)
            .Include(d => d.Environment)
            .OrderByDescending(d => d.DeployedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Deployment>> GetByApplicationIdAsync(int applicationId, int skip = 0, int take = 25)
    {
        return await _dbSet
            .Where(d => d.ApplicationId == applicationId && !d.IsDeleted)
            .Include(d => d.Version)
            .Include(d => d.Environment)
            .Include(d => d.Events)
            .OrderByDescending(d => d.DeployedAt)
            .Skip(skip)
            .Take(take)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<int> GetCountByApplicationIdAsync(int applicationId)
    {
        return await _dbSet
            .Where(d => d.ApplicationId == applicationId && !d.IsDeleted)
            .CountAsync();
    }

    public async Task<Deployment?> GetWithEventsAsync(Guid externalId)
    {
        return await _dbSet
            .Include(d => d.Events)
            .FirstOrDefaultAsync(d => d.ExternalId == externalId && !d.IsDeleted);
    }
}

/// <summary>
/// Specific repository for Environment entity.
/// </summary>
public class EnvironmentRepository(AppDbContext context) : GenericRepository<Environment>(context)
{

    /// <summary>
    /// Gets all environments for the current tenant, ordered by display order.
    /// </summary>
    public override async Task<IEnumerable<Environment>> GetAllAsync()
    {
        return await _dbSet
            .Where(e => !e.IsDeleted)
            .OrderBy(e => e.Order)
            .AsNoTracking()
            .ToListAsync();
    }
}

/// <summary>
/// Specific repository for Webhook entity.
/// </summary>
public class WebhookRepository(AppDbContext context) : GenericRepository<Webhook>(context)
{
    public async Task<IEnumerable<Webhook>> GetByApplicationIdAsync(int applicationId)
    {
        return await _dbSet
            .Where(w => w.ApplicationId == applicationId && !w.IsDeleted && w.IsActive)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Webhook?> GetWithEventsAsync(Guid externalId, int take = 50)
    {
        return await _dbSet
            .Include(w => w.Events_History.OrderByDescending(e => e.CreatedAt).Take(take))
            .FirstOrDefaultAsync(w => w.ExternalId == externalId && !w.IsDeleted);
    }
}
