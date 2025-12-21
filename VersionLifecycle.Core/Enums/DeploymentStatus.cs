namespace VersionLifecycle.Core.Enums;

/// <summary>
/// Enumeration of possible deployment statuses.
/// </summary>
public enum DeploymentStatus
{
    /// <summary>
    /// Deployment is pending confirmation.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Deployment is currently in progress.
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Deployment completed successfully.
    /// </summary>
    Success = 2,

    /// <summary>
    /// Deployment failed.
    /// </summary>
    Failed = 3,

    /// <summary>
    /// Deployment was cancelled.
    /// </summary>
    Cancelled = 4
}
