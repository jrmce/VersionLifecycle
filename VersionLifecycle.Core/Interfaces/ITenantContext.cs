namespace VersionLifecycle.Core.Interfaces;

/// <summary>
/// Interface for accessing current tenant context.
/// </summary>
public interface ITenantContext
{
    /// <summary>
    /// Gets the current tenant ID.
    /// </summary>
    string CurrentTenantId { get; }

    /// <summary>
    /// Gets the current user ID.
    /// </summary>
    string? CurrentUserId { get; }

    /// <summary>
    /// Gets whether the current request should bypass tenant filtering (SuperAdmin).
    /// </summary>
    bool IsCrossTenantQuery { get; }

    /// <summary>
    /// Sets the tenant context.
    /// </summary>
    void SetTenant(string tenantId, string? userId = null);
}
