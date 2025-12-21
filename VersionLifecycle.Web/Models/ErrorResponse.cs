namespace VersionLifecycle.Web.Models;

/// <summary>
/// Standard error response model.
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Error code for categorization.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable error message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Trace ID for debugging.
    /// </summary>
    public string? TraceId { get; set; }

    /// <summary>
    /// Additional error details.
    /// </summary>
    public Dictionary<string, object>? Details { get; set; }

    /// <summary>
    /// Error timestamp.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Paginated response wrapper.
/// </summary>
public class PaginatedResponse<T>
{
    /// <summary>
    /// Data items.
    /// </summary>
    public IEnumerable<T> Data { get; set; } = new List<T>();

    /// <summary>
    /// Total count of items.
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// Items skipped (page number * page size).
    /// </summary>
    public int Skip { get; set; }

    /// <summary>
    /// Items per page.
    /// </summary>
    public int Take { get; set; }

    /// <summary>
    /// Total pages.
    /// </summary>
    public int TotalPages => (Total + Take - 1) / Take;
}
