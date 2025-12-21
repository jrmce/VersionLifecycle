namespace VersionLifecycle.Core.Enums;

/// <summary>
/// Enumeration of possible version statuses.
/// </summary>
public enum VersionStatus
{
    /// <summary>
    /// Version is in draft state (not yet released).
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Version has been released.
    /// </summary>
    Released = 1,

    /// <summary>
    /// Version is deprecated and no longer recommended.
    /// </summary>
    Deprecated = 2,

    /// <summary>
    /// Version is archived (no longer maintained).
    /// </summary>
    Archived = 3
}
