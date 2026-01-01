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
public class VersionService(VersionRepository repository, IMapper mapper, ITenantContext tenantContext) : IVersionService
{
    public async Task<IEnumerable<VersionDto>> GetVersionsByApplicationAsync(int applicationId)
    {
        var versions = await repository.GetByApplicationIdAsync(applicationId);
        return mapper.Map<IEnumerable<VersionDto>>(versions);
    }

    public async Task<VersionDto?> GetVersionAsync(int id)
    {
        var version = await repository.GetByIdAsync(id);
        return version == null ? null : mapper.Map<VersionDto>(version);
    }

    public async Task<VersionDto> CreateVersionAsync(CreateVersionDto dto)
    {
        var version = new Version
        {
            ApplicationId = dto.ApplicationId,
            VersionNumber = dto.VersionNumber,
            ReleaseNotes = dto.ReleaseNotes,
            Status = Core.Enums.VersionStatus.Draft,
            TenantId = tenantContext.CurrentTenantId,
            CreatedBy = tenantContext.CurrentUserId ?? "system"
        };

        await repository.AddAsync(version);
        return mapper.Map<VersionDto>(version);
    }

    public async Task<VersionDto> UpdateVersionAsync(int id, UpdateVersionDto dto)
    {
        var version = await repository.GetByIdAsync(id);
        if (version == null)
            throw new InvalidOperationException($"Version with ID {id} not found");

        version.VersionNumber = dto.VersionNumber ?? version.VersionNumber;
        version.ReleaseNotes = dto.ReleaseNotes ?? version.ReleaseNotes;
        if (dto.Status.HasValue)
        {
            version.Status = dto.Status.Value;
        }

        await repository.UpdateAsync(version);
        return mapper.Map<VersionDto>(version);
    }

    public async Task DeleteVersionAsync(int id)
    {
        var deleted = await repository.DeleteAsync(id);
        if (!deleted)
            throw new InvalidOperationException($"Version with ID {id} not found");
    }
}
