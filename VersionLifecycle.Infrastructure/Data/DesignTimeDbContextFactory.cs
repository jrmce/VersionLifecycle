namespace VersionLifecycle.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;
using VersionLifecycle.Core.Interfaces;
using VersionLifecycle.Infrastructure.Multitenancy;

/// <summary>
/// Design-time factory for creating DbContext instances during migration operations.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
        
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // Always use PostgreSQL for consistency between Development and Production
        optionsBuilder.UseNpgsql(connectionString ?? "Host=localhost;Database=versionlifecycle;Username=postgres;Password=postgres");

        // Create a default tenant context for design-time operations
        ITenantContext tenantContext = new TenantContext();

        return new AppDbContext(optionsBuilder.Options, tenantContext);
    }
}
