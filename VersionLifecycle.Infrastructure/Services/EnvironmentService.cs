namespace VersionLifecycle.Infrastructure.Services;

using System.Linq;
using AutoMapper;
using VersionLifecycle.Application.DTOs;
using VersionLifecycle.Application.Services;
using VersionLifecycle.Core.Entities;
using VersionLifecycle.Core.Interfaces;
using VersionLifecycle.Infrastructure.Repositories;

/// <summary>
/// Service for managing environments.
/// </summary>
public class EnvironmentService : IEnvironmentService
{
    private readonly EnvironmentRepository _repository;
    private readonly DeploymentRepository _deploymentRepository;
    private readonly IMapper _mapper;
    private readonly ITenantContext _tenantContext;

    public EnvironmentService(EnvironmentRepository repository, DeploymentRepository deploymentRepository, IMapper mapper, ITenantContext tenantContext)
    {
        _repository = repository;
        _deploymentRepository = deploymentRepository;
        _mapper = mapper;
        _tenantContext = tenantContext;
    }

    public async Task<IEnumerable<EnvironmentDto>> GetEnvironmentsAsync()
    {
        var environments = await _repository.GetAllAsync();
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
            Name = dto.Name,
            Description = dto.Description,
            Order = dto.Order,
            Config = dto.Config,
            TenantId = _tenantContext.CurrentTenantId,
            CreatedBy = _tenantContext.CurrentUserId ?? "system"
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
        environment.Description = dto.Description ?? environment.Description;
        environment.Config = dto.Config ?? environment.Config;
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

    public async Task<IEnumerable<EnvironmentDeploymentOverviewDto>> GetEnvironmentDeploymentOverviewAsync()
    {
        var environments = (await _repository.GetAllAsync()).ToList();
        var deployments = await _deploymentRepository.GetAllWithNavigationAsync();

        var latestByEnvironment = deployments
            .GroupBy(d => new { d.EnvironmentId, d.ApplicationId })
            .Select(group => group
                .OrderByDescending(d => d.DeployedAt == default ? d.CreatedAt : d.DeployedAt)
                .First())
            .ToList();

        var overview = new List<EnvironmentDeploymentOverviewDto>();

        foreach (var environment in environments)
        {
            var environmentDeployments = latestByEnvironment
                .Where(d => d.EnvironmentId == environment.Id)
                .Select(d => new EnvironmentDeploymentStatusDto
                {
                    DeploymentId = d.Id,
                    ApplicationId = d.ApplicationId,
                    ApplicationName = d.Application?.Name ?? $"App {d.ApplicationId}",
                    VersionId = d.VersionId,
                    VersionNumber = d.Version?.VersionNumber ?? $"v{d.VersionId}",
                    Status = d.Status.ToString(),
                    DeployedAt = d.DeployedAt,
                    CompletedAt = d.CompletedAt
                })
                .OrderBy(d => d.ApplicationName)
                .ToList();

            overview.Add(new EnvironmentDeploymentOverviewDto
            {
                EnvironmentId = environment.Id,
                EnvironmentName = environment.Name,
                Order = environment.Order,
                Description = environment.Description,
                Deployments = environmentDeployments
            });
        }

        return overview;
    }
}
