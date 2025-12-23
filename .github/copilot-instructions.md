# GitHub Copilot Instructions for Version Lifecycle Management

## Architecture Overview

This is a **multi-tenant SaaS application** for tracking software version deployments using **.NET 10 Clean Architecture** + **Angular 17** frontend.

**Critical Architectural Pattern**: Every entity (except `Tenant`) inherits from `BaseEntity` which provides `TenantId` for data isolation. The `ITenantContext` service (scoped) provides current tenant context, and `AppDbContext` applies global query filters to enforce tenant boundaries automatically.

### Project Structure (4-layer clean architecture)
```
Core/           → Domain entities, enums, interfaces (no dependencies)
Application/    → DTOs, service interfaces, AutoMapper profiles
Infrastructure/ → EF Core (AppDbContext), repositories, TokenService
Web/            → API controllers, middleware, Angular ClientApp/
```

**Key Design Decision**: `Tenant` entity is special - it does NOT inherit `BaseEntity` and has a string `Id` (not int). Use `TenantRepository` (not `GenericRepository<Tenant>`).

## Development Workflows

### Local Development Setup
```bash
# Backend: SQLite for development (no PostgreSQL needed)
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet run --project VersionLifecycle.Web/VersionLifecycle.Web.csproj
# → Backend API on http://localhost:5000, Swagger at /swagger

# Frontend: Angular dev server
cd VersionLifecycle.Web/ClientApp
npm start
# → Frontend on http://localhost:4200
```

### Database Migrations
```bash
# Uses DesignTimeDbContextFactory which auto-detects SQLite vs PostgreSQL
dotnet ef migrations add MigrationName --project VersionLifecycle.Infrastructure --startup-project VersionLifecycle.Web
dotnet ef database update --project VersionLifecycle.Infrastructure --startup-project VersionLifecycle.Web
```

### Build & Test
```bash
dotnet build                    # All 4 backend projects
cd VersionLifecycle.Web/ClientApp && npm run build  # Angular production build
```

## Critical Patterns & Conventions

### 1. Multi-Tenancy Implementation
- **Query Filtering**: `AppDbContext.AddTenantQueryFilters()` uses reflection to apply `e => e.TenantId == currentTenant` to all `BaseEntity`-derived types
- **Tenant Context**: Inject `ITenantContext` to get/set `CurrentTenantId` and `CurrentUserId` (scoped service populated by `TenantResolutionMiddleware`)
- **Special Case**: `Tenant` entity has `Id` property (string), not `TenantId` - handled separately with `TenantRepository`

### 2. Service Layer DI Pattern
```csharp
// All services are implemented and registered in Program.cs:
builder.Services.AddScoped<ApplicationService>();
builder.Services.AddScoped<IApplicationService>(sp => sp.GetRequiredService<ApplicationService>());
builder.Services.AddScoped<VersionService>();
builder.Services.AddScoped<IVersionService>(sp => sp.GetRequiredService<VersionService>());
builder.Services.AddScoped<DeploymentService>();
builder.Services.AddScoped<IDeploymentService>(sp => sp.GetRequiredService<DeploymentService>());
// ... etc for EnvironmentService, WebhookService, TenantService
```

### 3. Repository Pattern
- **Generic**: `GenericRepository<T>` where `T : BaseEntity` - handles tenant filtering, soft deletes (`IsDeleted`), audit properties
- **Specialized**: `ApplicationRepository : GenericRepository<Application>` adds `GetByApplicationIdAsync()` methods
- **Special**: `TenantRepository` (standalone, not generic) because `Tenant` doesn't inherit `BaseEntity`

### 4. DTO Naming & Mapping
- Pattern: `{Entity}Dto`, `Create{Entity}Dto`, `Update{Entity}Dto`
- AutoMapper profiles in `Application/Mapping/MappingProfile.cs`
- **Pagination**: Use `PaginatedResponse<T>` (has `Items`, `TotalCount`, `Skip`, `Take`, `TotalPages`, `HasNextPage`)

### 5. Frontend Angular Conventions
- **Standalone Components**: No `NgModule`, use `imports: []` array directly
- **Route Guards**: `AuthGuard` uses `inject()` pattern (not constructor DI)
- **HTTP Interceptors**: `AuthInterceptor` auto-adds JWT + handles 401 refresh, `ErrorInterceptor` shows user feedback
- **API Config**: `src/app/core/services/api.config.ts` (not environment files) for base URLs
- **Lazy Loading**: Feature routes in `app.routes.ts` → `loadComponent: () => import(...)`

## Integration Points

### Backend → Database
- **Development**: SQLite (`Data Source=versionlifecycle.db`) - auto-detected if connection string starts with "Data Source="
- **Production**: PostgreSQL via `Npgsql.EntityFrameworkCore.PostgreSQL`
- **Switching**: `DesignTimeDbContextFactory` checks `ASPNETCORE_ENVIRONMENT` and connection string format

### Frontend → Backend
- **Base URL**: Angular services use `apiUrl = 'http://localhost:5000/api'` (hardcoded in `api.config.ts`)
- **CORS**: Backend allows `http://localhost:4200` and `https://localhost:4200` in `Program.cs`
- **Auth Flow**: Login → store JWT in localStorage → `AuthInterceptor` adds `Authorization: Bearer {token}` to all requests

### JWT Authentication
- **TokenService** (Infrastructure): Generates access tokens with `IConfiguration` for JWT settings
- **AuthController** (Web): `/api/auth/login`, `/api/auth/register`, `/api/auth/refresh`
- **LoginResponseDto**: Must include `TokenType = "Bearer"` property (set in all auth responses)

## Known Issues & Workarounds

1. **Multi-Tenancy**: `Tenant` entity uses `Id` (string), not `TenantId` - `TenantRepository` handles this separately
2. **Tenant Resolution**: `TenantResolutionMiddleware` resolves tenant from JWT token or defaults to first tenant in development

## Task Management Workflow

**All work must be tracked in TASKS.md** - a living document for project management.

### When Starting New Work:
1. Add task to appropriate section in `TASKS.md` (Current Sprint, Backlog, Bug Fixes, etc.)
2. Mark as `[IN PROGRESS]` when actively working
3. Update task with relevant details or sub-tasks if needed

### After Completing Each Task:
1. Mark task as `[✓]` in `TASKS.md`
2. Commit ALL changes including the updated `TASKS.md`:
   ```bash
   git add .
   git commit -m "task: <brief description of completed work>"
   git push origin main
   ```
3. Use descriptive commit messages that reference the task for traceability

### Task Document Structure:
- **Current Sprint**: Active and recently completed tasks
- **Backlog**: Planned features by priority (High/Medium/Low)
- **Bug Fixes**: Open and resolved issues
- **Technical Debt**: Code quality and maintenance items
- **Future Enhancements**: Long-term improvements

**Important**: Always update `TASKS.md` before committing. This ensures the repository history shows what was accomplished in each commit.

## Common Tasks

### Adding a New Entity
1. Create entity in `Core/Entities/` inheriting `BaseEntity`
2. Add `DbSet<YourEntity>` to `AppDbContext`
3. Create configuration method `ConfigureYourEntity(ModelBuilder)` in `AppDbContext`
4. Create DTOs in `Application/DTOs/`
5. Add AutoMapper mapping in `MappingProfile.cs`
6. Create repository if needed (or use `GenericRepository<YourEntity>`)
7. Create migration: `dotnet ef migrations add AddYourEntity`

### Testing API Endpoints
1. Start backend with `$env:ASPNETCORE_ENVIRONMENT="Development"; dotnet run --project VersionLifecycle.Web`
2. Open Swagger UI: http://localhost:5000/swagger
3. Authenticate: POST `/api/auth/register` → POST `/api/auth/login` → copy `accessToken`
4. Click "Authorize" button → paste token → test protected endpoints

### Frontend Component Creation
```bash
# Components are standalone - no need for module registration
cd VersionLifecycle.Web/ClientApp
ng generate component features/your-feature/your-component --standalone
# Add route to app.routes.ts with lazy loading
```

## Current Development Status (Dec 23, 2025)

✅ **Status**: Fully operational - all core features implemented  
✅ **Backend**: .NET 10 API with 7 controllers, all services wired, SQLite (dev) / PostgreSQL (prod)  
✅ **Frontend**: Angular 17+ with SignalStore state management, presentational/container pattern  
✅ **Database**: Migrations applied, test data seeded automatically in Development mode  
✅ **Documentation**: README.md, ARCHITECTURE.md, DEVELOPMENT.md all up-to-date

**Test Credentials** (Development):
- Tenant: `demo-tenant-001`
- Admin: `admin@example.com` / `Admin123!`
- Manager: `manager@example.com` / `Manager123!`
- Viewer: `viewer@example.com` / `Viewer123!`

See `TASKS.md` for current work items and backlog.
