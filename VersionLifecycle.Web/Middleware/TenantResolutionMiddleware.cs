namespace VersionLifecycle.Web.Middleware;

using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using VersionLifecycle.Core.Interfaces;
using VersionLifecycle.Infrastructure.Data;

/// <summary>
/// Middleware to resolve tenant from subdomain and set tenant context.
/// </summary>
public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext, AppDbContext dbContext)
    {
        // Skip tenant resolution for health checks to avoid unnecessary DB queries
        if (context.Request.Path.StartsWithSegments("/api/health"))
        {
            await _next(context);
            return;
        }

        // Try to get tenant from JWT token first
        var tenantIdFromToken = context.User?.FindFirst("tenantId")?.Value;
        
        if (!string.IsNullOrEmpty(tenantIdFromToken))
        {
            // Verify tenant exists
            var tenant = await dbContext.Tenants
                .FirstOrDefaultAsync(t => t.Id == tenantIdFromToken);

            if (tenant != null)
            {
                var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
                tenantContext.SetTenant(tenantIdFromToken, userId);
            }
        }
        else
        {
            // Fallback: Try to get first available tenant for development
            var firstTenant = await dbContext.Tenants.FirstOrDefaultAsync();
            if (firstTenant != null)
            {
                tenantContext.SetTenant(firstTenant.Id, "anonymous");
            }
        }

        await _next(context);
    }
}

/// <summary>
/// Extension method to register tenant resolution middleware.
/// </summary>
public static class TenantResolutionMiddlewareExtensions
{
    public static IApplicationBuilder UseTenantResolution(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TenantResolutionMiddleware>();
    }
}
