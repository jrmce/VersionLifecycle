namespace VersionLifecycle.Web.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VersionLifecycle.Application.DTOs;
using VersionLifecycle.Application.Services;
using VersionLifecycle.Web.Models;

/// <summary>
/// Tenants controller for managing multi-tenant operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TenantsController : ControllerBase
{
    private readonly ITenantService _tenantService;

    public TenantsController(ITenantService tenantService)
    {
        _tenantService = tenantService;
    }

    /// <summary>
    /// Gets active tenants for registration dropdowns.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<TenantLookupDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTenants([FromQuery] bool activeOnly = true)
    {
        var tenants = await _tenantService.GetTenantLookupsAsync(activeOnly);
        return Ok(tenants);
    }

    /// <summary>
    /// Gets all tenants with full details (SuperAdmin only).
    /// </summary>
    [HttpGet("all")]
    [Authorize(Policy = "SuperAdminOnly")]
    [ProducesResponseType(typeof(IEnumerable<TenantDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllTenants([FromQuery] bool activeOnly = false)
    {
        var tenants = await _tenantService.GetAllTenantsAsync(activeOnly);
        return Ok(tenants);
    }

    /// <summary>
    /// Gets a specific tenant.
    /// </summary>
    [HttpGet("{tenantId}")]
    [Authorize(Policy = "SuperAdminOnly")]
    [ProducesResponseType(typeof(TenantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTenant(string tenantId)
    {
        var result = await _tenantService.GetTenantAsync(tenantId);
        if (result == null)
            return NotFound(new ErrorResponse { Code = "NOT_FOUND", Message = $"Tenant with ID {tenantId} not found", TraceId = HttpContext.TraceIdentifier });

        return Ok(result);
    }

    /// <summary>
    /// Creates a new tenant.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "SuperAdminOnly")]
    [ProducesResponseType(typeof(TenantDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTenant([FromBody] CreateTenantDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ErrorResponse { Code = "INVALID_REQUEST", Message = "Invalid request", TraceId = HttpContext.TraceIdentifier });

        var result = await _tenantService.CreateTenantAsync(request);
        return CreatedAtAction(nameof(GetTenant), new { tenantId = result.Id }, result);
    }

    /// <summary>
    /// Updates an existing tenant.
    /// </summary>
    [HttpPut("{tenantId}")]
    [Authorize(Policy = "SuperAdminOnly")]
    [ProducesResponseType(typeof(TenantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTenant(string tenantId, [FromBody] CreateTenantDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ErrorResponse { Code = "INVALID_REQUEST", Message = "Invalid request", TraceId = HttpContext.TraceIdentifier });

        try
        {
            var result = await _tenantService.UpdateTenantAsync(tenantId, request);
            return Ok(result);
        }
        catch (InvalidOperationException)
        {
            return NotFound(new ErrorResponse { Code = "NOT_FOUND", Message = $"Tenant with ID {tenantId} not found", TraceId = HttpContext.TraceIdentifier });
        }
    }

    /// <summary>
    /// Gets statistics for a specific tenant.
    /// </summary>
    [HttpGet("{tenantId}/stats")]
    [Authorize(Policy = "SuperAdminOnly")]
    [ProducesResponseType(typeof(TenantStatsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTenantStats(string tenantId)
    {
        var stats = await _tenantService.GetTenantStatsAsync(tenantId);
        return Ok(stats);
    }
}
