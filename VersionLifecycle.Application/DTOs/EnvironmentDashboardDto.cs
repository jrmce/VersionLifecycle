using System;
using System.Collections.Generic;

namespace VersionLifecycle.Application.DTOs;

/// <summary>
/// Represents the latest deployment state for an application within an environment.
/// </summary>
public class EnvironmentDeploymentStatusDto
{
    public int DeploymentId { get; set; }
    public int ApplicationId { get; set; }
    public string ApplicationName { get; set; } = string.Empty;
    public int VersionId { get; set; }
    public string VersionNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime DeployedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

/// <summary>
/// Summary view of deployments per environment for dashboard consumption.
/// </summary>
public class EnvironmentDeploymentOverviewDto
{
    public int EnvironmentId { get; set; }
    public string EnvironmentName { get; set; } = string.Empty;
    public int Order { get; set; }
    public string? Description { get; set; }
    public IList<EnvironmentDeploymentStatusDto> Deployments { get; set; } = new List<EnvironmentDeploymentStatusDto>();
}
