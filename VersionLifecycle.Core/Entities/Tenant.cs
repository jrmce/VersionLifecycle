namespace VersionLifecycle.Core.Entities;

/// <summary>
/// Tenant entity representing an organization/team.
/// </summary>
public class Tenant
{
    /// <summary>
    /// Unique tenant identifier (usually UUID).
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Tenant name/organization name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Tenant description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Subscription plan (e.g., "Free", "Professional", "Enterprise").
    /// </summary>
    public string SubscriptionPlan { get; set; } = "Free";

    /// <summary>
    /// Tenant creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Indicates if the tenant is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Navigation property for applications owned by this tenant.
    /// </summary>
    public ICollection<Application> Applications { get; set; } = new List<Application>();
}
