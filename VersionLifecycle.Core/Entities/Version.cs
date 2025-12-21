namespace VersionLifecycle.Core.Entities;
using VersionLifecycle.Core.Enums;

/// <summary>
/// Version entity representing an application version/release.
/// </summary>
public class Version : BaseEntity
{
    /// <summary>
    /// Application identifier (foreign key).
    /// </summary>
    public int ApplicationId { get; set; }

    /// <summary>
    /// Navigation property for the parent application.
    /// </summary>
    public Application? Application { get; set; }

    /// <summary>
    /// Version number (e.g., "1.0.0", "2.1.3").
    /// </summary>
    public string VersionNumber { get; set; } = string.Empty;

    /// <summary>
    /// Version status (Draft, Released, Deprecated, Archived).
    /// </summary>
    public VersionStatus Status { get; set; } = VersionStatus.Draft;

    /// <summary>
    /// Release notes or changelog.
    /// </summary>
    public string? ReleaseNotes { get; set; }

    /// <summary>
    /// Release date timestamp.
    /// </summary>
    public DateTime? ReleasedAt { get; set; }

    /// <summary>
    /// Navigation property for deployments of this version.
    /// </summary>
    public ICollection<Deployment> Deployments { get; set; } = new List<Deployment>();
}
