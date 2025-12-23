namespace VersionLifecycle.Infrastructure.Services;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using VersionLifecycle.Application.DTOs;
using VersionLifecycle.Application.Services;
using VersionLifecycle.Core.Entities;
using VersionLifecycle.Core.Interfaces;
using VersionLifecycle.Infrastructure.Data;
using VersionLifecycle.Infrastructure.Repositories;

/// <summary>
/// Service for managing applications.
/// </summary>
public class ApplicationService : IApplicationService
{
    private readonly ApplicationRepository _repository;
    private readonly IMapper _mapper;
    private readonly ITenantContext _tenantContext;

    public ApplicationService(ApplicationRepository repository, IMapper mapper, ITenantContext tenantContext)
    {
        _repository = repository;
        _mapper = mapper;
        _tenantContext = tenantContext;
    }

    public async Task<PaginatedResponse<ApplicationDto>> GetApplicationsAsync(int skip = 0, int take = 25)
    {
        var applications = await _repository.GetAllAsync();
        var total = applications.Count();
        var items = applications.Skip(skip).Take(take).ToList();

        return new PaginatedResponse<ApplicationDto>
        {
            Items = _mapper.Map<List<ApplicationDto>>(items),
            TotalCount = total,
            Skip = skip,
            Take = take
        };
    }

    public async Task<ApplicationDto?> GetApplicationAsync(int id)
    {
        var application = await _repository.GetByIdAsync(id);
        return application == null ? null : _mapper.Map<ApplicationDto>(application);
    }

    public async Task<ApplicationDto> CreateApplicationAsync(CreateApplicationDto dto)
    {
        var application = new Application
        {
            Name = dto.Name,
            Description = dto.Description,
            RepositoryUrl = dto.RepositoryUrl,
            TenantId = _tenantContext.CurrentTenantId,
            CreatedBy = _tenantContext.CurrentUserId
        };

        await _repository.AddAsync(application);
        return _mapper.Map<ApplicationDto>(application);
    }

    public async Task<ApplicationDto> UpdateApplicationAsync(int id, UpdateApplicationDto dto)
    {
        var application = await _repository.GetByIdAsync(id);
        if (application == null)
            throw new InvalidOperationException($"Application with ID {id} not found");

        // Update only the properties that are provided
        if (dto.Name != null)
            application.Name = dto.Name;
        if (dto.Description != null)
            application.Description = dto.Description;
        if (dto.RepositoryUrl != null)
            application.RepositoryUrl = dto.RepositoryUrl;

        // Set audit properties
        application.ModifiedBy = _tenantContext.CurrentUserId;

        await _repository.UpdateAsync(application);
        return _mapper.Map<ApplicationDto>(application);
    }

    public async Task DeleteApplicationAsync(int id)
    {
        var deleted = await _repository.DeleteAsync(id);
        if (!deleted)
            throw new InvalidOperationException($"Application with ID {id} not found");
    }
}

/// <summary>
/// Service for managing versions.
/// </summary>
public class VersionService : IVersionService
{
    private readonly VersionRepository _repository;
    private readonly IMapper _mapper;
    private readonly ITenantContext _tenantContext;

    public VersionService(VersionRepository repository, IMapper mapper, ITenantContext tenantContext)
    {
        _repository = repository;
        _mapper = mapper;
        _tenantContext = tenantContext;
    }

    public async Task<IEnumerable<VersionDto>> GetVersionsByApplicationAsync(int applicationId)
    {
        var versions = await _repository.GetByApplicationIdAsync(applicationId);
        return _mapper.Map<IEnumerable<VersionDto>>(versions);
    }

    public async Task<VersionDto?> GetVersionAsync(int id)
    {
        var version = await _repository.GetByIdAsync(id);
        return version == null ? null : _mapper.Map<VersionDto>(version);
    }

    public async Task<VersionDto> CreateVersionAsync(CreateVersionDto dto)
    {
        var version = new Version
        {
            ApplicationId = dto.ApplicationId,
            VersionNumber = dto.VersionNumber,
            ReleaseNotes = dto.ReleaseNotes,
            Status = Core.Enums.VersionStatus.Draft,
            TenantId = _tenantContext.CurrentTenantId,
            CreatedBy = _tenantContext.CurrentUserId
        };

        await _repository.AddAsync(version);
        return _mapper.Map<VersionDto>(version);
    }

    public async Task<VersionDto> UpdateVersionAsync(int id, UpdateVersionDto dto)
    {
        var version = await _repository.GetByIdAsync(id);
        if (version == null)
            throw new InvalidOperationException($"Version with ID {id} not found");

        version.VersionNumber = dto.VersionNumber ?? version.VersionNumber;
        version.ReleaseNotes = dto.ReleaseNotes ?? version.ReleaseNotes;
        if (dto.Status.HasValue)
        {
            version.Status = dto.Status.Value;
        }

        await _repository.UpdateAsync(version);
        return _mapper.Map<VersionDto>(version);
    }

    public async Task DeleteVersionAsync(int id)
    {
        var deleted = await _repository.DeleteAsync(id);
        if (!deleted)
            throw new InvalidOperationException($"Version with ID {id} not found");
    }
}

/// <summary>
/// Service for managing deployments.
/// </summary>
public class DeploymentService : IDeploymentService
{
    private readonly DeploymentRepository _repository;
    private readonly IMapper _mapper;
    private readonly ITenantContext _tenantContext;

    public DeploymentService(DeploymentRepository repository, IMapper mapper, ITenantContext tenantContext)
    {
        _repository = repository;
        _mapper = mapper;
        _tenantContext = tenantContext;
    }

    public async Task<PaginatedResponse<DeploymentDto>> GetDeploymentsAsync(int skip = 0, int take = 25, string? statusFilter = null)
    {
        var deployments = await _repository.GetAllAsync();
        
        if (!string.IsNullOrEmpty(statusFilter))
        {
            deployments = deployments.Where(d => d.Status.ToString().Equals(statusFilter, StringComparison.OrdinalIgnoreCase));
        }
        var total = deployments.Count();
        var items = deployments.Skip(skip).Take(take).ToList();

        return new PaginatedResponse<DeploymentDto>
        {
            Items = _mapper.Map<List<DeploymentDto>>(items),
            TotalCount = total,
            Skip = skip,
            Take = take
        };
    }

    public async Task<DeploymentDto?> GetDeploymentAsync(int id)
    {
        var deployment = await _repository.GetByIdAsync(id);
        return deployment == null ? null : _mapper.Map<DeploymentDto>(deployment);
    }

    public async Task<DeploymentDto> CreatePendingDeploymentAsync(CreatePendingDeploymentDto dto)
    {
        var deployment = new Deployment
        {
            ApplicationId = dto.ApplicationId,
            VersionId = dto.VersionId,
            EnvironmentId = dto.EnvironmentId,
            Status = Core.Enums.DeploymentStatus.Pending,
            Notes = dto.Notes,
            TenantId = _tenantContext.CurrentTenantId,
            CreatedBy = _tenantContext.CurrentUserId
        };

        await _repository.AddAsync(deployment);
        return _mapper.Map<DeploymentDto>(deployment);
    }

    public async Task<DeploymentDto> ConfirmDeploymentAsync(int id, ConfirmDeploymentDto dto)
    {
        var deployment = await _repository.GetByIdAsync(id);
        if (deployment == null)
            throw new InvalidOperationException($"Deployment with ID {id} not found");

        if (deployment.Status != Core.Enums.DeploymentStatus.Pending)
            throw new InvalidOperationException("Only pending deployments can be confirmed");

        deployment.Status = Core.Enums.DeploymentStatus.InProgress;
        deployment.DeployedAt = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(dto.ConfirmationNotes))
            deployment.Notes = dto.ConfirmationNotes;

        await _repository.UpdateAsync(deployment);
        return _mapper.Map<DeploymentDto>(deployment);
    }

    public async Task<IEnumerable<DeploymentEventDto>> GetDeploymentHistoryAsync(int deploymentId)
    {
        var deployment = await _repository.GetWithEventsAsync(deploymentId);
        if (deployment == null)
            throw new InvalidOperationException($"Deployment with ID {deploymentId} not found");

        return _mapper.Map<IEnumerable<DeploymentEventDto>>(deployment.Events);
    }
}

/// <summary>
/// Service for managing environments.
/// </summary>
public class EnvironmentService : IEnvironmentService
{
    private readonly EnvironmentRepository _repository;
    private readonly IMapper _mapper;
    private readonly ITenantContext _tenantContext;

    public EnvironmentService(EnvironmentRepository repository, IMapper mapper, ITenantContext tenantContext)
    {
        _repository = repository;
        _mapper = mapper;
        _tenantContext = tenantContext;
    }

    public async Task<IEnumerable<EnvironmentDto>> GetEnvironmentsByApplicationAsync(int applicationId)
    {
        var environments = await _repository.GetByApplicationIdAsync(applicationId);
        return _mapper.Map<IEnumerable<EnvironmentDto>>(environments);
    }

    public async Task<EnvironmentDto?> GetEnvironmentAsync(int id)
    {
        var environment = await _repository.GetByIdAsync(id);
        return environment == null ? null : _mapper.Map<EnvironmentDto>(environment);
    }

    public async Task<EnvironmentDto> CreateEnvironmentAsync(CreateEnvironmentDto dto)
    {
        var environment = new Environment
        {
            ApplicationId = dto.ApplicationId,
            Name = dto.Name,
            Order = dto.Order,
            TenantId = _tenantContext.CurrentTenantId,
            CreatedBy = _tenantContext.CurrentUserId
        };

        await _repository.AddAsync(environment);
        return _mapper.Map<EnvironmentDto>(environment);
    }

    public async Task<EnvironmentDto> UpdateEnvironmentAsync(int id, UpdateEnvironmentDto dto)
    {
        var environment = await _repository.GetByIdAsync(id);
        if (environment == null)
            throw new InvalidOperationException($"Environment with ID {id} not found");

        environment.Name = dto.Name ?? environment.Name;
        if (dto.Order.HasValue)
            environment.Order = dto.Order.Value;

        await _repository.UpdateAsync(environment);
        return _mapper.Map<EnvironmentDto>(environment);
    }

    public async Task DeleteEnvironmentAsync(int id)
    {
        var deleted = await _repository.DeleteAsync(id);
        if (!deleted)
            throw new InvalidOperationException($"Environment with ID {id} not found");
    }
}

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
            CreatedBy = _tenantContext.CurrentUserId
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

/// <summary>
/// Service for managing tenants.
/// </summary>
public class TenantService : ITenantService
{
    private readonly TenantRepository _repository;
    private readonly IMapper _mapper;

    public TenantService(TenantRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<TenantDto?> GetTenantAsync(string tenantId)
    {
        var tenant = await _repository.GetByIdAsync(tenantId);
        return tenant == null ? null : _mapper.Map<TenantDto>(tenant);
    }

    public async Task<TenantDto> CreateTenantAsync(CreateTenantDto dto)
    {
        var tenant = new Tenant
        {
            Id = Guid.NewGuid().ToString(),
            Name = dto.Name,
            Description = dto.Description,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _repository.AddAsync(tenant);
        return _mapper.Map<TenantDto>(tenant);
    }

    public async Task<TenantDto> UpdateTenantAsync(string tenantId, CreateTenantDto dto)
    {
        var tenant = await _repository.GetByIdAsync(tenantId);
        if (tenant == null)
            throw new InvalidOperationException($"Tenant with ID {tenantId} not found");

        tenant.Name = dto.Name ?? tenant.Name;
        tenant.Description = dto.Description ?? tenant.Description;

        await _repository.UpdateAsync(tenant);
        return _mapper.Map<TenantDto>(tenant);
    }
}
