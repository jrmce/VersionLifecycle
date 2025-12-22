namespace VersionLifecycle.Infrastructure.Data;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VersionLifecycle.Core.Entities;
using VersionLifecycle.Core.Enums;

/// <summary>
/// Seeds initial data for development and testing.
/// </summary>
public class DataSeeder
{
    private readonly AppDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public DataSeeder(AppDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    /// <summary>
    /// Seeds all data.
    /// </summary>
    public async Task SeedAsync()
    {
        try
        {
            // Disable foreign key constraints for seeding
            await _context.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = OFF;");
            
            await SeedRolesAsync();
            await SeedUsersAsync();
            await SeedTenantsAsync();
            await SeedApplicationsAsync();
            
            // Re-enable foreign key constraints
            await _context.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = ON;");
        }
        catch
        {
            // Re-enable foreign key constraints on error
            try
            {
                await _context.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = ON;");
            }
            catch { }
            throw;
        }
    }

    /// <summary>
    /// Seeds identity roles.
    /// </summary>
    private async Task SeedRolesAsync()
    {
        var roles = new[] { "Admin", "Manager", "Viewer" };
        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    /// <summary>
    /// Seeds default users.
    /// </summary>
    private async Task SeedUsersAsync()
    {
        // Admin user
        if (await _userManager.FindByEmailAsync("admin@example.com") == null)
        {
            var adminUser = new IdentityUser
            {
                UserName = "admin@example.com",
                Email = "admin@example.com",
                EmailConfirmed = true
            };
            await _userManager.CreateAsync(adminUser, "Admin123!");
            await _userManager.AddToRoleAsync(adminUser, "Admin");
        }

        // Manager user
        if (await _userManager.FindByEmailAsync("manager@example.com") == null)
        {
            var managerUser = new IdentityUser
            {
                UserName = "manager@example.com",
                Email = "manager@example.com",
                EmailConfirmed = true
            };
            await _userManager.CreateAsync(managerUser, "Manager123!");
            await _userManager.AddToRoleAsync(managerUser, "Manager");
        }

        // Viewer user
        if (await _userManager.FindByEmailAsync("viewer@example.com") == null)
        {
            var viewerUser = new IdentityUser
            {
                UserName = "viewer@example.com",
                Email = "viewer@example.com",
                EmailConfirmed = true
            };
            await _userManager.CreateAsync(viewerUser, "Viewer123!");
            await _userManager.AddToRoleAsync(viewerUser, "Viewer");
        }
    }

    /// <summary>
    /// Seeds default tenant.
    /// </summary>
    private async Task SeedTenantsAsync()
    {
        if (!_context.Tenants.Any())
        {
            var tenant = new Tenant
            {
                Id = "demo-tenant-001",
                Name = "Demo Organization",
                Description = "Demo organization for testing",
                SubscriptionPlan = "Free",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Seeds sample applications with versions and environments.
    /// </summary>
    private async Task SeedApplicationsAsync()
    {
        var tenantId = "demo-tenant-001";
        var adminUser = await _userManager.FindByEmailAsync("admin@example.com");
        
        if (adminUser == null || _context.Applications.Any())
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
        _context.Applications.Add(app);
        await _context.SaveChangesAsync();

        // Create environments
        var dev = new Environment
        {
            TenantId = tenantId,
            ApplicationId = app.Id,
            Name = "Development",
            Order = 1,
            CreatedBy = adminUser.Id
        };

        var staging = new Environment
        {
            TenantId = tenantId,
            ApplicationId = app.Id,
            Name = "Staging",
            Order = 2,
            CreatedBy = adminUser.Id
        };

        var prod = new Environment
        {
            TenantId = tenantId,
            ApplicationId = app.Id,
            Name = "Production",
            Order = 3,
            CreatedBy = adminUser.Id
        };

        _context.Environments.AddRange(dev, staging, prod);
        await _context.SaveChangesAsync();

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

        _context.Versions.AddRange(v100, v110, v120);
        await _context.SaveChangesAsync();

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

        _context.Deployments.AddRange(deployment1, deployment2, deployment3);
        await _context.SaveChangesAsync();

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

        _context.DeploymentEvents.AddRange(event1, event2);
        await _context.SaveChangesAsync();
    }
}
