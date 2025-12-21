namespace VersionLifecycle.Application.DTOs;
using VersionLifecycle.Core.Enums;

/// <summary>
/// DTO for deployment information.
/// </summary>
public class DeploymentDto
{
    /// <summary>
    /// Deployment ID.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Application ID.
    /// </summary>
    public int ApplicationId { get; set; }

    /// <summary>
    /// Version ID.
    /// </summary>
    public int VersionId { get; set; }

    /// <summary>
    /// Version number.
    /// </summary>
    public string? VersionNumber { get; set; }

    /// <summary>
    /// Environment ID.
    /// </summary>
    public int EnvironmentId { get; set; }

    /// <summary>
    /// Environment name.
    /// </summary>
    public string? EnvironmentName { get; set; }

    /// <summary>
    /// Deployment status.
    /// </summary>
    public DeploymentStatus Status { get; set; }

    /// <summary>
    /// Deployment timestamp.
    /// </summary>
    public DateTime DeployedAt { get; set; }

    /// <summary>
    /// User who deployed.
    /// </summary>
    public string? DeployedBy { get; set; }

    /// <summary>
    /// Deployment notes.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Deployment duration in ms.
    /// </summary>
    public long? DurationMs { get; set; }
}

/// <summary>
/// DTO for creating a pending deployment.
/// </summary>
public class CreatePendingDeploymentDto
{
    /// <summary>
    /// Application ID.
    /// </summary>
    public int ApplicationId { get; set; }

    /// <summary>
    /// Version ID to deploy.
    /// </summary>
    public int VersionId { get; set; }

    /// <summary>
    /// Target environment ID.
    /// </summary>
    public int EnvironmentId { get; set; }

    /// <summary>
    /// Optional notes about the deployment.
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for confirming a pending deployment.
/// </summary>
public class ConfirmDeploymentDto
{
    /// <summary>
    /// Deployment ID to confirm.
    /// </summary>
    public int DeploymentId { get; set; }

    /// <summary>
    /// Optional confirmation notes.
    /// </summary>
    public string? ConfirmationNotes { get; set; }
}
