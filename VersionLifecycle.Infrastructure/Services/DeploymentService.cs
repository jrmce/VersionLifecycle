namespace VersionLifecycle.Infrastructure.Services;

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

    public async Task<IEnumerable<DeploymentEventDto>> GetDeploymentHistoryAsync(int deploymentId)
    {
        var deployment = await _deploymentRepository.GetWithEventsAsync(deploymentId);
        if (deployment == null)
            throw new InvalidOperationException($"Deployment with ID {deploymentId} not found");

        return _mapper.Map<IEnumerable<DeploymentEventDto>>(deployment.Events);
    }
}
