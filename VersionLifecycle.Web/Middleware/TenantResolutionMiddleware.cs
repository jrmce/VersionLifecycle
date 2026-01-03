namespace VersionLifecycle.Web.Middleware;

using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using VersionLifecycle.Core.Interfaces;
using VersionLifecycle.Infrastructure.Data;

/// <summary>
/// Middleware to resolve tenant from subdomain and set tenant context.
/// </summary>
public class TenantResolutionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext, AppDbContext dbContext)
    {
        // Skip tenant resolution for health checks to avoid unnecessary DB queries
        if (context.Request.Path.StartsWithSegments("/api/health"))
        {
            await next(context);
            return;
        }

        // Check if user is SuperAdmin - they bypass tenant filtering
        var userRole = context.User?.FindFirst(ClaimTypes.Role)?.Value;
        if (userRole == "SuperAdmin")
        {
            var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "superadmin";
            // Set a special marker for SuperAdmin (empty string indicates no tenant filtering)
            tenantContext.SetTenant(string.Empty, userId);
            await next(context);
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
            else
            {
                // Tenant in token doesn't exist - reject request
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { error = "Invalid tenant" });
                return;
            }
        }
        else if (context.User?.Identity?.IsAuthenticated == true)
        {
            // Authenticated user but no tenant claim - this should not happen
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Tenant ID missing from token" });
            return;
        }
        else
        {
            // Unauthenticated request - allow access to public endpoints only
            // The [Authorize] attribute on controllers will handle authorization
            // For development/testing, we don't set a tenant for anonymous requests
        }

        await next(context);
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
