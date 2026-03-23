namespace VersionLifecycle.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using VersionLifecycle.Core.Entities;
using VersionLifecycle.Core.Interfaces;
using VersionLifecycle.Infrastructure.Data;

/// <summary>
/// Repository for API token operations.
/// </summary>
public class ApiTokenRepository(AppDbContext context, ITenantContext tenantContext) : GenericRepository<ApiToken>(context, tenantContext)
{
    /// <summary>
    /// Finds an active, non-deleted token by its hash, ensuring it belongs to the current tenant.
    /// </summary>
    public async Task<ApiToken?> GetByTokenHashAsync(string tokenHash)
    {
        var query = _dbSet.Where(t => t.TokenHash == tokenHash && t.IsActive && !t.IsDeleted);
        if (!_tenantContext.IsCrossTenantQuery)
            query = query.Where(t => t.TenantId == _tenantContext.CurrentTenantId);
        return await query.FirstOrDefaultAsync();
    }

    /// <summary>
    /// Gets all non-deleted API tokens for the current tenant.
    /// </summary>
    public async Task<IEnumerable<ApiToken>> GetActiveTokensAsync()
    {
        var query = _dbSet.Where(t => !t.IsDeleted);
        if (!_tenantContext.IsCrossTenantQuery)
            query = query.Where(t => t.TenantId == _tenantContext.CurrentTenantId);
        return await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
    }

    /// <summary>
    /// Updates the LastUsedAt timestamp for a token.
    /// </summary>
    public async Task UpdateLastUsedAsync(int tokenId)
    {
        var token = await _dbSet.FindAsync(tokenId);
        if (token != null && (_tenantContext.IsCrossTenantQuery || token.TenantId == _tenantContext.CurrentTenantId))
        {
            token.LastUsedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}
