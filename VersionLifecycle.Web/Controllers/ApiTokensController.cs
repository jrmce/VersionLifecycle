namespace VersionLifecycle.Web.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VersionLifecycle.Application.DTOs;
using VersionLifecycle.Application.Services;
using VersionLifecycle.Web.Models;

/// <summary>
/// Controller for API token management (system-to-system authentication).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class ApiTokensController(IApiTokenService apiTokenService) : ControllerBase
{
    /// <summary>
    /// Gets all API tokens for the current tenant.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ApiTokenDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetApiTokens()
    {
        var tokens = await apiTokenService.GetApiTokensAsync();
        return Ok(tokens);
    }

    /// <summary>
    /// Gets a specific API token by ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiTokenDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetApiToken(int id)
    {
        var token = await apiTokenService.GetApiTokenAsync(id);
        if (token == null)
            return NotFound(new ErrorResponse 
            { 
                Code = "NOT_FOUND", 
                Message = $"API token with ID {id} not found", 
                TraceId = HttpContext.TraceIdentifier 
            });

        return Ok(token);
    }

    /// <summary>
    /// Creates a new API token.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiTokenCreatedDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateApiToken([FromBody] CreateApiTokenDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ErrorResponse 
            { 
                Code = "INVALID_REQUEST", 
                Message = "Invalid request", 
                TraceId = HttpContext.TraceIdentifier 
            });

        var result = await apiTokenService.CreateApiTokenAsync(request);
        return CreatedAtAction(nameof(GetApiToken), new { id = result.Id }, result);
    }

    /// <summary>
    /// Updates an API token (name, description, active status).
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiTokenDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateApiToken(int id, [FromBody] UpdateApiTokenDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ErrorResponse 
            { 
                Code = "INVALID_REQUEST", 
                Message = "Invalid request", 
                TraceId = HttpContext.TraceIdentifier 
            });

        try
        {
            var result = await apiTokenService.UpdateApiTokenAsync(id, request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return NotFound(new ErrorResponse 
            { 
                Code = "NOT_FOUND", 
                Message = ex.Message, 
                TraceId = HttpContext.TraceIdentifier 
            });
        }
    }

    /// <summary>
    /// Revokes an API token (soft delete).
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeApiToken(int id)
    {
        try
        {
            await apiTokenService.RevokeApiTokenAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return NotFound(new ErrorResponse 
            { 
                Code = "NOT_FOUND", 
                Message = ex.Message, 
                TraceId = HttpContext.TraceIdentifier 
            });
        }
    }
}
