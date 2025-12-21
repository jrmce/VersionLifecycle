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
        // Try to get tenant from JWT token first
        var tenantIdFromToken = context.User?.FindFirst("tenantId")?.Value;
        
        if (!string.IsNullOrEmpty(tenantIdFromToken))
        {
            // Verify tenant exists
            var tenant = await dbContext.Tenants
                .FirstOrDefaultAsync(t => t.TenantId == tenantIdFromToken);

            if (tenant != null)
            {
                var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
                tenantContext.SetTenant(tenantIdFromToken, userId);
            }
        }
        else
        {
            // Fallback: Try to extract from subdomain
            var host = context.Request.Host.Host;
            var subdomain = GetSubdomain(host);

            if (!string.IsNullOrEmpty(subdomain))
            {
                var tenant = await dbContext.Tenants
                    .FirstOrDefaultAsync(t => t.Subdomain == subdomain);

                if (tenant != null)
                {
                    tenantContext.SetTenant(tenant.TenantId, "anonymous");
                }
            }
        }

        await _next(context);
    }

    private string? GetSubdomain(string host)
    {
        // Extract subdomain from host (e.g., demo.example.com -> demo)
        var parts = host.Split('.');
        
        // If localhost or IP, no subdomain
        if (parts.Length < 3 || host.Contains("localhost") || host.Contains("127.0.0.1"))
            return null;

        return parts[0];
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
