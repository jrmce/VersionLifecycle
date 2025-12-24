namespace VersionLifecycle.Infrastructure.Services;

using AutoMapper;
using VersionLifecycle.Application.DTOs;
using VersionLifecycle.Application.Services;
using VersionLifecycle.Infrastructure.Repositories;

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
        var tenant = new VersionLifecycle.Core.Entities.Tenant
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
