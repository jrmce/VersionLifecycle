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
public class VersionService : IVersionService
{
    private readonly VersionRepository _repository;
    private readonly IMapper _mapper;
    private readonly ITenantContext _tenantContext;

    public VersionService(VersionRepository repository, IMapper mapper, ITenantContext tenantContext)
    {
        _repository = repository;
        _mapper = mapper;
        _tenantContext = tenantContext;
    }

    public async Task<IEnumerable<VersionDto>> GetVersionsByApplicationAsync(int applicationId)
    {
        var versions = await _repository.GetByApplicationIdAsync(applicationId);
        return _mapper.Map<IEnumerable<VersionDto>>(versions);
    }

    public async Task<VersionDto?> GetVersionAsync(int id)
    {
        var version = await _repository.GetByIdAsync(id);
        return version == null ? null : _mapper.Map<VersionDto>(version);
    }

    public async Task<VersionDto> CreateVersionAsync(CreateVersionDto dto)
    {
        var version = new Version
        {
            ApplicationId = dto.ApplicationId,
            VersionNumber = dto.VersionNumber,
            ReleaseNotes = dto.ReleaseNotes,
            Status = VersionLifecycle.Core.Enums.VersionStatus.Draft,
            TenantId = _tenantContext.CurrentTenantId,
            CreatedBy = _tenantContext.CurrentUserId ?? "system"
        };

        await _repository.AddAsync(version);
        return _mapper.Map<VersionDto>(version);
    }

    public async Task<VersionDto> UpdateVersionAsync(int id, UpdateVersionDto dto)
    {
        var version = await _repository.GetByIdAsync(id);
        if (version == null)
            throw new InvalidOperationException($"Version with ID {id} not found");

        version.VersionNumber = dto.VersionNumber ?? version.VersionNumber;
        version.ReleaseNotes = dto.ReleaseNotes ?? version.ReleaseNotes;
        if (dto.Status.HasValue)
        {
            version.Status = dto.Status.Value;
        }

        await _repository.UpdateAsync(version);
        return _mapper.Map<VersionDto>(version);
    }

    public async Task DeleteVersionAsync(int id)
    {
        var deleted = await _repository.DeleteAsync(id);
        if (!deleted)
            throw new InvalidOperationException($"Version with ID {id} not found");
    }
}
