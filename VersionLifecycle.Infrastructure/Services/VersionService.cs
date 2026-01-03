namespace VersionLifecycle.Infrastructure.Services;

using AutoMapper;
using VersionLifecycle.Application.DTOs;
using VersionLifecycle.Application.Services;
using VersionLifecycle.Core.Entities;
using VersionLifecycle.Core.Interfaces;
using VersionLifecycle.Infrastructure.Repositories;

/// <summary>
/// Service for managing versions.
/// </summary>
public class VersionService(
    VersionRepository repository, 
    ApplicationRepository applicationRepository,
    IMapper mapper, 
    ITenantContext tenantContext) : IVersionService
{
    public async Task<IEnumerable<VersionDto>> GetVersionsByApplicationAsync(Guid applicationExternalId)
    {
        var application = await applicationRepository.GetByExternalIdAsync(applicationExternalId);
        if (application == null)
            throw new InvalidOperationException($"Application with ID {applicationExternalId} not found");

        var versions = await repository.GetByApplicationIdAsync(application.Id);
        return mapper.Map<IEnumerable<VersionDto>>(versions);
    }

    public async Task<VersionDto?> GetVersionAsync(Guid externalId)
    {
        var version = await repository.GetByExternalIdAsync(externalId);
        return version == null ? null : mapper.Map<VersionDto>(version);
    }

    public async Task<VersionDto> CreateVersionAsync(CreateVersionDto dto)
    {
        var application = await applicationRepository.GetByExternalIdAsync(dto.ApplicationId);
        if (application == null)
            throw new InvalidOperationException($"Application with ID {dto.ApplicationId} not found");

        var version = new Version
        {
            ApplicationId = application.Id,
            VersionNumber = dto.VersionNumber,
            ReleaseNotes = dto.ReleaseNotes,
            Status = Core.Enums.VersionStatus.Draft,
            TenantId = tenantContext.CurrentTenantId,
            CreatedBy = tenantContext.CurrentUserId ?? "system"
        };

        await repository.AddAsync(version);
        return mapper.Map<VersionDto>(version);
    }

    public async Task<VersionDto> UpdateVersionAsync(Guid externalId, UpdateVersionDto dto)
    {
        var version = await repository.GetByExternalIdAsync(externalId);
        if (version == null)
            throw new InvalidOperationException($"Version with ID {externalId} not found");

        version.VersionNumber = dto.VersionNumber ?? version.VersionNumber;
        version.ReleaseNotes = dto.ReleaseNotes ?? version.ReleaseNotes;
        if (dto.Status.HasValue)
        {
            version.Status = dto.Status.Value;
        }

        await repository.UpdateAsync(version);
        return mapper.Map<VersionDto>(version);
    }

    public async Task DeleteVersionAsync(Guid externalId)
    {
        var deleted = await repository.DeleteAsync(externalId);
        if (!deleted)
            throw new InvalidOperationException($"Version with ID {externalId} not found");
    }
}
