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
public class ApplicationService(
    ApplicationRepository repository,
    IMapper mapper,
    ITenantContext tenantContext) : IApplicationService
{
    public async Task<PaginatedResponse<ApplicationDto>> GetApplicationsAsync(int skip = 0, int take = 25)
    {
        var applications = await repository.GetAllAsync();
        var total = applications.Count();
        var items = applications.Skip(skip).Take(take).ToList();

        return new PaginatedResponse<ApplicationDto>
        {
            Items = mapper.Map<List<ApplicationDto>>(items),
            TotalCount = total,
            Skip = skip,
            Take = take
        };
    }

    public async Task<ApplicationDto?> GetApplicationAsync(int id)
    {
        var application = await repository.GetByIdAsync(id);
        return application == null ? null : mapper.Map<ApplicationDto>(application);
    }

    public async Task<ApplicationDto> CreateApplicationAsync(CreateApplicationDto dto)
    {
        var application = new Application
        {
            Name = dto.Name,
            Description = dto.Description,
            RepositoryUrl = dto.RepositoryUrl,
            TenantId = tenantContext.CurrentTenantId,
            CreatedBy = tenantContext.CurrentUserId ?? "system"
        };

        await repository.AddAsync(application);
        return mapper.Map<ApplicationDto>(application);
    }

    public async Task<ApplicationDto> UpdateApplicationAsync(int id, UpdateApplicationDto dto)
    {
        var application = await repository.GetByIdAsync(id);
        if (application == null)
            throw new InvalidOperationException($"Application with ID {id} not found");

        if (dto.Name != null)
            application.Name = dto.Name;
        if (dto.Description != null)
            application.Description = dto.Description;
        if (dto.RepositoryUrl != null)
            application.RepositoryUrl = dto.RepositoryUrl;

        application.ModifiedBy = tenantContext.CurrentUserId;

        await repository.UpdateAsync(application);
        return mapper.Map<ApplicationDto>(application);
    }

    public async Task DeleteApplicationAsync(int id)
    {
        var deleted = await repository.DeleteAsync(id);
        if (!deleted)
            throw new InvalidOperationException($"Application with ID {id} not found");
    }
}
