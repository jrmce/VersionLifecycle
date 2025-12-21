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
    /// Sets the tenant context.
    /// </summary>
    void SetTenant(string tenantId, string? userId = null);
}
