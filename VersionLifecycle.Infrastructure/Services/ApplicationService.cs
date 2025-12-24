namespace VersionLifecycle.Infrastructure.Services;

using AutoMapper;
using VersionLifecycle.Application.DTOs;
using VersionLifecycle.Application.Services;
using VersionLifecycle.Core.Entities;
using VersionLifecycle.Core.Interfaces;
using VersionLifecycle.Infrastructure.Repositories;

/// <summary>
/// Service for managing applications.
/// </summary>
public class ApplicationService : IApplicationService
{
    private readonly ApplicationRepository _repository;
    private readonly IMapper _mapper;
    private readonly ITenantContext _tenantContext;

    public ApplicationService(
        ApplicationRepository repository,
        IMapper mapper,
        ITenantContext tenantContext)
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
            CreatedBy = _tenantContext.CurrentUserId ?? "system"
        };

        await _repository.AddAsync(application);
        return _mapper.Map<ApplicationDto>(application);
    }

    public async Task<ApplicationDto> UpdateApplicationAsync(int id, UpdateApplicationDto dto)
    {
        var application = await _repository.GetByIdAsync(id);
        if (application == null)
            throw new InvalidOperationException($"Application with ID {id} not found");

        if (dto.Name != null)
            application.Name = dto.Name;
        if (dto.Description != null)
            application.Description = dto.Description;
        if (dto.RepositoryUrl != null)
            application.RepositoryUrl = dto.RepositoryUrl;

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
