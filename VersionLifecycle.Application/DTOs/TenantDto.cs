namespace VersionLifecycle.Application.DTOs;

/// <summary>
/// DTO for tenant information.
/// </summary>
public class TenantDto
{
    /// <summary>
    /// Tenant ID.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Tenant name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Tenant description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Subscription plan.
    /// </summary>
    public string SubscriptionPlan { get; set; } = string.Empty;

    /// <summary>
    /// Is active flag.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Tenant registration code (admin only context).
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Created date.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for creating a new tenant.
/// </summary>
public class CreateTenantDto
{
    /// <summary>
    /// Tenant name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Tenant description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Subscription plan.
    /// </summary>
    public string SubscriptionPlan { get; set; } = "Free";
}

/// <summary>
/// Lightweight tenant lookup for registration dropdown.
/// </summary>
public class TenantLookupDto
{
    /// <summary>
    /// Tenant ID.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Tenant name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Tenant description.
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// Tenant statistics for admin dashboard.
/// </summary>
public class TenantStatsDto
{
    /// <summary>
    /// Tenant ID.
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Total number of users in this tenant.
    /// </summary>
    public int UserCount { get; set; }

    /// <summary>
    /// Total number of applications in this tenant.
    /// </summary>
    public int ApplicationCount { get; set; }

    /// <summary>
    /// Total number of versions in this tenant.
    /// </summary>
    public int VersionCount { get; set; }

    /// <summary>
    /// Total number of deployments in this tenant.
    /// </summary>
    public int DeploymentCount { get; set; }
}
