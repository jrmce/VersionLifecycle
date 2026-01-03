namespace VersionLifecycle.Infrastructure.Data;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VersionLifecycle.Core.Entities;
using VersionLifecycle.Core.Enums;

/// <summary>
/// Seeds initial data for development and testing.
/// </summary>
public class DataSeeder(AppDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
{

    /// <summary>
    /// Seeds all data.
    /// </summary>
    public async Task SeedAsync()
    {
        await SeedRolesAsync();
        await SeedUsersAsync();
        await SeedTenantsAsync();
        await SeedApplicationsAsync();
    }

    /// <summary>
    /// Seeds identity roles.
    /// </summary>
    private async Task SeedRolesAsync()
    {
        var roles = new[] { "SuperAdmin", "Admin", "Manager", "Viewer" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    /// <summary>
    /// Seeds default users.
    /// </summary>
    private async Task SeedUsersAsync()
    {
        // SuperAdmin user (not tenant-specific)
        if (await userManager.FindByEmailAsync("superadmin@example.com") == null)
        {
            var superAdminUser = new IdentityUser
            {
                UserName = "superadmin@example.com",
                Email = "superadmin@example.com",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(superAdminUser, "SuperAdmin123!");
            await userManager.AddToRoleAsync(superAdminUser, "SuperAdmin");
        }

        // Admin user
        if (await userManager.FindByEmailAsync("admin@example.com") == null)
        {
            var adminUser = new IdentityUser
            {
                UserName = "admin@example.com",
                Email = "admin@example.com",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(adminUser, "Admin123!");
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }

        // Manager user
        if (await userManager.FindByEmailAsync("manager@example.com") == null)
        {
            var managerUser = new IdentityUser
            {
                UserName = "manager@example.com",
                Email = "manager@example.com",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(managerUser, "Manager123!");
            await userManager.AddToRoleAsync(managerUser, "Manager");
        }

        // Viewer user
        if (await userManager.FindByEmailAsync("viewer@example.com") == null)
        {
            var viewerUser = new IdentityUser
            {
                UserName = "viewer@example.com",
                Email = "viewer@example.com",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(viewerUser, "Viewer123!");
            await userManager.AddToRoleAsync(viewerUser, "Viewer");
        }
    }

    /// <summary>
    /// Seeds default tenant.
    /// </summary>
    private async Task SeedTenantsAsync()
    {
        if (!context.Tenants.Any())
        {
            var tenant = new Tenant
            {
                Id = "demo-tenant-001",
                Name = "Demo Organization",
                Description = "Demo organization for testing",
                SubscriptionPlan = "Free",
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                Code = "DEMO-CODE"
            };
            context.Tenants.Add(tenant);
            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Seeds sample applications with versions and environments.
    /// </summary>
    private async Task SeedApplicationsAsync()
    {
        var tenantId = "demo-tenant-001";
        var adminUser = await userManager.FindByEmailAsync("admin@example.com");
        
        if (adminUser == null || context.Applications.Any())
            return;

        // Create sample application
        var app = new Application
        {
            TenantId = tenantId,
            Name = "Payment Service",
            Description = "Core payment processing microservice",
            RepositoryUrl = "https://github.com/example/payment-service",
            CreatedBy = adminUser.Id
        };
        context.Applications.Add(app);
        await context.SaveChangesAsync();

        // Create environments (tenant-level, not application-specific)
        var dev = new Environment
        {
            TenantId = tenantId,
            Name = "Development",
            Order = 1,
            CreatedBy = adminUser.Id
        };

        var staging = new Environment
        {
            TenantId = tenantId,
            Name = "Staging",
            Order = 2,
            CreatedBy = adminUser.Id
        };

        var prod = new Environment
        {
            TenantId = tenantId,
            Name = "Production",
            Order = 3,
            CreatedBy = adminUser.Id
        };

        context.Environments.AddRange(dev, staging, prod);
        await context.SaveChangesAsync();

        // Create versions
        var v100 = new Version
        {
            TenantId = tenantId,
            ApplicationId = app.Id,
            VersionNumber = "1.0.0",
            ReleaseNotes = "Initial release with core payment features",
            Status = VersionStatus.Released,
            CreatedBy = adminUser.Id
        };

        var v110 = new Version
        {
            TenantId = tenantId,
            ApplicationId = app.Id,
            VersionNumber = "1.1.0",
            ReleaseNotes = "Added refund processing and webhook notifications",
            Status = VersionStatus.Released,
            CreatedBy = adminUser.Id
        };

        var v120 = new Version
        {
            TenantId = tenantId,
            ApplicationId = app.Id,
            VersionNumber = "1.2.0",
            ReleaseNotes = "Performance improvements and bug fixes",
            Status = VersionStatus.Draft,
            CreatedBy = adminUser.Id
        };

        context.Versions.AddRange(v100, v110, v120);
        await context.SaveChangesAsync();

        // Create deployments
        var deployment1 = new Deployment
        {
            TenantId = tenantId,
            ApplicationId = app.Id,
            VersionId = v110.Id,
            EnvironmentId = prod.Id,
            Status = DeploymentStatus.Success,
            Notes = "Deployed to production successfully",
            DeployedAt = DateTime.UtcNow.AddDays(-7),
            CreatedBy = adminUser.Id
        };

        var deployment2 = new Deployment
        {
            TenantId = tenantId,
            ApplicationId = app.Id,
            VersionId = v120.Id,
            EnvironmentId = dev.Id,
            Status = DeploymentStatus.Success,
            Notes = "Testing new features in dev",
            DeployedAt = DateTime.UtcNow.AddDays(-2),
            CreatedBy = adminUser.Id
        };

        var deployment3 = new Deployment
        {
            TenantId = tenantId,
            ApplicationId = app.Id,
            VersionId = v120.Id,
            EnvironmentId = staging.Id,
            Status = DeploymentStatus.Pending,
            Notes = "Awaiting QA approval",
            CreatedBy = adminUser.Id
        };

        context.Deployments.AddRange(deployment1, deployment2, deployment3);
        await context.SaveChangesAsync();

        // Create deployment events
        var event1 = new DeploymentEvent
        {
            TenantId = tenantId,
            DeploymentId = deployment1.Id,
            EventType = "Started",
            Message = "Deployment initiated by admin",
            Timestamp = DateTime.UtcNow,
            CreatedBy = adminUser.Id
        };

        var event2 = new DeploymentEvent
        {
            TenantId = tenantId,
            DeploymentId = deployment1.Id,
            EventType = "Completed",
            Message = "Deployment completed successfully",
            Timestamp = DateTime.UtcNow.AddMinutes(5),
            CreatedBy = adminUser.Id
        };

        context.DeploymentEvents.AddRange(event1, event2);
        await context.SaveChangesAsync();
    }
}
