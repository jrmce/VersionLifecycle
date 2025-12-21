namespace VersionLifecycle.Web.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VersionLifecycle.Application.DTOs;
using VersionLifecycle.Application.Services;

/// <summary>
/// Webhooks controller for managing webhook integrations.
/// </summary>
[ApiController]
[Route("api/applications/{applicationId}/[controller]")]
[Authorize]
public class WebhooksController : ControllerBase
{
    private readonly IWebhookService _webhookService;

    public WebhooksController(IWebhookService webhookService)
    {
        _webhookService = webhookService;
    }

    /// <summary>
    /// Gets all webhooks for an application.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "ManagerOrAdmin")]
    [ProducesResponseType(typeof(IEnumerable<WebhookDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWebhooks(int applicationId)
    {
        var result = await _webhookService.GetWebhooksAsync(applicationId);
        return Ok(result);
    }

    /// <summary>
    /// Gets a specific webhook.
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Policy = "ManagerOrAdmin")]
    [ProducesResponseType(typeof(WebhookDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWebhook(int applicationId, int id)
    {
        var result = await _webhookService.GetWebhookAsync(id);
        if (result == null)
            return NotFound(new ErrorResponse { Code = "NOT_FOUND", Message = $"Webhook with ID {id} not found", TraceId = HttpContext.TraceIdentifier });

        return Ok(result);
    }

    /// <summary>
    /// Creates a new webhook.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(WebhookDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateWebhook(int applicationId, [FromBody] CreateWebhookDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ErrorResponse { Code = "INVALID_REQUEST", Message = "Invalid request", TraceId = HttpContext.TraceIdentifier });

        request.ApplicationId = applicationId;
        var result = await _webhookService.CreateWebhookAsync(request);
        return CreatedAtAction(nameof(GetWebhook), new { applicationId, id = result.Id }, result);
    }

    /// <summary>
    /// Deletes a webhook.
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteWebhook(int applicationId, int id)
    {
        try
        {
            await _webhookService.DeleteWebhookAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException)
        {
            return NotFound(new ErrorResponse { Code = "NOT_FOUND", Message = $"Webhook with ID {id} not found", TraceId = HttpContext.TraceIdentifier });
        }
    }

    /// <summary>
    /// Gets the delivery history for a webhook.
    /// </summary>
    [HttpGet("{id}/events")]
    [Authorize(Policy = "ManagerOrAdmin")]
    [ProducesResponseType(typeof(IEnumerable<WebhookEventDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDeliveryHistory(int applicationId, int id, [FromQuery] int take = 50)
    {
        try
        {
            var result = await _webhookService.GetDeliveryHistoryAsync(id, take);
            return Ok(result);
        }
        catch (InvalidOperationException)
        {
            return NotFound(new ErrorResponse { Code = "NOT_FOUND", Message = $"Webhook with ID {id} not found", TraceId = HttpContext.TraceIdentifier });
        }
    }
}
