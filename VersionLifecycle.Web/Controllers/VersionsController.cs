namespace VersionLifecycle.Web.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VersionLifecycle.Application.DTOs;
using VersionLifecycle.Application.Services;
using VersionLifecycle.Web.Models;

/// <summary>
/// Versions controller for managing application versions.
/// </summary>
[ApiController]
[Route("api/applications/{applicationId}/[controller]")]
[Authorize]
public class VersionsController : ControllerBase
{
    private readonly IVersionService _versionService;

    public VersionsController(IVersionService versionService)
    {
        _versionService = versionService;
    }

    /// <summary>
    /// Gets all versions for an application.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<VersionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVersions(int applicationId)
    {
        var result = await _versionService.GetVersionsByApplicationAsync(applicationId);
        return Ok(result);
    }

    /// <summary>
    /// Gets a specific version.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(VersionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVersion(int applicationId, int id)
    {
        var result = await _versionService.GetVersionAsync(id);
        if (result == null)
            return NotFound(new ErrorResponse { Code = "NOT_FOUND", Message = $"Version with ID {id} not found", TraceId = HttpContext.TraceIdentifier });

        return Ok(result);
    }

    /// <summary>
    /// Creates a new version.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "ManagerOrAdmin")]
    [ProducesResponseType(typeof(VersionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateVersion(int applicationId, [FromBody] CreateVersionDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ErrorResponse { Code = "INVALID_REQUEST", Message = "Invalid request", TraceId = HttpContext.TraceIdentifier });

        request.ApplicationId = applicationId;
        var result = await _versionService.CreateVersionAsync(request);
        return CreatedAtAction(nameof(GetVersion), new { applicationId, id = result.Id }, result);
    }

    /// <summary>
    /// Updates a version.
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Policy = "ManagerOrAdmin")]
    [ProducesResponseType(typeof(VersionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateVersion(int applicationId, int id, [FromBody] UpdateVersionDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ErrorResponse { Code = "INVALID_REQUEST", Message = "Invalid request", TraceId = HttpContext.TraceIdentifier });

        try
        {
            var result = await _versionService.UpdateVersionAsync(id, request);
            return Ok(result);
        }
        catch (InvalidOperationException)
        {
            return NotFound(new ErrorResponse { Code = "NOT_FOUND", Message = $"Version with ID {id} not found", TraceId = HttpContext.TraceIdentifier });
        }
    }

    /// <summary>
    /// Deletes a version.
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteVersion(int applicationId, int id)
    {
        try
        {
            await _versionService.DeleteVersionAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException)
        {
            return NotFound(new ErrorResponse { Code = "NOT_FOUND", Message = $"Version with ID {id} not found", TraceId = HttpContext.TraceIdentifier });
        }
    }
}
