namespace VersionLifecycle.Infrastructure.Services;

using AutoMapper;
using VersionLifecycle.Application.DTOs;
using VersionLifecycle.Application.Services;
using VersionLifecycle.Core.Entities;
using VersionLifecycle.Core.Interfaces;
using VersionLifecycle.Infrastructure.Repositories;

/// <summary>
/// Service for managing webhooks.
/// </summary>
public class WebhookService(WebhookRepository repository, WebhookDeliveryService webhookDeliveryService, IMapper mapper, ITenantContext tenantContext) : IWebhookService
{
    public async Task<IEnumerable<WebhookDto>> GetWebhooksAsync(int applicationId)
    {
        var webhooks = await repository.GetByApplicationIdAsync(applicationId);
        return mapper.Map<IEnumerable<WebhookDto>>(webhooks);
    }

    public async Task<WebhookDto?> GetWebhookAsync(int id)
    {
        var webhook = await repository.GetByIdAsync(id);
        return webhook == null ? null : mapper.Map<WebhookDto>(webhook);
    }

    public async Task<WebhookDto> CreateWebhookAsync(CreateWebhookDto dto)
    {
        var webhook = new Webhook
        {
            ApplicationId = dto.ApplicationId,
            Url = dto.Url,
            Secret = dto.Secret,
            Events = dto.Events,
            MaxRetries = dto.MaxRetries,
            IsActive = true,
            TenantId = tenantContext.CurrentTenantId,
            CreatedBy = tenantContext.CurrentUserId ?? "system"
        };

        await repository.AddAsync(webhook);
        return mapper.Map<WebhookDto>(webhook);
    }

    public async Task<WebhookDto> UpdateWebhookAsync(int id, UpdateWebhookDto dto)
    {
        var webhook = await repository.GetByIdAsync(id);
        if (webhook == null)
            throw new InvalidOperationException($"Webhook with ID {id} not found");

        if (!string.IsNullOrEmpty(dto.Url))
            webhook.Url = dto.Url;
        
        if (!string.IsNullOrEmpty(dto.Secret))
            webhook.Secret = dto.Secret;
        
        if (!string.IsNullOrEmpty(dto.Events))
            webhook.Events = dto.Events;
        
        if (dto.IsActive.HasValue)
            webhook.IsActive = dto.IsActive.Value;
        
        if (dto.MaxRetries.HasValue)
            webhook.MaxRetries = dto.MaxRetries.Value;

        await repository.UpdateAsync(webhook);
        return mapper.Map<WebhookDto>(webhook);
    }

    public async Task DeleteWebhookAsync(int id)
    {
        var webhook = await repository.GetByIdAsync(id);
        if (webhook == null)
            throw new InvalidOperationException($"Webhook with ID {id} not found");

        webhook.IsActive = false;
        await repository.UpdateAsync(webhook);
    }

    public async Task<IEnumerable<WebhookEventDto>> GetDeliveryHistoryAsync(int webhookId, int take = 50)
    {
        var webhook = await repository.GetWithEventsAsync(webhookId, take);
        if (webhook == null)
            throw new InvalidOperationException($"Webhook with ID {webhookId} not found");

        return mapper.Map<IEnumerable<WebhookEventDto>>(webhook.Events_History);
    }

    public async Task<WebhookDto> TestWebhookAsync(int id)
    {
        var webhook = await repository.GetByIdAsync(id);
        if (webhook == null)
            throw new InvalidOperationException($"Webhook with ID {id} not found");

        var testPayload = new
        {
            Event = "webhook.test",
            Timestamp = DateTime.UtcNow,
            Message = "This is a test webhook delivery"
        };

        await webhookDeliveryService.TriggerDeploymentWebhooksAsync(webhook.ApplicationId, "webhook.test", testPayload);

        return mapper.Map<WebhookDto>(webhook);
    }
}
