using Microsoft.EntityFrameworkCore;
using VersionLifecycle.Core.Interfaces;
using VersionLifecycle.Infrastructure.Data;
using VersionLifecycle.Infrastructure.Multitenancy;

namespace VersionLifecycle.Tests.Fixtures;

/// <summary>
/// Fixture for creating an in-memory DbContext for testing.
/// </summary>
public class InMemoryDbContextFixture : IDisposable
{
    private readonly DbContextOptions<AppDbContext> _options;

    public InMemoryDbContextFixture()
    {
        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    /// <summary>
    /// Creates a new DbContext instance for testing.
    /// </summary>
    public AppDbContext CreateDbContext(string? tenantId = null)
    {
        var context = new AppDbContext(_options, new TestTenantContext(tenantId ?? "test-tenant"));
        context.Database.EnsureCreated();
        return context;
    }

    public void Dispose()
    {
        using (var context = new AppDbContext(_options, new TestTenantContext("test")))
        {
            context.Database.EnsureDeleted();
        }
    }
}

/// <summary>
/// Test implementation of ITenantContext.
/// </summary>
public class TestTenantContext : ITenantContext
{
    private string _tenantId;
    private string? _userId;

    public TestTenantContext(string tenantId = "test-tenant", string? userId = null)
    {
        _tenantId = tenantId;
        _userId = userId;
    }

    public string CurrentTenantId => _tenantId;
    public string? CurrentUserId => _userId;

    public void SetTenant(string tenantId, string? userId = null)
    {
        _tenantId = tenantId;
        _userId = userId;
    }
}
