namespace VersionLifecycle.Infrastructure.Services;

using Microsoft.Extensions.DependencyInjection;
using VersionLifecycle.Core.Interfaces;

/// <summary>
/// Service for running background tasks with proper scope and tenant context management.
/// Ensures background tasks get a fresh service scope and restored tenant context to avoid DbContext disposal issues.
/// </summary>
public interface IBackgroundTaskRunner
{
    /// <summary>
    /// Queues a background task with automatic scope creation and tenant context restoration.
    /// </summary>
    /// <param name="task">The task to execute. Receives the scoped service provider.</param>
    void QueueTask(Func<IServiceProvider, Task> task);
}

/// <summary>
/// Implementation of background task runner that handles scope creation and tenant context.
/// </summary>
public class BackgroundTaskRunner : IBackgroundTaskRunner
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ITenantContext _tenantContext;

    public BackgroundTaskRunner(IServiceScopeFactory serviceScopeFactory, ITenantContext tenantContext)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _tenantContext = tenantContext;
    }

    public void QueueTask(Func<IServiceProvider, Task> task)
    {
        // Capture tenant context before entering background task
        var tenantId = _tenantContext.CurrentTenantId;
        var userId = _tenantContext.CurrentUserId;

        // Fire and forget with proper scope management
        _ = Task.Run(async () =>
        {
            using var scope = _serviceScopeFactory.CreateScope();
            
            // Restore tenant context in new scope
            var scopedTenantContext = scope.ServiceProvider.GetRequiredService<ITenantContext>();
            scopedTenantContext.SetTenant(tenantId, userId);
            
            // Execute the task with scoped service provider
            await task(scope.ServiceProvider);
        });
    }
}
