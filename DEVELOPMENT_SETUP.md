# Development Setup

## Database Strategy

For development, the application uses **PostgreSQL** to match the production environment. The database is **automatically recreated on every startup** in Development mode, eliminating the need to manage migrations during active development.

### Prerequisites

**PostgreSQL 15+** must be running locally:

#### Option 1: Using Docker (Recommended)
```bash
# Start PostgreSQL container
docker run -d \
  --name versionlifecycle-postgres \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=versionlifecycle \
  -p 5432:5432 \
  postgres:15-alpine

# Or use docker-compose (includes PostgreSQL + API + Frontend)
docker-compose up
```

#### Option 2: Local PostgreSQL Installation
```bash
# Ubuntu/Debian
sudo apt install postgresql-15

# macOS (Homebrew)
brew install postgresql@15

# Create database
psql -U postgres -c "CREATE DATABASE versionlifecycle;"
```

### Default Connection String
```
Host=localhost;Database=versionlifecycle;Username=postgres;Password=postgres
```

Update in `appsettings.json` if your PostgreSQL credentials differ.

### What Happens on Startup (Development Mode)

1. **Database is dropped** (`EnsureDeletedAsync()`)
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

**Step 1: Start PostgreSQL**
```bash
# If using Docker
docker run -d \
  --name versionlifecycle-postgres \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=versionlifecycle \
  -p 5432:5432 \
  postgres:15-alpine
```

**Step 2: Start Backend**
```bash
cd VersionLifecycle.Web
dotnet run
```

**Step 3: Start Frontend** (separate terminal)
```bash
cd VersionLifecycle.Web/ClientApp
npm start
```

The backend will:
- Start on `http://localhost:5000` (or configured port)
- Drop and recreate the PostgreSQL database
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

**Development:** PostgreSQL database `versionlifecycle` on `localhost:5432`
**Docker:** PostgreSQL container with persistent volume `postgres_data`

The database is recreated on every startup in Development mode.

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

**Using Docker (Everything Included)**
```bash
docker-compose up --build

# Access:
# Frontend: http://localhost
# API: http://localhost:5000
# PostgreSQL: localhost:5432
```

**Manual Setup**
```bash
# 1. Start PostgreSQL
docker run -d --name versionlifecycle-postgres \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=versionlifecycle \
  -p 5432:5432 postgres:15-alpine

# 2. Navigate to project
cd VersionLifecycle.Web

# 3. Install frontend dependencies (first time only)
cd ClientApp && npm install && cd ..

# 4. Start backend (auto-creates database with seed data)
dotnet run

# 5. In another terminal, start frontend
cd ClientApp
npm start

# Login with test credentials
# Email: admin@example.com
# Password: Admin123!
# Tenant ID: demo-tenant-001
# Tenant Code: DEMO-CODE
```

## Troubleshooting

**"Could not connect to PostgreSQL" error:**
- Ensure PostgreSQL is running: `docker ps` or `sudo systemctl status postgresql`
- Check connection string in `appsettings.json`
- Verify port 5432 is not in use: `lsof -i :5432`

**Database locked/permission error:**
- Stop the application
- Drop database: `psql -U postgres -c "DROP DATABASE IF EXISTS versionlifecycle;"`
- Restart the application (database will be recreated)

**Seed data not appearing:**
- Check you're in Development mode: `ASPNETCORE_ENVIRONMENT=Development`
- Check the console output for seeding errors
- Manually drop and recreate: `psql -U postgres -c "DROP DATABASE versionlifecycle; CREATE DATABASE versionlifecycle;"`
- Restart the app

**Migration errors:**
- Delete all migrations: `rm -f VersionLifecycle.Infrastructure/Migrations/*.cs`
- Recreate: `dotnet ef migrations add InitialCreate --project VersionLifecycle.Infrastructure --startup-project VersionLifecycle.Web`
- Restart the app

**PostgreSQL not accessible:**
```bash
# Using Docker
docker logs versionlifecycle-postgres

# Check if container is running
docker ps -a | grep postgres

# Restart container
docker restart versionlifecycle-postgres
```
