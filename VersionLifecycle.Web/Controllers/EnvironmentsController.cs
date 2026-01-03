namespace VersionLifecycle.Web.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VersionLifecycle.Application.DTOs;
using VersionLifecycle.Application.Services;
using VersionLifecycle.Web.Models;

/// <summary>
/// Environments controller for managing deployment environments.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EnvironmentsController(IEnvironmentService environmentService) : ControllerBase
{

    /// <summary>
    /// Gets all environments for the current tenant.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EnvironmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEnvironments()
    {
        var result = await environmentService.GetEnvironmentsAsync();
        return Ok(result);
    }

    /// <summary>
    /// Gets a dashboard-friendly view of environments with their latest deployments per application.
    /// </summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(IEnumerable<EnvironmentDeploymentOverviewDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEnvironmentDashboard()
    {
        var result = await environmentService.GetEnvironmentDeploymentOverviewAsync();
        return Ok(result);
    }

    /// <summary>
    /// Gets a specific environment.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(EnvironmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEnvironment(Guid id)
    {
        var result = await environmentService.GetEnvironmentAsync(id);
        if (result == null)
            return NotFound(new ErrorResponse { Code = "NOT_FOUND", Message = $"Environment with ID {id} not found", TraceId = HttpContext.TraceIdentifier });

        return Ok(result);
    }

    /// <summary>
    /// Creates a new environment.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "ManagerOrAdmin")]
    [ProducesResponseType(typeof(EnvironmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateEnvironment([FromBody] CreateEnvironmentDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ErrorResponse { Code = "INVALID_REQUEST", Message = "Invalid request", TraceId = HttpContext.TraceIdentifier });

        var result = await environmentService.CreateEnvironmentAsync(request);
        return CreatedAtAction(nameof(GetEnvironment), new { id = result.Id }, result);
    }

    /// <summary>
    /// Updates an environment.
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Policy = "ManagerOrAdmin")]
    [ProducesResponseType(typeof(EnvironmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateEnvironment(Guid id, [FromBody] UpdateEnvironmentDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ErrorResponse { Code = "INVALID_REQUEST", Message = "Invalid request", TraceId = HttpContext.TraceIdentifier });

        try
        {
            var result = await environmentService.UpdateEnvironmentAsync(id, request);
            return Ok(result);
        }
        catch (InvalidOperationException)
        {
            return NotFound(new ErrorResponse { Code = "NOT_FOUND", Message = $"Environment with ID {id} not found", TraceId = HttpContext.TraceIdentifier });
        }
    }

    /// <summary>
    /// Deletes an environment.
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "ManagerOrAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteEnvironment(Guid id)
    {
        try
        {
            await environmentService.DeleteEnvironmentAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException)
        {
            return NotFound(new ErrorResponse { Code = "NOT_FOUND", Message = $"Environment with ID {id} not found", TraceId = HttpContext.TraceIdentifier });
        }
    }
}
