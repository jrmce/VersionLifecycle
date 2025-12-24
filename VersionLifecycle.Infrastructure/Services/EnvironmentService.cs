namespace VersionLifecycle.Infrastructure.Services;

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
