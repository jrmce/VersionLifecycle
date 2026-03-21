namespace VersionLifecycle.Web.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VersionLifecycle.Application.DTOs;
using VersionLifecycle.Application.Services;
using VersionLifecycle.Web.Models;

/// <summary>
/// Controller for AI-powered insights and natural language queries about tenant data.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InsightsController(IInsightsService insightsService) : ControllerBase
{
    /// <summary>
    /// Asks a natural language question about the tenant's data.
    /// </summary>
    [HttpPost("ask")]
    [ProducesResponseType(typeof(InsightsResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Ask([FromBody] InsightsQueryDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
            return BadRequest(new ErrorResponse { Code = "INVALID_REQUEST", Message = "Question is required", TraceId = HttpContext.TraceIdentifier });

        var result = await insightsService.AskQuestionAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Checks whether the AI insights feature is available.
    /// </summary>
    [HttpGet("status")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult GetStatus()
    {
        return Ok(new { available = insightsService.IsAvailable });
    }
}
