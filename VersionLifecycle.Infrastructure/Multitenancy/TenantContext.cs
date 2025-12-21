namespace VersionLifecycle.Infrastructure.Multitenancy;

/// <summary>
/// Scoped service for managing current tenant context.
/// </summary>
public class TenantContext : Core.Interfaces.ITenantContext
{
    private string _currentTenantId = string.Empty;
    private string? _currentUserId;

    /// <summary>
    /// Gets the current tenant ID.
    /// </summary>
    public string CurrentTenantId => _currentTenantId;

    /// <summary>
    /// Gets the current user ID.
    /// </summary>
    public string? CurrentUserId => _currentUserId;

    /// <summary>
    /// Sets the tenant context.
    /// </summary>
    public void SetTenant(string tenantId, string? userId = null)
    {
        _currentTenantId = tenantId;
        _currentUserId = userId;
    }

    /// <summary>
    /// Clears the tenant context.
    /// </summary>
    public void Clear()
    {
        _currentTenantId = string.Empty;
        _currentUserId = null;
    }
}
