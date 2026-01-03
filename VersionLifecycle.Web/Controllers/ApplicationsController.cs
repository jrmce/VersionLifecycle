namespace VersionLifecycle.Web.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VersionLifecycle.Application.DTOs;
using VersionLifecycle.Application.Services;
using VersionLifecycle.Web.Models;

/// <summary>
/// Applications controller for CRUD operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ApplicationsController(IApplicationService applicationService) : ControllerBase
{

    /// <summary>
    /// Gets all applications with pagination.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(VersionLifecycle.Application.DTOs.PaginatedResponse<ApplicationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetApplications([FromQuery] int skip = 0, [FromQuery] int take = 25)
    {
        if (take > 100)
            take = 100;

        var result = await applicationService.GetApplicationsAsync(skip, take);
        return Ok(result);
    }

    /// <summary>
    /// Gets a specific application by ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApplicationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetApplication(Guid id)
    {
        var result = await applicationService.GetApplicationAsync(id);
        if (result == null)
            return NotFound(new ErrorResponse { Code = "NOT_FOUND", Message = $"Application with ID {id} not found", TraceId = HttpContext.TraceIdentifier });

        return Ok(result);
    }

    /// <summary>
    /// Creates a new application.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "ManagerOrAdmin")]
    [ProducesResponseType(typeof(ApplicationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateApplication([FromBody] CreateApplicationDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ErrorResponse { Code = "INVALID_REQUEST", Message = "Invalid request", TraceId = HttpContext.TraceIdentifier });

        var result = await applicationService.CreateApplicationAsync(request);
        return CreatedAtAction(nameof(GetApplication), new { id = result.Id }, result);
    }

    /// <summary>
    /// Updates an existing application.
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Policy = "ManagerOrAdmin")]
    [ProducesResponseType(typeof(ApplicationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateApplication(Guid id, [FromBody] UpdateApplicationDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ErrorResponse { Code = "INVALID_REQUEST", Message = "Invalid request", TraceId = HttpContext.TraceIdentifier });

        try
        {
            var result = await applicationService.UpdateApplicationAsync(id, request);
            return Ok(result);
        }
        catch (InvalidOperationException)
        {
            return NotFound(new ErrorResponse { Code = "NOT_FOUND", Message = $"Application with ID {id} not found", TraceId = HttpContext.TraceIdentifier });
        }
    }

    /// <summary>
    /// Deletes an application.
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteApplication(Guid id)
    {
        try
        {
            await applicationService.DeleteApplicationAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException)
        {
            return NotFound(new ErrorResponse { Code = "NOT_FOUND", Message = $"Application with ID {id} not found", TraceId = HttpContext.TraceIdentifier });
        }
    }
}
