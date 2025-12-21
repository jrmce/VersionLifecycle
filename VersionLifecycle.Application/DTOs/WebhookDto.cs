namespace VersionLifecycle.Application.DTOs;

/// <summary>
/// DTO for webhook information.
/// </summary>
public class WebhookDto
{
    /// <summary>
    /// Webhook ID.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Application ID.
    /// </summary>
    public int ApplicationId { get; set; }

    /// <summary>
    /// Webhook URL.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Subscribed events.
    /// </summary>
    public string Events { get; set; } = string.Empty;

    /// <summary>
    /// Is webhook active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Maximum retries.
    /// </summary>
    public int MaxRetries { get; set; }

    /// <summary>
    /// Creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for creating a webhook.
/// </summary>
public class CreateWebhookDto
{
    /// <summary>
    /// Application ID.
    /// </summary>
    public int ApplicationId { get; set; }

    /// <summary>
    /// Webhook URL.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Secret for signature validation.
    /// </summary>
    public string Secret { get; set; } = string.Empty;

    /// <summary>
    /// Events to subscribe to.
    /// </summary>
    public string Events { get; set; } = "deployment.completed";

    /// <summary>
    /// Maximum retries.
    /// </summary>
    public int MaxRetries { get; set; } = 5;
}
