namespace VersionLifecycle.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using VersionLifecycle.Core.Entities;
using VersionLifecycle.Infrastructure.Data;

/// <summary>
/// Repository for Tenant entity.
/// </summary>
public class TenantRepository(AppDbContext context)
{
    private readonly DbSet<Tenant> _dbSet = context.Set<Tenant>();

    /// <summary>
    /// Gets all tenants.
    /// </summary>
    public async Task<IEnumerable<Tenant>> GetAllAsync()
    {
        return await _dbSet.AsNoTracking().ToListAsync();
    }

    /// <summary>
    /// Gets active tenants.
    /// </summary>
    public async Task<IEnumerable<Tenant>> GetActiveAsync()
    {
        return await _dbSet.AsNoTracking().Where(t => t.IsActive).ToListAsync();
    }

    /// <summary>
    /// Gets a tenant by ID.
    /// </summary>
    public async Task<Tenant?> GetByIdAsync(string id)
    {
        return await _dbSet.FirstOrDefaultAsync(t => t.Id == id);
    }

    /// <summary>
    /// Checks if a tenant exists and is active with the specified code.
    /// </summary>
    public async Task<bool> ExistsActiveWithCodeAsync(string id, string code)
    {
        return await _dbSet.AsNoTracking().AnyAsync(t => t.Id == id && t.IsActive && t.Code == code);
    }

    /// <summary>
    /// Checks if a tenant code already exists.
    /// </summary>
    public async Task<bool> CodeExistsAsync(string code)
    {
        return await _dbSet.AsNoTracking().AnyAsync(t => t.Code == code);
    }

    /// <summary>
    /// Adds a new tenant.
    /// </summary>
    public async Task<Tenant> AddAsync(Tenant tenant)
    {
        await _dbSet.AddAsync(tenant);
        await context.SaveChangesAsync();
        return tenant;
    }

    /// <summary>
    /// Updates an existing tenant.
    /// </summary>
    public async Task<Tenant> UpdateAsync(Tenant tenant)
    {
        _dbSet.Update(tenant);
        await context.SaveChangesAsync();
        return tenant;
    }

    /// <summary>
    /// Deletes a tenant.
    /// </summary>
    public async Task DeleteAsync(string id)
    {
        var tenant = await _dbSet.FirstOrDefaultAsync(t => t.Id == id);
        if (tenant != null)
        {
            _dbSet.Remove(tenant);
            await context.SaveChangesAsync();
        }
    }
}
