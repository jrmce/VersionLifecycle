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
public class WebhookService(
    WebhookRepository repository, 
    ApplicationRepository applicationRepository,
    WebhookDeliveryService webhookDeliveryService, 
    IMapper mapper, 
    ITenantContext tenantContext) : IWebhookService
{
    public async Task<IEnumerable<WebhookDto>> GetWebhooksAsync(Guid applicationExternalId)
    {
        var application = await applicationRepository.GetByExternalIdAsync(applicationExternalId);
        if (application == null)
            throw new InvalidOperationException($"Application with ID {applicationExternalId} not found");

        var webhooks = await repository.GetByApplicationIdAsync(application.Id);
        return mapper.Map<IEnumerable<WebhookDto>>(webhooks);
    }

    public async Task<WebhookDto?> GetWebhookAsync(Guid externalId)
    {
        var webhook = await repository.GetByExternalIdAsync(externalId);
        return webhook == null ? null : mapper.Map<WebhookDto>(webhook);
    }

    public async Task<WebhookDto> CreateWebhookAsync(CreateWebhookDto dto)
    {
        var application = await applicationRepository.GetByExternalIdAsync(dto.ApplicationId);
        if (application == null)
            throw new InvalidOperationException($"Application with ID {dto.ApplicationId} not found");

        var webhook = new Webhook
        {
            ApplicationId = application.Id,
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

    public async Task<WebhookDto> UpdateWebhookAsync(Guid externalId, UpdateWebhookDto dto)
    {
        var webhook = await repository.GetByExternalIdAsync(externalId);
        if (webhook == null)
            throw new InvalidOperationException($"Webhook with ID {externalId} not found");

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

    public async Task DeleteWebhookAsync(Guid externalId)
    {
        var webhook = await repository.GetByExternalIdAsync(externalId);
        if (webhook == null)
            throw new InvalidOperationException($"Webhook with ID {externalId} not found");

        webhook.IsActive = false;
        await repository.UpdateAsync(webhook);
    }

    public async Task<IEnumerable<WebhookEventDto>> GetDeliveryHistoryAsync(Guid webhookExternalId, int take = 50)
    {
        var webhook = await repository.GetWithEventsAsync(webhookExternalId, take);
        if (webhook == null)
            throw new InvalidOperationException($"Webhook with ID {webhookExternalId} not found");

        return mapper.Map<IEnumerable<WebhookEventDto>>(webhook.Events_History);
    }

    public async Task<WebhookDto> TestWebhookAsync(Guid externalId)
    {
        var webhook = await repository.GetByExternalIdAsync(externalId);
        if (webhook == null)
            throw new InvalidOperationException($"Webhook with ID {externalId} not found");

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
