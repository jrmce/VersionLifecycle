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
