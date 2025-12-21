namespace VersionLifecycle.Web.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VersionLifecycle.Application.DTOs;
using VersionLifecycle.Application.Services;

/// <summary>
/// Deployments controller for managing deployments and their lifecycle.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DeploymentsController : ControllerBase
{
    private readonly IDeploymentService _deploymentService;

    public DeploymentsController(IDeploymentService deploymentService)
    {
        _deploymentService = deploymentService;
    }

    /// <summary>
    /// Gets all deployments with optional status filtering.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<DeploymentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeployments([FromQuery] int skip = 0, [FromQuery] int take = 25, [FromQuery] string? status = null)
    {
        if (take > 100)
            take = 100;

        var result = await _deploymentService.GetDeploymentsAsync(skip, take, status);
        return Ok(result);
    }

    /// <summary>
    /// Gets a specific deployment.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(DeploymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDeployment(int id)
    {
        var result = await _deploymentService.GetDeploymentAsync(id);
        if (result == null)
            return NotFound(new ErrorResponse { Code = "NOT_FOUND", Message = $"Deployment with ID {id} not found", TraceId = HttpContext.TraceIdentifier });

        return Ok(result);
    }

    /// <summary>
    /// Creates a pending deployment.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "ManagerOrAdmin")]
    [ProducesResponseType(typeof(DeploymentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateDeployment([FromBody] CreatePendingDeploymentDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ErrorResponse { Code = "INVALID_REQUEST", Message = "Invalid request", TraceId = HttpContext.TraceIdentifier });

        var result = await _deploymentService.CreatePendingDeploymentAsync(request);
        return CreatedAtAction(nameof(GetDeployment), new { id = result.Id }, result);
    }

    /// <summary>
    /// Confirms a pending deployment.
    /// </summary>
    [HttpPost("{id}/confirm")]
    [Authorize(Policy = "ManagerOrAdmin")]
    [ProducesResponseType(typeof(DeploymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConfirmDeployment(int id, [FromBody] ConfirmDeploymentDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ErrorResponse { Code = "INVALID_REQUEST", Message = "Invalid request", TraceId = HttpContext.TraceIdentifier });

        try
        {
            var result = await _deploymentService.ConfirmDeploymentAsync(id, request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("not found"))
                return NotFound(new ErrorResponse { Code = "NOT_FOUND", Message = ex.Message, TraceId = HttpContext.TraceIdentifier });

            return BadRequest(new ErrorResponse { Code = "INVALID_STATE", Message = ex.Message, TraceId = HttpContext.TraceIdentifier });
        }
    }

    /// <summary>
    /// Gets the deployment event history.
    /// </summary>
    [HttpGet("{id}/events")]
    [ProducesResponseType(typeof(IEnumerable<DeploymentEventDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDeploymentHistory(int id)
    {
        try
        {
            var result = await _deploymentService.GetDeploymentHistoryAsync(id);
            return Ok(result);
        }
        catch (InvalidOperationException)
        {
            return NotFound(new ErrorResponse { Code = "NOT_FOUND", Message = $"Deployment with ID {id} not found", TraceId = HttpContext.TraceIdentifier });
        }
    }
}
