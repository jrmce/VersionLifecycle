namespace VersionLifecycle.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using VersionLifecycle.Core.Entities;
using VersionLifecycle.Infrastructure.Data;

/// <summary>
/// Repository for API token operations.
/// </summary>
public class ApiTokenRepository(AppDbContext context) : GenericRepository<ApiToken>(context)
{
    /// <summary>
    /// Finds an active, non-deleted token by its hash.
    /// </summary>
    public async Task<ApiToken?> GetByTokenHashAsync(string tokenHash)
    {
        return await _dbSet
            .Where(t => t.TokenHash == tokenHash && t.IsActive && !t.IsDeleted)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Gets all active API tokens for the current tenant.
    /// </summary>
    public async Task<IEnumerable<ApiToken>> GetActiveTokensAsync()
    {
        return await _dbSet
            .Where(t => t.IsActive && !t.IsDeleted)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Updates the LastUsedAt timestamp for a token.
    /// </summary>
    public async Task UpdateLastUsedAsync(int tokenId)
    {
        var token = await _dbSet.FindAsync(tokenId);
        if (token != null)
        {
            token.LastUsedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}
