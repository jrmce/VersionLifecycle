namespace VersionLifecycle.Core.Entities;
using VersionLifecycle.Core.Enums;

/// <summary>
/// Deployment aggregate root. Tracks deployment of a version to an environment.
/// </summary>
public class Deployment : BaseEntity
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
    /// Version identifier (foreign key).
    /// </summary>
    public int VersionId { get; set; }

    /// <summary>
    /// Navigation property for the deployed version.
    /// </summary>
    public Version? Version { get; set; }

    /// <summary>
    /// Environment identifier (foreign key).
    /// </summary>
    public int EnvironmentId { get; set; }

    /// <summary>
    /// Navigation property for the target environment.
    /// </summary>
    public Environment? Environment { get; set; }

    /// <summary>
    /// Deployment status (Pending, InProgress, Success, Failed, Cancelled).
    /// </summary>
    public DeploymentStatus Status { get; set; } = DeploymentStatus.Pending;

    /// <summary>
    /// Deployment timestamp.
    /// </summary>
    public DateTime DeployedAt { get; set; }

    /// <summary>
    /// User ID who initiated the deployment.
    /// </summary>
    public string? DeployedBy { get; set; }

    /// <summary>
    /// Deployment notes or comments.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Duration of the deployment in milliseconds.
    /// </summary>
    public long? DurationMs { get; set; }

    /// <summary>    /// Completion timestamp (when deployment finished).
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>    /// Navigation property for deployment events/history.
    /// </summary>
    public ICollection<DeploymentEvent> Events { get; set; } = new List<DeploymentEvent>();
}
