namespace VersionLifecycle.Application.DTOs;

/// <summary>
/// Request DTO for asking a natural language question about tenant data.
/// </summary>
public class InsightsQueryDto
{
    /// <summary>
    /// The natural language question to ask.
    /// </summary>
    public string Question { get; set; } = string.Empty;
}

/// <summary>
/// Response DTO containing the AI-generated answer.
/// </summary>
public class InsightsResponseDto
{
    /// <summary>
    /// The original question that was asked.
    /// </summary>
    public string Question { get; set; } = string.Empty;

    /// <summary>
    /// The AI-generated answer.
    /// </summary>
    public string Answer { get; set; } = string.Empty;

    /// <summary>
    /// When the answer was generated.
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}
