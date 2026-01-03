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

    public async Task<ApplicationDto?> GetApplicationAsync(Guid externalId)
    {
        var application = await repository.GetByExternalIdAsync(externalId);
        return application == null ? null : mapper.Map<ApplicationDto>(application);
    }

    public async Task<ApplicationDto> CreateApplicationAsync(CreateApplicationDto dto)
    {
        var currentTenantId = tenantContext.CurrentTenantId;
        
        if (string.IsNullOrEmpty(currentTenantId))
        {
            throw new InvalidOperationException("Tenant context is not set. Current tenant ID is null or empty.");
        }

        var application = new Application
        {
            Name = dto.Name,
            Description = dto.Description,
            RepositoryUrl = dto.RepositoryUrl,
            TenantId = currentTenantId,
            CreatedBy = tenantContext.CurrentUserId ?? "system"
        };

        await repository.AddAsync(application);
        return mapper.Map<ApplicationDto>(application);
    }

    public async Task<ApplicationDto> UpdateApplicationAsync(Guid externalId, UpdateApplicationDto dto)
    {
        var application = await repository.GetByExternalIdAsync(externalId);
        if (application == null)
            throw new InvalidOperationException($"Application with ID {externalId} not found");

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

    public async Task DeleteApplicationAsync(Guid externalId)
    {
        var deleted = await repository.DeleteAsync(externalId);
        if (!deleted)
            throw new InvalidOperationException($"Application with ID {externalId} not found");
    }
}
