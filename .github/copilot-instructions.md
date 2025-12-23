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

### 6. Frontend State Management (SignalStore)

**Pattern**: Feature-scoped SignalStores manage local UI state and orchestrate data fetching using Angular signals for reactivity.

#### Key Stores (all root-provided)

**AuthStore** (`core/stores/auth.store.ts`):
```typescript
export const AuthStore = signalStore(
  { providedIn: 'root' },
  withState({ user: null, token: null, isAuthenticated: false, loading: false, error: null }),
  withMethods((store, authService = inject(AuthService), router = inject(Router)) => ({
    login: rxMethod<LoginDto>(...),
    register: rxMethod<RegisterDto>(...),
    logout: () => { localStorage.removeItem('token'); router.navigate(['/login']); }
  }))
);
```
- Manages authentication state: `isAuthenticated()`, `user()`, `token()`
- Actions: `login()`, `register()`, `logout()`, `refreshToken()`
- Persists token to localStorage
- Auto-redirects to `/login` on logout

**ApplicationsStore** (`features/applications/applications.store.ts`):
```typescript
export const ApplicationsStore = signalStore(
  { providedIn: 'root' },
  withState({ applications: [], selectedApplication: null, loading: false, error: null, skip: 0, take: 25, totalCount: 0 }),
  withComputed((state) => ({
    hasNextPage: computed(() => state.skip() + state.take() < state.totalCount()),
    currentPageNumber: computed(() => Math.floor(state.skip() / state.take()) + 1)
  })),
  withMethods((store, appService = inject(ApplicationService)) => ({
    loadApplications: rxMethod<void>(...),
    createApplication: rxMethod<CreateApplicationDto>(...),
    updateApplication: rxMethod<{id: number, dto: UpdateApplicationDto}>(...),
    deleteApplication: rxMethod<number>(...)
  }))
);
```
- Manages applications list, selected application, pagination state
- Computed signals: `hasNextPage()`, `currentPageNumber()`
- Auto-sets `selectedApplication` after create for navigation

**DeploymentsStore** (`features/deployments/deployments.store.ts`):
```typescript
export const DeploymentsStore = signalStore(
  { providedIn: 'root' },
  withState({ 
    deployments: [], selectedDeployment: null, events: [], 
    versions: [], environments: [],
    loading: false, error: null, skip: 0, take: 25, totalCount: 0, status: null 
  }),
  withMethods((store, depService = inject(DeploymentService)) => ({
    loadDeployments: rxMethod<{skip?: number, take?: number, status?: string}>(...),
    loadDeploymentEvents: rxMethod<number>(...),
    confirmDeployment: rxMethod<number>(...),
    loadVersions: rxMethod<number>(...),        // for deployment creation
    loadEnvironments: rxMethod<number>(...),     // for deployment creation
    createPendingDeployment: rxMethod<CreatePendingDeploymentDto>(...)
  }))
);
```
- Manages deployments list, filters (status), versions, environments
- Supports status filtering: `{ status: 'Pending' }` in `loadDeployments()`
- Pre-loads versions/environments for multi-step deployment wizard

**DashboardStore** (`features/dashboard/dashboard.store.ts`):
```typescript
export const DashboardStore = signalStore(
  { providedIn: 'root' },
  withState({ applications: [], recentDeployments: [], loading: false, error: null }),
  withMethods((store, appService = inject(ApplicationService), depService = inject(DeploymentService)) => ({
    loadDashboard: rxMethod<void>(
      pipe(
        switchMap(() => forkJoin({
          apps: appService.getApplications(0, 10),
          deployments: depService.getDeployments(0, 10)
        })),
        tapResponse(...)
      )
    )
  }))
);
```
- Aggregates data from multiple sources using RxJS `forkJoin`
- Provides dashboard overview signals

#### Presentational/Container Pattern

**Presentational Components** - Pure display logic:
- Accept data via `@Input()` properties (signals or plain values)
- Emit user actions via `@Output()` events
- NO direct HTTP calls or store injections
- Easily testable in isolation

**Container Components** - Data orchestration:
- Inject SignalStores
- Subscribe to store signals (pass to presentational via inputs)
- Handle `@Output()` events and dispatch store actions
- Named with `.container.ts` suffix

**Example Structure**:
```
applications/
├── list/
│   ├── applications-list.component.ts         ← Presentational (@Input/@Output)
│   ├── applications-list.component.html
│   └── applications-list.container.ts          ← Container (injects ApplicationsStore)
├── detail/
│   ├── applications-detail.component.ts        ← Presentational
│   ├── applications-detail.component.html
│   └── applications-detail.container.ts         ← Container
└── applications.store.ts                        ← SignalStore (root-provided)
```

**Route Configuration** (loads containers):
```typescript
{
  path: 'applications',
  children: [
    { path: '', loadComponent: () => import('./list/applications-list.container') },
    { path: 'new', loadComponent: () => import('./detail/applications-detail.container') },
    { path: ':id', loadComponent: () => import('./detail/applications-detail.container') }
  ]
}
```

#### RxJS Interop with rxMethod

**Pattern**: Use `rxMethod()` from `@ngrx/signals/rxjs-interop` for async operations:
```typescript
withMethods((store, service = inject(MyService)) => ({
  loadData: rxMethod<void>(
    pipe(
      tap(() => patchState(store, { loading: true, error: null })),
      switchMap(() => service.getData()),
      tapResponse(
        (data) => patchState(store, { data, loading: false }),
        (error: HttpErrorResponse) => patchState(store, { error: error.message, loading: false })
      )
    )
  )
}))
```

**Benefits**:
- Automatic subscription management (no manual `subscribe()` needed)
- Built-in error handling with `tapResponse`
- Type-safe state updates with `patchState`
- Signals auto-track dependencies in templates

#### Critical Rules

1. **Root-Provided Stores**: All stores use `{ providedIn: 'root' }` to avoid DI errors
2. **No Direct Service Injection in Presentational Components**: Only containers inject stores/services
3. **Effect Creation in Injection Context**: Use class field initialization or constructor for `effect()`:
   ```typescript
   private autoSaveEffect = effect(() => { /* ... */ });  // ✅ Correct
   // NOT in ngOnInit(): effect(() => { /* ... */ });     // ❌ Wrong (NG02003 error)
   ```
4. **Signal-Based Templates**: Use `()` to read signals in templates:
   ```html
   @if (store.loading()) { <spinner /> }
   @for (app of store.applications(); track app.id) { ... }
   ```

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
