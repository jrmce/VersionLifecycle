namespace VersionLifecycle.Infrastructure.Services;

using System.Linq;
using AutoMapper;
using VersionLifecycle.Application.DTOs;
using VersionLifecycle.Application.Services;
using VersionLifecycle.Core.Entities;
using VersionLifecycle.Core.Interfaces;
using VersionLifecycle.Infrastructure.Repositories;

/// <summary>
/// Service for managing deployments.
/// </summary>
public class DeploymentService : IDeploymentService
{
    private readonly DeploymentRepository _deploymentRepository;
    private readonly ApplicationRepository _applicationRepository;
    private readonly VersionRepository _versionRepository;
    private readonly EnvironmentRepository _environmentRepository;
    private readonly IMapper _mapper;
    private readonly ITenantContext _tenantContext;

    public DeploymentService(
        DeploymentRepository deploymentRepository,
        ApplicationRepository applicationRepository,
        VersionRepository versionRepository,
        EnvironmentRepository environmentRepository,
        IMapper mapper,
        ITenantContext tenantContext)
    {
        _deploymentRepository = deploymentRepository;
        _applicationRepository = applicationRepository;
        _versionRepository = versionRepository;
        _environmentRepository = environmentRepository;
        _mapper = mapper;
        _tenantContext = tenantContext;
    }

    public async Task<PaginatedResponse<DeploymentDto>> GetDeploymentsAsync(int skip = 0, int take = 25, string? statusFilter = null)
    {
        var deployments = await _deploymentRepository.GetAllWithNavigationAsync();
        
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
        var deployment = await _deploymentRepository.GetByIdAsync(id);
        return deployment == null ? null : _mapper.Map<DeploymentDto>(deployment);
    }

    public async Task<DeploymentDto> CreatePendingDeploymentAsync(CreatePendingDeploymentDto dto)
    {
        var application = await _applicationRepository.GetByIdAsync(dto.ApplicationId);
        if (application == null)
            throw new InvalidOperationException($"Application with ID {dto.ApplicationId} not found");

        var version = await _versionRepository.GetByIdAsync(dto.VersionId);
        if (version == null || version.ApplicationId != dto.ApplicationId)
            throw new InvalidOperationException("Version does not belong to the selected application");

        var environment = await _environmentRepository.GetByIdAsync(dto.EnvironmentId);
        if (environment == null)
            throw new InvalidOperationException("Environment not found");

        var deployment = new Deployment
        {
            ApplicationId = dto.ApplicationId,
            VersionId = dto.VersionId,
            EnvironmentId = dto.EnvironmentId,
            Status = VersionLifecycle.Core.Enums.DeploymentStatus.Pending,
            Notes = dto.Notes,
            TenantId = _tenantContext.CurrentTenantId,
            CreatedBy = _tenantContext.CurrentUserId ?? "system"
        };

        await _deploymentRepository.AddAsync(deployment);
        return _mapper.Map<DeploymentDto>(deployment);
    }

    public async Task<DeploymentDto> ConfirmDeploymentAsync(int id, ConfirmDeploymentDto dto)
    {
        var deployment = await _deploymentRepository.GetByIdAsync(id);
        if (deployment == null)
            throw new InvalidOperationException($"Deployment with ID {id} not found");

        if (deployment.Status != VersionLifecycle.Core.Enums.DeploymentStatus.Pending)
            throw new InvalidOperationException("Only pending deployments can be confirmed");

        deployment.Status = VersionLifecycle.Core.Enums.DeploymentStatus.InProgress;
        deployment.DeployedAt = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(dto.ConfirmationNotes))
            deployment.Notes = dto.ConfirmationNotes;

        await _deploymentRepository.UpdateAsync(deployment);
        return _mapper.Map<DeploymentDto>(deployment);
    }

    public async Task<DeploymentDto> PromoteDeploymentAsync(int id, PromoteDeploymentDto dto)
    {
        var source = await _deploymentRepository.GetByIdAsync(id);
        if (source == null)
            throw new InvalidOperationException($"Deployment with ID {id} not found");

        // Only allow promotion from a successful deployment.
        if (source.Status != VersionLifecycle.Core.Enums.DeploymentStatus.Success)
            throw new InvalidOperationException("Only successful deployments can be promoted");

        // Determine the next environment in order relative to the source deployment's environment.
        var environments = (await _environmentRepository.GetAllAsync()).OrderBy(e => e.Order).ToList();
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
            Status = VersionLifecycle.Core.Enums.DeploymentStatus.InProgress,
            DeployedAt = DateTime.UtcNow,
            DeployedBy = _tenantContext.CurrentUserId ?? "system",
            Notes = dto.Notes,
            TenantId = _tenantContext.CurrentTenantId,
            CreatedBy = _tenantContext.CurrentUserId ?? "system"
        };

        await _deploymentRepository.AddAsync(promotion);
        return _mapper.Map<DeploymentDto>(promotion);
    }

    public async Task<DeploymentDto> UpdateDeploymentStatusAsync(int id, UpdateDeploymentStatusDto dto)
    {
        var deployment = await _deploymentRepository.GetByIdAsync(id);
        if (deployment == null)
            throw new InvalidOperationException($"Deployment with ID {id} not found");

        var current = deployment.Status;
        var target = dto.Status;

        if (current is VersionLifecycle.Core.Enums.DeploymentStatus.Success
            or VersionLifecycle.Core.Enums.DeploymentStatus.Failed
            or VersionLifecycle.Core.Enums.DeploymentStatus.Cancelled)
        {
            throw new InvalidOperationException("Completed deployments cannot be updated");
        }

        if (target == VersionLifecycle.Core.Enums.DeploymentStatus.Pending)
            throw new InvalidOperationException("Cannot revert a deployment to Pending");

        if (target == VersionLifecycle.Core.Enums.DeploymentStatus.InProgress && current != VersionLifecycle.Core.Enums.DeploymentStatus.Pending)
            throw new InvalidOperationException("Only pending deployments can be moved to InProgress");

        if ((target == VersionLifecycle.Core.Enums.DeploymentStatus.Success
                || target == VersionLifecycle.Core.Enums.DeploymentStatus.Failed
                || target == VersionLifecycle.Core.Enums.DeploymentStatus.Cancelled)
            && current == VersionLifecycle.Core.Enums.DeploymentStatus.Pending
            && target != VersionLifecycle.Core.Enums.DeploymentStatus.Cancelled)
        {
            throw new InvalidOperationException("Pending deployments must be in progress before completion");
        }

        deployment.Status = target;

        if (!string.IsNullOrWhiteSpace(dto.Notes))
        {
            deployment.Notes = dto.Notes;
        }

        if (target == VersionLifecycle.Core.Enums.DeploymentStatus.InProgress)
        {
            deployment.DeployedAt = deployment.DeployedAt == default ? DateTime.UtcNow : deployment.DeployedAt;
            deployment.DeployedBy = _tenantContext.CurrentUserId ?? deployment.DeployedBy ?? "system";
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

        await _deploymentRepository.UpdateAsync(deployment);
        return _mapper.Map<DeploymentDto>(deployment);
    }

    public async Task<IEnumerable<DeploymentEventDto>> GetDeploymentHistoryAsync(int deploymentId)
    {
        var deployment = await _deploymentRepository.GetWithEventsAsync(deploymentId);
        if (deployment == null)
            throw new InvalidOperationException($"Deployment with ID {deploymentId} not found");

        return _mapper.Map<IEnumerable<DeploymentEventDto>>(deployment.Events);
    }
}
