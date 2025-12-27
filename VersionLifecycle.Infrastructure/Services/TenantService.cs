namespace VersionLifecycle.Infrastructure.Services;

using AutoMapper;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using VersionLifecycle.Application.DTOs;
using VersionLifecycle.Application.Services;
using VersionLifecycle.Infrastructure.Data;
using VersionLifecycle.Infrastructure.Repositories;

/// <summary>
/// Service for managing tenants.
/// </summary>
public class TenantService : ITenantService
{
    private readonly TenantRepository _repository;
    private readonly IMapper _mapper;
    private readonly AppDbContext _context;

    public TenantService(TenantRepository repository, IMapper mapper, AppDbContext context)
    {
        _repository = repository;
        _mapper = mapper;
        _context = context;
    }

    public async Task<IEnumerable<TenantDto>> GetTenantsAsync(bool activeOnly = true)
    {
        var tenants = activeOnly
            ? await _repository.GetActiveAsync()
            : await _repository.GetAllAsync();

        return _mapper.Map<IEnumerable<TenantDto>>(tenants);
    }

    public async Task<IEnumerable<TenantDto>> GetAllTenantsAsync(bool activeOnly = false)
    {
        var tenants = activeOnly
            ? await _repository.GetActiveAsync()
            : await _repository.GetAllAsync();

        return _mapper.Map<IEnumerable<TenantDto>>(tenants);
    }

    public async Task<IEnumerable<TenantLookupDto>> GetTenantLookupsAsync(bool activeOnly = true)
    {
        var tenants = activeOnly
            ? await _repository.GetActiveAsync()
            : await _repository.GetAllAsync();

        return _mapper.Map<IEnumerable<TenantLookupDto>>(tenants);
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
            SubscriptionPlan = dto.SubscriptionPlan,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            Code = await GenerateUniqueCodeAsync()
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
        tenant.SubscriptionPlan = dto.SubscriptionPlan ?? tenant.SubscriptionPlan;

        await _repository.UpdateAsync(tenant);
        return _mapper.Map<TenantDto>(tenant);
    }

    public async Task<TenantStatsDto> GetTenantStatsAsync(string tenantId)
    {
        // Query user count from AspNetUserClaims
        var userCount = await _context.Set<Microsoft.AspNetCore.Identity.IdentityUserClaim<string>>()
            .CountAsync(c => c.ClaimType == "tenantId" && c.ClaimValue == tenantId);

        var applicationCount = await _context.Applications.CountAsync(a => a.TenantId == tenantId && !a.IsDeleted);
        var versionCount = await _context.Versions.CountAsync(v => v.TenantId == tenantId && !v.IsDeleted);
        var deploymentCount = await _context.Deployments.CountAsync(d => d.TenantId == tenantId && !d.IsDeleted);

        return new TenantStatsDto
        {
            TenantId = tenantId,
            UserCount = userCount,
            ApplicationCount = applicationCount,
            VersionCount = versionCount,
            DeploymentCount = deploymentCount
        };
    }

    private async Task<string> GenerateUniqueCodeAsync()
    {
        const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var random = new Random();

        string NextCode()
        {
            return new string(Enumerable.Range(0, 8).Select(_ => alphabet[random.Next(alphabet.Length)]).ToArray());
        }

        string code;
        do
        {
            code = NextCode();
        }
        while (await _repository.CodeExistsAsync(code));

        return code;
    }
}
