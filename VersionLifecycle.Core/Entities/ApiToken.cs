namespace VersionLifecycle.Core.Entities;

/// <summary>
/// API token entity for system-to-system authentication.
/// Tokens provide tenant admin privileges for external integrations.
/// </summary>
public class ApiToken : BaseEntity
{
    /// <summary>
    /// User-friendly name for the token (e.g., "CI/CD Pipeline", "Monitoring Service").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional description of the token's purpose.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// SHA256 hash of the token. Never store plaintext tokens.
    /// </summary>
    public string TokenHash { get; set; } = string.Empty;

    /// <summary>
    /// Token prefix for identification (e.g., "vl_live_" or "vl_test_").
    /// Stored separately for display purposes without revealing the full token.
    /// </summary>
    public string TokenPrefix { get; set; } = string.Empty;

    /// <summary>
    /// Token expiration date. Null means no expiration.
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Last time this token was used for authentication.
    /// </summary>
    public DateTime? LastUsedAt { get; set; }

    /// <summary>
    /// Whether this token is active. Use IsDeleted for soft delete.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Optional metadata for rate limiting, IP restrictions, etc. (JSON format).
    /// </summary>
    public string? Metadata { get; set; }
}
