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
public class WebhookService : IWebhookService
{
    private readonly WebhookRepository _repository;
    private readonly IMapper _mapper;
    private readonly ITenantContext _tenantContext;

    public WebhookService(WebhookRepository repository, IMapper mapper, ITenantContext tenantContext)
    {
        _repository = repository;
        _mapper = mapper;
        _tenantContext = tenantContext;
    }

    public async Task<IEnumerable<WebhookDto>> GetWebhooksAsync(int applicationId)
    {
        var webhooks = await _repository.GetByApplicationIdAsync(applicationId);
        return _mapper.Map<IEnumerable<WebhookDto>>(webhooks);
    }

    public async Task<WebhookDto?> GetWebhookAsync(int id)
    {
        var webhook = await _repository.GetByIdAsync(id);
        return webhook == null ? null : _mapper.Map<WebhookDto>(webhook);
    }

    public async Task<WebhookDto> CreateWebhookAsync(CreateWebhookDto dto)
    {
        var webhook = new Webhook
        {
            ApplicationId = dto.ApplicationId,
            Url = dto.Url,
            Secret = dto.Secret,
            MaxRetries = dto.MaxRetries,
            IsActive = true,
            TenantId = _tenantContext.CurrentTenantId,
            CreatedBy = _tenantContext.CurrentUserId ?? "system"
        };

        await _repository.AddAsync(webhook);
        return _mapper.Map<WebhookDto>(webhook);
    }

    public async Task DeleteWebhookAsync(int id)
    {
        var webhook = await _repository.GetByIdAsync(id);
        if (webhook == null)
            throw new InvalidOperationException($"Webhook with ID {id} not found");

        webhook.IsActive = false;
        await _repository.UpdateAsync(webhook);
    }

    public async Task<IEnumerable<WebhookEventDto>> GetDeliveryHistoryAsync(int webhookId, int take = 50)
    {
        var webhook = await _repository.GetWithEventsAsync(webhookId, take);
        if (webhook == null)
            throw new InvalidOperationException($"Webhook with ID {webhookId} not found");

        return _mapper.Map<IEnumerable<WebhookEventDto>>(webhook.Events_History);
    }
}
