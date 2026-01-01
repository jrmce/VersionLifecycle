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
public class EnvironmentService(EnvironmentRepository repository, DeploymentRepository deploymentRepository, IMapper mapper, ITenantContext tenantContext) : IEnvironmentService
{
    public async Task<IEnumerable<EnvironmentDto>> GetEnvironmentsAsync()
    {
        var environments = await repository.GetAllAsync();
        return mapper.Map<IEnumerable<EnvironmentDto>>(environments);
    }

    public async Task<EnvironmentDto?> GetEnvironmentAsync(int id)
    {
        var environment = await repository.GetByIdAsync(id);
        return environment == null ? null : mapper.Map<EnvironmentDto>(environment);
    }

    public async Task<EnvironmentDto> CreateEnvironmentAsync(CreateEnvironmentDto dto)
    {
        var environment = new Environment
        {
            Name = dto.Name,
            Description = dto.Description,
            Order = dto.Order,
            Config = dto.Config,
            TenantId = tenantContext.CurrentTenantId,
            CreatedBy = tenantContext.CurrentUserId ?? "system"
        };

        await repository.AddAsync(environment);
        return mapper.Map<EnvironmentDto>(environment);
    }

    public async Task<EnvironmentDto> UpdateEnvironmentAsync(int id, UpdateEnvironmentDto dto)
    {
        var environment = await repository.GetByIdAsync(id);
        if (environment == null)
            throw new InvalidOperationException($"Environment with ID {id} not found");

        environment.Name = dto.Name ?? environment.Name;
        environment.Description = dto.Description ?? environment.Description;
        environment.Config = dto.Config ?? environment.Config;
        if (dto.Order.HasValue)
            environment.Order = dto.Order.Value;

        await repository.UpdateAsync(environment);
        return mapper.Map<EnvironmentDto>(environment);
    }

    public async Task DeleteEnvironmentAsync(int id)
    {
        var deleted = await repository.DeleteAsync(id);
        if (!deleted)
            throw new InvalidOperationException($"Environment with ID {id} not found");
    }

    public async Task<IEnumerable<EnvironmentDeploymentOverviewDto>> GetEnvironmentDeploymentOverviewAsync()
    {
        var environments = (await repository.GetAllAsync()).ToList();
        var deployments = await deploymentRepository.GetAllWithNavigationAsync();

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
