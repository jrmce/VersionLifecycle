namespace VersionLifecycle.Core.Entities;

/// <summary>
/// Webhook entity for registering external webhooks for deployment events.
/// </summary>
public class Webhook : BaseEntity
{
    /// <summary>
    /// Application identifier (foreign key).
    /// </summary>
    public int ApplicationId { get; set; }

    /// <summary>
    /// Navigation property for the parent application.
    /// </summary>
    public Application? Application { get; set; }

    /// <summary>
    /// Webhook endpoint URL.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Secret for HMAC signature validation.
    /// </summary>
    public string Secret { get; set; } = string.Empty;

    /// <summary>
    /// Comma-separated list of event types to subscribe to.
    /// </summary>
    public string Events { get; set; } = "deployment.completed";

    /// <summary>
    /// Indicates if the webhook is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Retry count after failures.
    /// </summary>
    public int MaxRetries { get; set; } = 5;

    /// <summary>
    /// Navigation property for webhook events.
    /// </summary>
    public ICollection<WebhookEvent> Events_History { get; set; } = new List<WebhookEvent>();
}
