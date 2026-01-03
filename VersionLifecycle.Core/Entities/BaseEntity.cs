namespace VersionLifecycle.Core.Entities;

/// <summary>
/// Base entity class for all domain entities with multi-tenancy support.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Internal database identifier (primary key).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// External identifier exposed via API (GUID for security and portability).
    /// </summary>
    public Guid ExternalId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Tenant identifier for multi-tenancy isolation.
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Entity creation timestamp (shadow property, set via EF Core).
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// User ID who created the entity.
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Last modification timestamp (shadow property).
    /// </summary>
    public DateTime? ModifiedAt { get; set; }

    /// <summary>
    /// User ID who last modified the entity.
    /// </summary>
    public string? ModifiedBy { get; set; }

    /// <summary>
    /// Soft delete flag.
    /// </summary>
    public bool IsDeleted { get; set; }
}
