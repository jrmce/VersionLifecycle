namespace VersionLifecycle.Core.Entities;

/// <summary>
/// WebhookEvent entity. Tracks webhook delivery attempts.
/// </summary>
public class WebhookEvent : BaseEntity
{
    /// <summary>
    /// Webhook identifier (foreign key).
    /// </summary>
    public int WebhookId { get; set; }

    /// <summary>
    /// Navigation property for the parent webhook.
    /// </summary>
    public Webhook? Webhook { get; set; }

    /// <summary>
    /// Event type that triggered this webhook call.
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Payload sent to the webhook endpoint (JSON).
    /// </summary>
    public string Payload { get; set; } = string.Empty;

    /// <summary>
    /// Delivery status (Pending, Sent, Failed).
    /// </summary>
    public string DeliveryStatus { get; set; } = "Pending";

    /// <summary>
    /// HTTP response status code.
    /// </summary>
    public int? ResponseStatusCode { get; set; }

    /// <summary>
    /// HTTP response body.
    /// </summary>
    public string? ResponseBody { get; set; }

    /// <summary>
    /// Number of delivery attempts.
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// Last delivery attempt timestamp.
    /// </summary>
    public DateTime? DeliveredAt { get; set; }

    /// <summary>
    /// Next scheduled retry timestamp.
    /// </summary>
    public DateTime? NextRetryAt { get; set; }
}
