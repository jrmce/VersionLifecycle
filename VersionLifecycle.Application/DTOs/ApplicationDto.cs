namespace VersionLifecycle.Application.DTOs;

/// <summary>
/// DTO for application information.
/// </summary>
public class ApplicationDto
{
    /// <summary>
    /// Application ID.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Application name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Application description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Repository URL.
    /// </summary>
    public string? RepositoryUrl { get; set; }

    /// <summary>
    /// Creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last modification timestamp.
    /// </summary>
    public DateTime? ModifiedAt { get; set; }
}

/// <summary>
/// DTO for creating an application.
/// </summary>
public class CreateApplicationDto
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
    /// Repository URL.
    /// </summary>
    public string? RepositoryUrl { get; set; }
}

/// <summary>
/// DTO for updating an application.
/// </summary>
public class UpdateApplicationDto
{
    /// <summary>
    /// Application name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Application description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Repository URL.
    /// </summary>
    public string? RepositoryUrl { get; set; }
}
