namespace VersionLifecycle.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using VersionLifecycle.Core.Entities;
using VersionLifecycle.Infrastructure.Data;

/// <summary>
/// Repository for Tenant entity.
/// </summary>
public class TenantRepository
{
    private readonly AppDbContext _context;
    private readonly DbSet<Tenant> _dbSet;

    public TenantRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<Tenant>();
    }

    /// <summary>
    /// Gets all tenants.
    /// </summary>
    public async Task<IEnumerable<Tenant>> GetAllAsync()
    {
        return await _dbSet.AsNoTracking().ToListAsync();
    }

    /// <summary>
    /// Gets a tenant by ID.
    /// </summary>
    public async Task<Tenant?> GetByIdAsync(string id)
    {
        return await _dbSet.FirstOrDefaultAsync(t => t.Id == id);
    }

    /// <summary>
    /// Adds a new tenant.
    /// </summary>
    public async Task<Tenant> AddAsync(Tenant tenant)
    {
        await _dbSet.AddAsync(tenant);
        await _context.SaveChangesAsync();
        return tenant;
    }

    /// <summary>
    /// Updates an existing tenant.
    /// </summary>
    public async Task<Tenant> UpdateAsync(Tenant tenant)
    {
        _dbSet.Update(tenant);
        await _context.SaveChangesAsync();
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
            await _context.SaveChangesAsync();
        }
    }
}
