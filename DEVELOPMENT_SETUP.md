# Development Setup

## Database Strategy

For development, the application is configured to **automatically recreate the database on every startup**. This eliminates the need to manage multiple migrations during active development.

### What Happens on Startup (Development Mode)

1. **Database is deleted** (`EnsureDeletedAsync()`)
2. **Database is recreated** with the latest schema (`MigrateAsync()`)
3. **Seed data is loaded** automatically:
   - Identity roles (SuperAdmin, Admin, Manager, Viewer)
   - Test users with default passwords
   - Demo tenant ("demo-tenant-001")
   - Sample application ("Payment Service")
   - Sample versions, environments, and deployments

### Default Test Users

All users have the tenant ID: `demo-tenant-001`

| Email | Password | Role | Tenant Code |
|-------|----------|------|-------------|
| superadmin@example.com | SuperAdmin123! | SuperAdmin | DEMO-CODE |
| admin@example.com | Admin123! | Admin | DEMO-CODE |
| manager@example.com | Manager123! | Manager | DEMO-CODE |
| viewer@example.com | Viewer123! | Viewer | DEMO-CODE |

### Running the Application

```bash
# Backend API (auto-recreates DB on startup)
cd VersionLifecycle.Web
dotnet run

# Frontend (separate terminal)
cd VersionLifecycle.Web/ClientApp
npm start
```

The backend will:
- Start on `http://localhost:5000` (or configured port)
- Delete and recreate the SQLite database
- Apply the single `InitialCreate` migration
- Seed test data
- Be ready for API calls

The frontend will:
- Start on `http://localhost:4200`
- Connect to the backend API

### Production Behavior

In **Production** mode (when `ASPNETCORE_ENVIRONMENT` is not "Development"):
- Database is **NOT** deleted
- Only migrations are applied (`MigrateAsync()`)
- No seed data is loaded
- Existing data is preserved

### Migration Management

During active development, we use a **single squashed migration**:

```bash
# If you make schema changes:

# 1. Remove the old migration
cd VersionLifecycle.Infrastructure/Migrations
rm -f *.cs

# 2. Create a new migration
cd ../..
dotnet ef migrations add InitialCreate --project VersionLifecycle.Infrastructure --startup-project VersionLifecycle.Web

# 3. Restart the app - database will be recreated automatically
dotnet run --project VersionLifecycle.Web
```

### When to Switch to Incremental Migrations

When you're ready to move to production or need to preserve data:

1. Update `Program.cs` to remove `EnsureDeletedAsync()`
2. Start using incremental migrations (`dotnet ef migrations add <Name>`)
3. Apply migrations manually or via CI/CD

### Database Location

Development SQLite database: `VersionLifecycle.Web/versionlifecycle.db`

This file is recreated on every startup in Development mode.

## GUID Implementation

All entities now use:
- **Internal integer primary keys** for database performance
- **External GUID identifiers** for API security

Example API response:
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Payment Service",
  "tenantId": "demo-tenant-001"
}
```

The `id` field is automatically generated from the entity's `ExternalId` property via AutoMapper.

## Quick Start

```bash
# Clone and navigate to project
cd VersionLifecycle.Web

# Install frontend dependencies (first time only)
cd ClientApp && npm install && cd ..

# Start backend (auto-creates database with seed data)
dotnet run

# In another terminal, start frontend
cd ClientApp
npm start

# Login with test credentials
# Email: admin@example.com
# Password: Admin123!
# Tenant ID: demo-tenant-001
# Tenant Code: DEMO-CODE
```

## Troubleshooting

**Database locked error:**
- Stop the application
- Delete `versionlifecycle.db` manually
- Restart the application

**Seed data not appearing:**
- Check you're in Development mode: `ASPNETCORE_ENVIRONMENT=Development`
- Check the console output for seeding errors
- Delete the database file and restart

**Migration errors:**
- Delete all migrations: `rm -f VersionLifecycle.Infrastructure/Migrations/*.cs`
- Recreate: `dotnet ef migrations add InitialCreate --project VersionLifecycle.Infrastructure --startup-project VersionLifecycle.Web`
- Restart the app
