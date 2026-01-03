namespace VersionLifecycle.Application.DTOs;
using System.ComponentModel.DataAnnotations;
using VersionLifecycle.Core.Enums;

/// <summary>
/// DTO for deployment information.
/// </summary>
public class DeploymentDto
{
    /// <summary>
    /// Deployment ID.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Application ID.
    /// </summary>
    public Guid ApplicationId { get; set; }

    /// <summary>
    /// Application name.
    /// </summary>
    public string? ApplicationName { get; set; }

    /// <summary>
    /// Version ID.
    /// </summary>
    public Guid VersionId { get; set; }

    /// <summary>
    /// Version number.
    /// </summary>
    public string? VersionNumber { get; set; }

    /// <summary>
    /// Environment ID.
    /// </summary>
    public Guid EnvironmentId { get; set; }

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

    /// <summary>
    /// Completion timestamp.
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last modified timestamp.
    /// </summary>
    public DateTime ModifiedAt { get; set; }
}

/// <summary>
/// DTO for creating a pending deployment.
/// </summary>
public class CreatePendingDeploymentDto
{
    /// <summary>
    /// Application ID.
    /// </summary>
    [Required(ErrorMessage = "Application ID is required")]
    public Guid ApplicationId { get; set; }

    /// <summary>
    /// Version ID to deploy.
    /// </summary>
    [Required(ErrorMessage = "Version ID is required")]
    public Guid VersionId { get; set; }

    /// <summary>
    /// Target environment ID.
    /// </summary>
    [Required(ErrorMessage = "Environment ID is required")]
    public Guid EnvironmentId { get; set; }

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
    [Required(ErrorMessage = "Deployment ID is required")]
    public Guid DeploymentId { get; set; }

    /// <summary>
    /// Optional confirmation notes.
    /// </summary>
    public string? ConfirmationNotes { get; set; }
}

/// <summary>
/// DTO for updating deployment status through the UI or automation.
/// </summary>
public class UpdateDeploymentStatusDto
{
    /// <summary>
    /// Target status for the deployment.
    /// </summary>
    [Required]
    public DeploymentStatus Status { get; set; }

    /// <summary>
    /// Optional operator notes for the status change.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Optional duration (ms) if known at the time of completion.
    /// </summary>
    [Range(0, long.MaxValue)]
    public long? DurationMs { get; set; }
}

/// <summary>
/// DTO for promoting an existing deployment to the next environment.
/// </summary>
public class PromoteDeploymentDto
{
    /// <summary>
    /// Target environment ID (must be the next environment in order).
    /// </summary>
    [Required(ErrorMessage = "Target environment is required")]
    public Guid TargetEnvironmentId { get; set; }

    /// <summary>
    /// Optional operator notes for the promotion.
    /// </summary>
    public string? Notes { get; set; }
}
