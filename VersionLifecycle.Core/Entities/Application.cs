namespace VersionLifecycle.Core.Entities;

/// <summary>
/// Application aggregate root. Represents a software application being version-tracked.
/// </summary>
public class Application : BaseEntity
{
    /// <summary>
    /// Application name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Application description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Repository URL (e.g., GitHub/GitLab link).
    /// </summary>
    public string? RepositoryUrl { get; set; }

    /// <summary>
    /// Navigation property for versions.
    /// </summary>
    public ICollection<Version> Versions { get; set; } = new List<Version>();

    /// <summary>
    /// Navigation property for deployments.
    /// </summary>
    public ICollection<Deployment> Deployments { get; set; } = new List<Deployment>();

    /// <summary>
    /// Navigation property for webhooks.
    /// </summary>
    public ICollection<Webhook> Webhooks { get; set; } = new List<Webhook>();
}
