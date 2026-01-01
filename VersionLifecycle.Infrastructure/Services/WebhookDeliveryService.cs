namespace VersionLifecycle.Infrastructure.Services;

using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using VersionLifecycle.Core.Entities;
using VersionLifecycle.Core.Interfaces;
using VersionLifecycle.Infrastructure.Repositories;

/// <summary>
/// Service for delivering webhook notifications.
/// </summary>
public class WebhookDeliveryService(
    WebhookRepository webhookRepository,
    IRepository<WebhookEvent> webhookEventRepository,
    IHttpClientFactory httpClientFactory,
    ILogger<WebhookDeliveryService> logger)
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("WebhookClient");

    /// <summary>
    /// Triggers webhooks for a deployment event.
    /// </summary>
    public async Task TriggerDeploymentWebhooksAsync(int applicationId, string eventType, object payload)
    {
        var webhooks = await webhookRepository.GetByApplicationIdAsync(applicationId);
        var activeWebhooks = webhooks.Where(w => w.Events.Contains(eventType) || w.Events.Contains("*"));

        foreach (var webhook in activeWebhooks)
        {
            await QueueWebhookDeliveryAsync(webhook, eventType, payload);
        }
    }

    /// <summary>
    /// Queues a webhook delivery event.
    /// </summary>
    private async Task QueueWebhookDeliveryAsync(Webhook webhook, string eventType, object payload)
    {
        var payloadJson = JsonSerializer.Serialize(payload);

        var webhookEvent = new WebhookEvent
        {
            WebhookId = webhook.Id,
            EventType = eventType,
            Payload = payloadJson,
            DeliveryStatus = "Pending",
            RetryCount = 0,
            TenantId = webhook.TenantId,
            CreatedBy = "system"
        };

        await webhookEventRepository.AddAsync(webhookEvent);

        // Attempt immediate delivery
        _ = Task.Run(async () => await DeliverWebhookAsync(webhookEvent.Id, webhook));
    }

    /// <summary>
    /// Delivers a webhook event.
    /// </summary>
    public async Task DeliverWebhookAsync(int webhookEventId, Webhook? webhook = null)
    {
        var webhookEvent = await webhookEventRepository.GetByIdAsync(webhookEventId);
        if (webhookEvent == null)
        {
            logger.LogWarning("WebhookEvent {EventId} not found", webhookEventId);
            return;
        }

        if (webhook == null)
        {
            webhook = await webhookRepository.GetByIdAsync(webhookEvent.WebhookId);
            if (webhook == null)
            {
                logger.LogWarning("Webhook {WebhookId} not found for event {EventId}", webhookEvent.WebhookId, webhookEventId);
                webhookEvent.DeliveryStatus = "Failed";
                await webhookEventRepository.UpdateAsync(webhookEvent);
                return;
            }
        }

        // Check if max retries exceeded
        if (webhookEvent.RetryCount >= webhook.MaxRetries)
        {
            logger.LogWarning("Max retries ({MaxRetries}) exceeded for webhook event {EventId}", webhook.MaxRetries, webhookEventId);
            webhookEvent.DeliveryStatus = "Failed";
            await webhookEventRepository.UpdateAsync(webhookEvent);
            return;
        }

        try
        {
            // Generate HMAC signature
            var signature = GenerateSignature(webhookEvent.Payload, webhook.Secret);

            // Prepare request
            var request = new HttpRequestMessage(HttpMethod.Post, webhook.Url)
            {
                Content = new StringContent(webhookEvent.Payload, Encoding.UTF8, "application/json")
            };
            request.Headers.Add("X-Webhook-Signature", signature);
            request.Headers.Add("X-Webhook-Event", webhookEvent.EventType);
            request.Headers.Add("X-Webhook-Id", webhookEvent.Id.ToString());

            // Send request
            var response = await _httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            webhookEvent.ResponseStatusCode = (int)response.StatusCode;
            webhookEvent.ResponseBody = responseBody.Length > 1000 ? responseBody.Substring(0, 1000) : responseBody;
            webhookEvent.DeliveredAt = DateTime.UtcNow;

            if (response.IsSuccessStatusCode)
            {
                webhookEvent.DeliveryStatus = "Sent";
                logger.LogInformation("Webhook event {EventId} delivered successfully to {Url}", webhookEventId, webhook.Url);
            }
            else
            {
                webhookEvent.DeliveryStatus = "Failed";
                webhookEvent.RetryCount++;
                
                // Schedule retry with exponential backoff
                if (webhookEvent.RetryCount < webhook.MaxRetries)
                {
                    var retryDelay = TimeSpan.FromMinutes(Math.Pow(2, webhookEvent.RetryCount));
                    webhookEvent.NextRetryAt = DateTime.UtcNow.Add(retryDelay);
                    logger.LogWarning("Webhook event {EventId} failed with status {StatusCode}. Scheduled retry {RetryCount}/{MaxRetries} at {NextRetry}", 
                        webhookEventId, response.StatusCode, webhookEvent.RetryCount, webhook.MaxRetries, webhookEvent.NextRetryAt);
                }
                else
                {
                    logger.LogError("Webhook event {EventId} failed permanently after {MaxRetries} retries", webhookEventId, webhook.MaxRetries);
                }
            }

            await webhookEventRepository.UpdateAsync(webhookEvent);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error delivering webhook event {EventId} to {Url}", webhookEventId, webhook.Url);
            
            webhookEvent.DeliveryStatus = "Failed";
            webhookEvent.RetryCount++;
            webhookEvent.ResponseBody = $"Exception: {ex.Message}";

            // Schedule retry with exponential backoff
            if (webhookEvent.RetryCount < webhook.MaxRetries)
            {
                var retryDelay = TimeSpan.FromMinutes(Math.Pow(2, webhookEvent.RetryCount));
                webhookEvent.NextRetryAt = DateTime.UtcNow.Add(retryDelay);
                logger.LogInformation("Scheduled retry {RetryCount}/{MaxRetries} for webhook event {EventId} at {NextRetry}", 
                    webhookEvent.RetryCount, webhook.MaxRetries, webhookEventId, webhookEvent.NextRetryAt);
            }

            await webhookEventRepository.UpdateAsync(webhookEvent);
        }
    }

    /// <summary>
    /// Retries pending webhook deliveries.
    /// </summary>
    public async Task RetryPendingWebhooksAsync()
    {
        var allWebhookEvents = await webhookEventRepository.GetAllAsync();
        var pendingEvents = allWebhookEvents
            .Where(e => e.DeliveryStatus == "Failed" && e.NextRetryAt.HasValue && e.NextRetryAt.Value <= DateTime.UtcNow)
            .ToList();

        logger.LogInformation("Retrying {Count} pending webhook events", pendingEvents.Count);

        foreach (var webhookEvent in pendingEvents)
        {
            await DeliverWebhookAsync(webhookEvent.Id);
        }
    }

    /// <summary>
    /// Generates HMAC-SHA256 signature for webhook payload.
    /// </summary>
    private static string GenerateSignature(string payload, string secret)
    {
        var encoding = Encoding.UTF8;
        var keyBytes = encoding.GetBytes(secret);
        var payloadBytes = encoding.GetBytes(payload);

        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(payloadBytes);
        return Convert.ToHexString(hashBytes).ToLower();
    }
}
