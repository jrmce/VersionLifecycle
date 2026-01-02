namespace VersionLifecycle.Infrastructure.Services;

using System.Linq;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VersionLifecycle.Application.DTOs;
using VersionLifecycle.Application.Services;
using VersionLifecycle.Core.Entities;
using VersionLifecycle.Core.Exceptions;
using VersionLifecycle.Core.Interfaces;
using VersionLifecycle.Infrastructure.Repositories;

/// <summary>
/// Service for managing deployments.
/// </summary>
public class DeploymentService(
    DeploymentRepository deploymentRepository,
    ApplicationRepository applicationRepository,
    VersionRepository versionRepository,
    EnvironmentRepository environmentRepository,
    IMapper mapper,
    ITenantContext tenantContext,
    IBackgroundTaskRunner backgroundTaskRunner) : IDeploymentService
{
    public async Task<PaginatedResponse<DeploymentDto>> GetDeploymentsAsync(int skip = 0, int take = 25, string? statusFilter = null)
    {
        var deployments = await deploymentRepository.GetAllWithNavigationAsync();
        
        if (!string.IsNullOrEmpty(statusFilter))
        {
            deployments = deployments.Where(d => d.Status.ToString().Equals(statusFilter, StringComparison.OrdinalIgnoreCase));
        }
        var total = deployments.Count();
        var items = deployments.Skip(skip).Take(take).ToList();

        return new PaginatedResponse<DeploymentDto>
        {
            Items = mapper.Map<List<DeploymentDto>>(items),
            TotalCount = total,
            Skip = skip,
            Take = take
        };
    }

    public async Task<DeploymentDto?> GetDeploymentAsync(int id)
    {
        var deployment = await deploymentRepository.GetByIdAsync(id);
        return deployment == null ? null : mapper.Map<DeploymentDto>(deployment);
    }

    public async Task<DeploymentDto> CreatePendingDeploymentAsync(CreatePendingDeploymentDto dto)
    {
        var application = await applicationRepository.GetByIdAsync(dto.ApplicationId);
        if (application == null)
            throw new InvalidOperationException($"Application with ID {dto.ApplicationId} not found");

        var version = await versionRepository.GetByIdAsync(dto.VersionId);
        if (version == null || version.ApplicationId != dto.ApplicationId)
            throw new InvalidOperationException("Version does not belong to the selected application");

        var environment = await environmentRepository.GetByIdAsync(dto.EnvironmentId);
        if (environment == null)
            throw new InvalidOperationException("Environment not found");

        // Check for active deployment with same application, version, and environment
        // Allow redeployment if previous deployment is completed (Success, Failed, or Cancelled)
        var existingDeployments = await deploymentRepository.GetAllWithNavigationAsync();
        var activeDeployment = existingDeployments.FirstOrDefault(d =>
            d.ApplicationId == dto.ApplicationId &&
            d.VersionId == dto.VersionId &&
            d.EnvironmentId == dto.EnvironmentId &&
            (d.Status == Core.Enums.DeploymentStatus.Pending || d.Status == Core.Enums.DeploymentStatus.InProgress));

        if (activeDeployment != null)
        {
            throw new DuplicateDeploymentException(
                $"An active deployment already exists for this version in this environment. " +
                $"Deployment ID: {activeDeployment.Id}, Status: {activeDeployment.Status}. " +
                $"Please wait for it to complete or cancel it before redeploying.");
        }

        var deployment = new Deployment
        {
            ApplicationId = dto.ApplicationId,
            VersionId = dto.VersionId,
            EnvironmentId = dto.EnvironmentId,
            Status = Core.Enums.DeploymentStatus.Pending,
            Notes = dto.Notes,
            TenantId = tenantContext.CurrentTenantId,
            CreatedBy = tenantContext.CurrentUserId ?? "system"
        };

        await deploymentRepository.AddAsync(deployment);
        return mapper.Map<DeploymentDto>(deployment);
    }

    public async Task<DeploymentDto> ConfirmDeploymentAsync(int id, ConfirmDeploymentDto dto)
    {
        var deployment = await deploymentRepository.GetByIdAsync(id);
        if (deployment == null)
            throw new InvalidOperationException($"Deployment with ID {id} not found");

        if (deployment.Status != Core.Enums.DeploymentStatus.Pending)
            throw new InvalidOperationException("Only pending deployments can be confirmed");

        deployment.Status = Core.Enums.DeploymentStatus.InProgress;
        deployment.DeployedAt = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(dto.ConfirmationNotes))
            deployment.Notes = dto.ConfirmationNotes;

        await deploymentRepository.UpdateAsync(deployment);
        return mapper.Map<DeploymentDto>(deployment);
    }

    public async Task<DeploymentDto> PromoteDeploymentAsync(int id, PromoteDeploymentDto dto)
    {
        var source = await deploymentRepository.GetByIdAsync(id);
        if (source == null)
            throw new InvalidOperationException($"Deployment with ID {id} not found");

        // Only allow promotion from a successful deployment.
        if (source.Status != Core.Enums.DeploymentStatus.Success)
            throw new InvalidOperationException("Only successful deployments can be promoted");

        // Determine the next environment in order relative to the source deployment's environment.
        var environments = (await environmentRepository.GetAllAsync()).OrderBy(e => e.Order).ToList();
        var sourceEnvironment = environments.FirstOrDefault(e => e.Id == source.EnvironmentId);
        if (sourceEnvironment == null)
            throw new InvalidOperationException("Source environment not found for deployment");

        var nextEnvironment = environments
            .Where(e => e.Order > sourceEnvironment.Order)
            .OrderBy(e => e.Order)
            .FirstOrDefault();

        if (nextEnvironment == null)
            throw new InvalidOperationException("No higher environment available to promote to");

        if (nextEnvironment.Id != dto.TargetEnvironmentId)
            throw new InvalidOperationException("Promotion target must be the immediate next environment");

        var promotion = new Deployment
        {
            ApplicationId = source.ApplicationId,
            VersionId = source.VersionId,
            EnvironmentId = nextEnvironment.Id,
            Status = Core.Enums.DeploymentStatus.InProgress,
            DeployedAt = DateTime.UtcNow,
            DeployedBy = tenantContext.CurrentUserId ?? "system",
            Notes = dto.Notes,
            TenantId = tenantContext.CurrentTenantId,
            CreatedBy = tenantContext.CurrentUserId ?? "system"
        };

        await deploymentRepository.AddAsync(promotion);
        return mapper.Map<DeploymentDto>(promotion);
    }

    public async Task<DeploymentDto> UpdateDeploymentStatusAsync(int id, UpdateDeploymentStatusDto dto)
    {
        var deployment = await deploymentRepository.GetByIdAsync(id) ?? throw new InvalidOperationException($"Deployment with ID {id} not found");
        var current = deployment.Status;
        var target = dto.Status;

        if (current is Core.Enums.DeploymentStatus.Success
            or Core.Enums.DeploymentStatus.Failed
            or Core.Enums.DeploymentStatus.Cancelled)
        {
            throw new InvalidOperationException("Completed deployments cannot be updated");
        }

        if (target == Core.Enums.DeploymentStatus.Pending)
            throw new InvalidOperationException("Cannot revert a deployment to Pending");

        if (target == Core.Enums.DeploymentStatus.InProgress && current != Core.Enums.DeploymentStatus.Pending)
            throw new InvalidOperationException("Only pending deployments can be moved to InProgress");

        if ((target == Core.Enums.DeploymentStatus.Success
                || target == Core.Enums.DeploymentStatus.Failed
                || target == Core.Enums.DeploymentStatus.Cancelled)
            && current == Core.Enums.DeploymentStatus.Pending
            && target != Core.Enums.DeploymentStatus.Cancelled)
        {
            throw new InvalidOperationException("Pending deployments must be in progress before completion");
        }

        deployment.Status = target;

        if (!string.IsNullOrWhiteSpace(dto.Notes))
        {
            deployment.Notes = dto.Notes;
        }

        if (target == Core.Enums.DeploymentStatus.InProgress)
        {
            deployment.DeployedAt = deployment.DeployedAt == default ? DateTime.UtcNow : deployment.DeployedAt;
            deployment.DeployedBy = tenantContext.CurrentUserId ?? deployment.DeployedBy ?? "system";
            deployment.CompletedAt = null;
            deployment.DurationMs = null;
        }
        else
        {
            deployment.CompletedAt = DateTime.UtcNow;
            deployment.DurationMs = dto.DurationMs ??
                (deployment.DeployedAt != default
                    ? (long?)(DateTime.UtcNow - deployment.DeployedAt).TotalMilliseconds
                    : null);
        }

        await deploymentRepository.UpdateAsync(deployment);
        
        // Trigger webhooks on status changes
        var eventType = target switch
        {
            Core.Enums.DeploymentStatus.InProgress => "deployment.started",
            Core.Enums.DeploymentStatus.Success => "deployment.completed",
            Core.Enums.DeploymentStatus.Failed => "deployment.failed",
            Core.Enums.DeploymentStatus.Cancelled => "deployment.cancelled",
            _ => null
        };

        if (eventType != null)
        {
            var deploymentDto = mapper.Map<DeploymentDto>(deployment);
            var application = await applicationRepository.GetByIdAsync(deployment.ApplicationId);
            var version = await versionRepository.GetByIdAsync(deployment.VersionId);
            var environment = await environmentRepository.GetByIdAsync(deployment.EnvironmentId);
            
            var payload = new
            {
                Event = eventType,
                Timestamp = DateTime.UtcNow,
                Deployment = deploymentDto,
                Application = application?.Name,
                Version = version?.VersionNumber,
                Environment = environment?.Name
            };

            // Queue webhook delivery as background task
            backgroundTaskRunner.QueueTask(async sp =>
            {
                var webhookService = sp.GetRequiredService<WebhookDeliveryService>();
                await webhookService.TriggerDeploymentWebhooksAsync(deployment.ApplicationId, eventType, payload);
            });
        }
        
        return mapper.Map<DeploymentDto>(deployment);
    }

    public async Task<IEnumerable<DeploymentEventDto>> GetDeploymentHistoryAsync(int deploymentId)
    {
        var deployment = await deploymentRepository.GetWithEventsAsync(deploymentId);
        if (deployment == null)
            throw new InvalidOperationException($"Deployment with ID {deploymentId} not found");

        return mapper.Map<IEnumerable<DeploymentEventDto>>(deployment.Events);
    }
}
