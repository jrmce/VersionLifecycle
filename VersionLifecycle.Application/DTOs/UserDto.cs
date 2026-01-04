namespace VersionLifecycle.Application.DTOs;

/// <summary>
/// DTO for user information.
/// </summary>
public class UserDto
{
    /// <summary>
    /// User ID.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// User email.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User role.
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Tenant ID.
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Date the user was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for updating user role.
/// </summary>
public class UpdateUserRoleDto
{
    /// <summary>
    /// New role for the user (Viewer, Manager, Admin).
    /// </summary>
    public string Role { get; set; } = string.Empty;
}
