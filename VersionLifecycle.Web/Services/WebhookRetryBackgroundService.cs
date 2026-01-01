namespace VersionLifecycle.Web.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VersionLifecycle.Infrastructure.Services;

/// <summary>
/// Background service for retrying failed webhook deliveries.
/// </summary>
public class WebhookRetryBackgroundService(
    IServiceProvider serviceProvider,
    ILogger<WebhookRetryBackgroundService> logger) : BackgroundService
{
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Webhook retry background service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_interval, stoppingToken);

                using var scope = serviceProvider.CreateScope();
                var webhookDeliveryService = scope.ServiceProvider.GetRequiredService<WebhookDeliveryService>();
                
                await webhookDeliveryService.RetryPendingWebhooksAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in webhook retry background service");
            }
        }

        logger.LogInformation("Webhook retry background service stopped");
    }
}
