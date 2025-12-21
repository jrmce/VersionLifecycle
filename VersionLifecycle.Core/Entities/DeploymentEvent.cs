namespace VersionLifecycle.Core.Entities;

/// <summary>
/// DeploymentEvent entity. Tracks events/milestones within a deployment.
/// </summary>
public class DeploymentEvent : BaseEntity
{
    /// <summary>
    /// Deployment identifier (foreign key).
    /// </summary>
    public int DeploymentId { get; set; }

    /// <summary>
    /// Navigation property for the parent deployment.
    /// </summary>
    public Deployment? Deployment { get; set; }

    /// <summary>
    /// Event type (e.g., "Started", "Completed", "Failed", "Rolled Back").
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Event message/description.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Event timestamp.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Additional event metadata as JSON.
    /// </summary>
    public string? Metadata { get; set; }
}
