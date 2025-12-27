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
public class EnvironmentsController : ControllerBase
{
    private readonly IEnvironmentService _environmentService;

    public EnvironmentsController(IEnvironmentService environmentService)
    {
        _environmentService = environmentService;
    }

    /// <summary>
    /// Gets all environments for the current tenant.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EnvironmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEnvironments()
    {
        var result = await _environmentService.GetEnvironmentsAsync();
        return Ok(result);
    }

    /// <summary>
    /// Gets a specific environment.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(EnvironmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEnvironment(int id)
    {
        var result = await _environmentService.GetEnvironmentAsync(id);
        if (result == null)
            return NotFound(new ErrorResponse { Code = "NOT_FOUND", Message = $"Environment with ID {id} not found", TraceId = HttpContext.TraceIdentifier });

        return Ok(result);
    }

    /// <summary>
    /// Creates a new environment.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(EnvironmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateEnvironment([FromBody] CreateEnvironmentDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ErrorResponse { Code = "INVALID_REQUEST", Message = "Invalid request", TraceId = HttpContext.TraceIdentifier });

        var result = await _environmentService.CreateEnvironmentAsync(request);
        return CreatedAtAction(nameof(GetEnvironment), new { id = result.Id }, result);
    }

    /// <summary>
    /// Updates an environment.
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(EnvironmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateEnvironment(int id, [FromBody] UpdateEnvironmentDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ErrorResponse { Code = "INVALID_REQUEST", Message = "Invalid request", TraceId = HttpContext.TraceIdentifier });

        try
        {
            var result = await _environmentService.UpdateEnvironmentAsync(id, request);
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
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteEnvironment(int id)
    {
        try
        {
            await _environmentService.DeleteEnvironmentAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException)
        {
            return NotFound(new ErrorResponse { Code = "NOT_FOUND", Message = $"Environment with ID {id} not found", TraceId = HttpContext.TraceIdentifier });
        }
    }
}
