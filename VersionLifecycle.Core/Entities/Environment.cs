namespace VersionLifecycle.Core.Entities;

/// <summary>
/// Environment entity representing deployment environments (Dev, Test, Prod, etc.).
/// Tenant-level entity shared across all applications within a tenant.
/// </summary>
public class Environment : BaseEntity
{
    /// <summary>
    /// Environment name (e.g., "Development", "Staging", "Production").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Environment description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Display order for timeline visualization.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Environment configuration (e.g., base URL, API endpoint).
    /// </summary>
    public string? Config { get; set; }

    /// <summary>
    /// Navigation property for deployments to this environment.
    /// </summary>
    public ICollection<Deployment> Deployments { get; set; } = new List<Deployment>();
}
