namespace VersionLifecycle.Application.DTOs;

/// <summary>
/// DTO for API token response (list/get operations).
/// </summary>
public class ApiTokenDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string TokenPrefix { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

/// <summary>
/// DTO for creating a new API token.
/// </summary>
public class CreateApiTokenDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

/// <summary>
/// DTO returned when creating a new token (includes plaintext token - shown only once).
/// </summary>
public class ApiTokenCreatedDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Token { get; set; } = string.Empty; // Full plaintext token (show only once!)
    public string TokenPrefix { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for updating an API token (name, description, active status).
/// </summary>
public class UpdateApiTokenDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}
