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
public class TenantService(TenantRepository repository, IMapper mapper, AppDbContext context) : ITenantService
{
    public async Task<IEnumerable<TenantDto>> GetTenantsAsync(bool activeOnly = true)
    {
        var tenants = activeOnly
            ? await repository.GetActiveAsync()
            : await repository.GetAllAsync();

        return mapper.Map<IEnumerable<TenantDto>>(tenants);
    }

    public async Task<IEnumerable<TenantDto>> GetAllTenantsAsync(bool activeOnly = false)
    {
        var tenants = activeOnly
            ? await repository.GetActiveAsync()
            : await repository.GetAllAsync();

        return mapper.Map<IEnumerable<TenantDto>>(tenants);
    }

    public async Task<IEnumerable<TenantLookupDto>> GetTenantLookupsAsync(bool activeOnly = true)
    {
        var tenants = activeOnly
            ? await repository.GetActiveAsync()
            : await repository.GetAllAsync();

        return mapper.Map<IEnumerable<TenantLookupDto>>(tenants);
    }

    public async Task<TenantDto?> GetTenantAsync(string tenantId)
    {
        var tenant = await repository.GetByIdAsync(tenantId);
        return tenant == null ? null : mapper.Map<TenantDto>(tenant);
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

        await repository.AddAsync(tenant);
        return mapper.Map<TenantDto>(tenant);
    }

    public async Task<TenantDto> UpdateTenantAsync(string tenantId, CreateTenantDto dto)
    {
        var tenant = await repository.GetByIdAsync(tenantId);
        if (tenant == null)
            throw new InvalidOperationException($"Tenant with ID {tenantId} not found");

        tenant.Name = dto.Name ?? tenant.Name;
        tenant.Description = dto.Description ?? tenant.Description;
        tenant.SubscriptionPlan = dto.SubscriptionPlan ?? tenant.SubscriptionPlan;

        await repository.UpdateAsync(tenant);
        return mapper.Map<TenantDto>(tenant);
    }

    public async Task<TenantStatsDto> GetTenantStatsAsync(string tenantId)
    {
        // Query user count from AspNetUserClaims
        var userCount = await context.Set<Microsoft.AspNetCore.Identity.IdentityUserClaim<string>>()
            .CountAsync(c => c.ClaimType == "tenantId" && c.ClaimValue == tenantId);

        var applicationCount = await context.Applications.CountAsync(a => a.TenantId == tenantId && !a.IsDeleted);
        var versionCount = await context.Versions.CountAsync(v => v.TenantId == tenantId && !v.IsDeleted);
        var deploymentCount = await context.Deployments.CountAsync(d => d.TenantId == tenantId && !d.IsDeleted);

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
        while (await repository.CodeExistsAsync(code));

        return code;
    }
}
