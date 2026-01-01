namespace VersionLifecycle.Web.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VersionLifecycle.Application.DTOs;
using VersionLifecycle.Application.Services;
using VersionLifecycle.Web.Models;

/// <summary>
/// Deployments controller for managing deployments and their lifecycle.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DeploymentsController(IDeploymentService deploymentService) : ControllerBase
{

    /// <summary>
    /// Gets all deployments with optional status filtering.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(VersionLifecycle.Application.DTOs.PaginatedResponse<DeploymentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeployments([FromQuery] int skip = 0, [FromQuery] int take = 25, [FromQuery] string? status = null)
    {
        if (take > 100)
            take = 100;

        var result = await deploymentService.GetDeploymentsAsync(skip, take, status);
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
        var result = await deploymentService.GetDeploymentAsync(id);
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

        var result = await deploymentService.CreatePendingDeploymentAsync(request);
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
            var result = await deploymentService.ConfirmDeploymentAsync(id, request);
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
    /// Updates deployment status (InProgress, Success, Failed, or Cancelled).
    /// </summary>
    [HttpPatch("{id}/status")]
    [Authorize(Policy = "ManagerOrAdmin")]
    [ProducesResponseType(typeof(DeploymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateDeploymentStatus(int id, [FromBody] UpdateDeploymentStatusDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ErrorResponse { Code = "INVALID_REQUEST", Message = "Invalid request", TraceId = HttpContext.TraceIdentifier });

        try
        {
            var result = await deploymentService.UpdateDeploymentStatusAsync(id, request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return NotFound(new ErrorResponse { Code = "NOT_FOUND", Message = ex.Message, TraceId = HttpContext.TraceIdentifier });

            return BadRequest(new ErrorResponse { Code = "INVALID_STATE", Message = ex.Message, TraceId = HttpContext.TraceIdentifier });
        }
    }

    /// <summary>
    /// Promotes a deployment to the next environment in order. Auto-confirms to InProgress.
    /// </summary>
    [HttpPost("{id}/promote")]
    [Authorize(Policy = "ManagerOrAdmin")]
    [ProducesResponseType(typeof(DeploymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PromoteDeployment(int id, [FromBody] PromoteDeploymentDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ErrorResponse { Code = "INVALID_REQUEST", Message = "Invalid request", TraceId = HttpContext.TraceIdentifier });

        try
        {
            var result = await deploymentService.PromoteDeploymentAsync(id, request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
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
            var result = await deploymentService.GetDeploymentHistoryAsync(id);
            return Ok(result);
        }
        catch (InvalidOperationException)
        {
            return NotFound(new ErrorResponse { Code = "NOT_FOUND", Message = $"Deployment with ID {id} not found", TraceId = HttpContext.TraceIdentifier });
        }
    }
}
