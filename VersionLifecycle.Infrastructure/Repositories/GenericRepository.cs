namespace VersionLifecycle.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using VersionLifecycle.Core.Entities;
using VersionLifecycle.Core.Interfaces;
using VersionLifecycle.Infrastructure.Data;

/// <summary>
/// Generic repository implementation for all entities.
/// </summary>
public class GenericRepository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    /// <summary>
    /// Gets all non-deleted entities.
    /// </summary>
    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.Where(e => !e.IsDeleted).AsNoTracking().ToListAsync();
    }

    /// <summary>
    /// Gets an entity by ID.
    /// </summary>
    public virtual async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
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
    /// Soft deletes an entity.
    /// </summary>
    public virtual async Task<bool> DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity == null)
            return false;

        entity.IsDeleted = true;
        await UpdateAsync(entity);
        return true;
    }

    /// <summary>
    /// Checks if an entity exists.
    /// </summary>
    public virtual async Task<bool> ExistsAsync(int id)
    {
        return await _dbSet.AnyAsync(e => e.Id == id && !e.IsDeleted);
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
public class ApplicationRepository : GenericRepository<Application>
{
    public ApplicationRepository(AppDbContext context) : base(context) { }

    public async Task<Application?> GetByNameAsync(string name)
    {
        return await _dbSet
            .FirstOrDefaultAsync(a => a.Name == name && !a.IsDeleted);
    }

    public async Task<IEnumerable<Application>> GetWithVersionsAndEnvironmentsAsync()
    {
        return await _dbSet
            .Where(a => !a.IsDeleted)
            .Include(a => a.Versions)
            .Include(a => a.Environments)
            .AsNoTracking()
            .ToListAsync();
    }
}

/// <summary>
/// Specific repository for Version entity.
/// </summary>
public class VersionRepository : GenericRepository<Version>
{
    public VersionRepository(AppDbContext context) : base(context) { }

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
public class DeploymentRepository : GenericRepository<Deployment>
{
    public DeploymentRepository(AppDbContext context) : base(context) { }

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

    public async Task<Deployment?> GetWithEventsAsync(int id)
    {
        return await _dbSet
            .Include(d => d.Events)
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
    }
}

/// <summary>
/// Specific repository for Environment entity.
/// </summary>
public class EnvironmentRepository : GenericRepository<Environment>
{
    public EnvironmentRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Environment>> GetByApplicationIdAsync(int applicationId)
    {
        return await _dbSet
            .Where(e => e.ApplicationId == applicationId && !e.IsDeleted)
            .OrderBy(e => e.Order)
            .AsNoTracking()
            .ToListAsync();
    }
}

/// <summary>
/// Specific repository for Webhook entity.
/// </summary>
public class WebhookRepository : GenericRepository<Webhook>
{
    public WebhookRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Webhook>> GetByApplicationIdAsync(int applicationId)
    {
        return await _dbSet
            .Where(w => w.ApplicationId == applicationId && !w.IsDeleted && w.IsActive)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Webhook?> GetWithEventsAsync(int id, int take = 50)
    {
        return await _dbSet
            .Include(w => w.Events_History.OrderByDescending(e => e.CreatedAt).Take(take))
            .FirstOrDefaultAsync(w => w.Id == id && !w.IsDeleted);
    }
}
