namespace VersionLifecycle.Application.DTOs;
using VersionLifecycle.Core.Enums;

/// <summary>
/// DTO for version information.
/// </summary>
public class VersionDto
{
    /// <summary>
    /// Version ID.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Application ID.
    /// </summary>
    public Guid ApplicationId { get; set; }

    /// <summary>
    /// Version number.
    /// </summary>
    public string VersionNumber { get; set; } = string.Empty;

    /// <summary>
    /// Version status.
    /// </summary>
    public VersionStatus Status { get; set; }

    /// <summary>
    /// Release notes.
    /// </summary>
    public string? ReleaseNotes { get; set; }

    /// <summary>
    /// Release date.
    /// </summary>
    public DateTime? ReleasedAt { get; set; }

    /// <summary>
    /// Creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for creating a version.
/// </summary>
public class CreateVersionDto
{
    /// <summary>
    /// Application ID.
    /// </summary>
    public Guid ApplicationId { get; set; }

    /// <summary>
    /// Version number.
    /// </summary>
    public string VersionNumber { get; set; } = string.Empty;

    /// <summary>
    /// Release notes.
    /// </summary>
    public string? ReleaseNotes { get; set; }

    /// <summary>
    /// Initial status (usually Draft).
    /// </summary>
    public VersionStatus Status { get; set; } = VersionStatus.Draft;
}

/// <summary>
/// DTO for updating a version.
/// </summary>
public class UpdateVersionDto
{
    /// <summary>
    /// Version number.
    /// </summary>
    public string? VersionNumber { get; set; }

    /// <summary>
    /// Version status.
    /// </summary>
    public VersionStatus? Status { get; set; }

    /// <summary>
    /// Release notes.
    /// </summary>
    public string? ReleaseNotes { get; set; }
}
