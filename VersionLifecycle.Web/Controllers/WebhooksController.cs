namespace VersionLifecycle.Web.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VersionLifecycle.Application.DTOs;
using VersionLifecycle.Application.Services;
using VersionLifecycle.Infrastructure.Services;
using VersionLifecycle.Web.Models;

/// <summary>
/// Webhooks controller for managing webhook integrations.
/// </summary>
[ApiController]
[Route("api/applications/{applicationId}/[controller]")]
[Authorize]
public class WebhooksController(IWebhookService webhookService, WebhookDeliveryService webhookDeliveryService) : ControllerBase
{

    /// <summary>
    /// Gets all webhooks for an application.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "ManagerOrAdmin")]
    [ProducesResponseType(typeof(IEnumerable<WebhookDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWebhooks(int applicationId)
    {
        var result = await webhookService.GetWebhooksAsync(applicationId);
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
        var result = await webhookService.GetWebhookAsync(id);
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
        var result = await webhookService.CreateWebhookAsync(request);
        return CreatedAtAction(nameof(GetWebhook), new { applicationId, id = result.Id }, result);
    }

    /// <summary>
    /// Updates an existing webhook.
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(WebhookDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateWebhook(int applicationId, int id, [FromBody] UpdateWebhookDto request)
    {
        try
        {
            var result = await webhookService.UpdateWebhookAsync(id, request);
            return Ok(result);
        }
        catch (InvalidOperationException)
        {
            return NotFound(new ErrorResponse { Code = "NOT_FOUND", Message = $"Webhook with ID {id} not found", TraceId = HttpContext.TraceIdentifier });
        }
    }

    /// <summary>
    /// Tests a webhook by sending a test payload.
    /// </summary>
    [HttpPost("{id}/test")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(WebhookDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TestWebhook(int applicationId, int id)
    {
        try
        {
            var result = await webhookService.TestWebhookAsync(id);
            return Accepted(result);
        }
        catch (InvalidOperationException)
        {
            return NotFound(new ErrorResponse { Code = "NOT_FOUND", Message = $"Webhook with ID {id} not found", TraceId = HttpContext.TraceIdentifier });
        }
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
            await webhookService.DeleteWebhookAsync(id);
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
            var result = await webhookService.GetDeliveryHistoryAsync(id, take);
            return Ok(result);
        }
        catch (InvalidOperationException)
        {
            return NotFound(new ErrorResponse { Code = "NOT_FOUND", Message = $"Webhook with ID {id} not found", TraceId = HttpContext.TraceIdentifier });
        }
    }

    /// <summary>
    /// Manually retries a failed webhook event.
    /// </summary>
    [HttpPost("{id}/events/{eventId}/retry")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RetryWebhookEvent(int applicationId, int id, int eventId)
    {
        _ = Task.Run(async () => await webhookDeliveryService.DeliverWebhookAsync(eventId));
        return Accepted();
    }
}
