namespace VersionLifecycle.Application.DTOs;

/// <summary>
/// DTO for environment information.
/// </summary>
public class EnvironmentDto
{
    /// <summary>
    /// Environment ID.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Environment name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Environment description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Display order.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Environment configuration.
    /// </summary>
    public string? Config { get; set; }

    /// <summary>
    /// Creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for creating an environment.
/// </summary>
public class CreateEnvironmentDto
{
    /// <summary>
    /// Environment name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Environment description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Display order.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Environment configuration.
    /// </summary>
    public string? Config { get; set; }
}

/// <summary>
/// DTO for updating an environment.
/// </summary>
public class UpdateEnvironmentDto
{
    /// <summary>
    /// Environment name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Environment description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Display order.
    /// </summary>
    public int? Order { get; set; }

    /// <summary>
    /// Environment configuration.
    /// </summary>
    public string? Config { get; set; }
}
