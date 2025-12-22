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
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        optionsBuilder.UseNpgsql(connectionString);

        // Create a default tenant context for design-time operations
        ITenantContext tenantContext = new TenantContext();

        return new AppDbContext(optionsBuilder.Options, tenantContext);
    }
}
