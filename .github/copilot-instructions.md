# GitHub Copilot Instructions for Version Lifecycle Management

## Quick Reference

### Essential Commands
```bash
# Backend Development
dotnet run --project VersionLifecycle.Web                    # Start API (port 5000)
dotnet test                                                   # Run all tests
dotnet ef migrations add <Name> --project VersionLifecycle.Infrastructure --startup-project VersionLifecycle.Web
dotnet ef database update --project VersionLifecycle.Infrastructure --startup-project VersionLifecycle.Web

# Frontend Development
cd VersionLifecycle.Web/ClientApp
npm start                                                     # Start dev server (port 4200)
npm test                                                      # Run tests
npm run build                                                 # Production build
npm run lint                                                  # Lint code

# Docker
docker-compose up --build                                    # Start all services
docker-compose down                                          # Stop all services
```

### Key Files to Know
- `Program.cs` - Dependency injection, middleware pipeline, service registration
- `AppDbContext.cs` - Entity configurations, query filters, audit properties
- `copilot-instructions.md` (this file) - Development guidelines
- `TASKS.md` - Current work items and backlog
- `ARCHITECTURE.md` - Detailed architecture documentation
- `DEVELOPMENT.md` - Setup and troubleshooting guide
- `docs/FRONTEND_PR_CHECKLIST.md` - **MANDATORY frontend patterns checklist - review before every frontend change**

### Common Patterns at a Glance
```csharp
// Backend: Service dependency injection pattern
builder.Services.AddScoped<MyService>();
builder.Services.AddScoped<IMyService>(sp => sp.GetRequiredService<MyService>());

// Backend: Repository usage with tenant context
private readonly IRepository<MyEntity> _repository;
private readonly ITenantContext _tenantContext;
// Query automatically filtered by TenantId via global filters

// Backend: Soft delete (already handled in GenericRepository)
entity.IsDeleted = true; // Set flag instead of removing from database
```

```typescript
// Frontend: SignalStore pattern
export const MyStore = signalStore(
  { providedIn: 'root' },
  withState({ items: [], loading: false }),
  withMethods((store, service = inject(MyService)) => ({
    loadItems: rxMethod<void>(/* ... */)
  }))
);

// Frontend: Container/Presentational pattern
// Container: Injects store, passes data to presentational
// Presentational: @Input/@Output only, no store injection
```

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

### Code Quality & Linting

**Backend (.NET):**
```bash
# Build with warnings as errors (recommended for CI)
dotnet build /p:TreatWarningsAsErrors=true

# Format code (requires dotnet-format tool)
dotnet format

# Analyze code for issues
dotnet build /p:RunAnalyzers=true
```

**Frontend (Angular/TypeScript):**
```bash
cd VersionLifecycle.Web/ClientApp

# Lint TypeScript code
npm run lint

# Lint with auto-fix
npm run lint -- --fix

# Format code (if prettier is configured)
npm run format
```

**Code Style Guidelines:**
- **C#**: Follow Microsoft C# coding conventions
  - PascalCase for classes, methods, properties
  - camelCase for parameters and local variables
  - Use `var` when type is obvious
  - Keep methods focused and under 50 lines when possible
- **TypeScript**: Follow Angular style guide
  - Use strict mode (`"strict": true` in tsconfig)
  - PascalCase for classes and interfaces
  - camelCase for variables, functions, and properties
  - Use `const` by default, `let` when reassignment needed
  - Avoid `any` type - use proper typing

## Critical Patterns & Conventions

### MANDATORY: Frontend Development Checklist

**BEFORE making ANY frontend changes, you MUST:**
1. Review `docs/FRONTEND_PR_CHECKLIST.md` for all requirements
2. Verify no direct service injection in presentational components
3. Ensure all state uses SignalStore (no local state for shared data)
4. Check effects are created as class fields, not in lifecycle hooks
5. Confirm container/presentational split is maintained

**Common Violations to Avoid:**
- ❌ Injecting services into presentational components
- ❌ Creating effects in `ngOnInit()` or other lifecycle hooks
- ❌ Using manual `subscribe()` instead of `rxMethod()`
- ❌ Storing shared data in component state instead of SignalStore
- ❌ Forgetting `()` syntax when reading signals in templates

**If uncertain about a pattern, consult the checklist FIRST before implementing.**

---

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

## Security Best Practices

### Authentication & Authorization
- **JWT Tokens**: Always use HTTPS in production for token transmission
- **Token Storage**: Frontend stores tokens in localStorage (consider HttpOnly cookies for enhanced security in production)
- **Token Expiration**: Access tokens expire after 60 minutes; use refresh tokens for renewal
- **Password Requirements**: Minimum 8 characters, must include uppercase, lowercase, digit, and special character

### Input Validation
- **Backend**: FluentValidation on all DTOs - NEVER trust client input
- **Frontend**: Angular reactive forms with validators - provides immediate user feedback
- **SQL Injection**: Entity Framework parameterized queries protect against SQL injection
- **XSS Protection**: Angular sanitizes HTML by default; avoid using `bypassSecurityTrust*` unless absolutely necessary

### Multi-Tenant Security
- **Data Isolation**: Global query filters enforce `TenantId` filtering on all queries
- **Authorization Checks**: Always verify user has access to tenant resources in service layer
- **Tenant Context**: `ITenantContext` populated by middleware; never allow client to specify tenant
- **Audit Trail**: `CreatedBy`, `ModifiedBy` tracked automatically for all entities

### API Security Checklist
- [ ] All endpoints (except `/api/auth/*`) require JWT authentication
- [ ] Role-based authorization: Use `[Authorize(Policy = "ManagerOrAdmin")]` appropriately
- [ ] Rate limiting configured in `Program.cs` to prevent abuse
- [ ] CORS configured with specific origins (not `*` in production)
- [ ] Request size limits enforced to prevent DoS attacks
- [ ] Sensitive data (passwords, tokens) never logged or returned in responses

## Performance Optimization

### Backend Performance
- **Query Optimization**: Use `AsNoTracking()` for read-only queries to improve performance
- **Pagination**: All list endpoints support `skip`/`take` - default 25 items per page
- **Eager Loading**: Use `.Include()` strategically to avoid N+1 queries
- **Indexing**: Database indexes on foreign keys and frequently filtered columns (already configured)
- **Connection Pooling**: EF Core manages connection pool automatically

### Frontend Performance
- **Lazy Loading**: Routes use `loadComponent()` for on-demand loading
- **OnPush Change Detection**: Components use `ChangeDetectionStrategy.OnPush` where possible
- **Signal-Based Reactivity**: SignalStore provides efficient change detection
- **Bundle Optimization**: Production build uses tree-shaking and minification
- **HTTP Caching**: Consider adding cache headers for static resources

### Database Performance
- **SQLite (Development)**: Fast for local development but limited concurrent writes
- **PostgreSQL (Production)**: Optimized for concurrent access and large datasets
- **Soft Deletes**: Queries filter `IsDeleted = false` - consider archival strategy for old data
- **Regular Maintenance**: PostgreSQL requires periodic `VACUUM` and `ANALYZE`

## Troubleshooting Common Issues

### Backend Issues

**Problem**: "Unable to resolve service for type 'X'"
```
Solution: Check Program.cs for service registration:
builder.Services.AddScoped<XService>();
builder.Services.AddScoped<IXService>(sp => sp.GetRequiredService<XService>());
```

**Problem**: "A second operation started on this context"
```
Solution: AppDbContext is scoped - ensure you're not sharing context across threads.
Make services scoped (not singleton) when injecting DbContext.
```

**Problem**: "Tenant query filter not applied"
```
Solution: Verify entity inherits BaseEntity and is configured in AppDbContext.
Check ITenantContext.CurrentTenantId is set by TenantResolutionMiddleware.
```

**Problem**: Migration fails with "column already exists"
```
Solution: 
1. Check migration files for duplicates
2. Revert with: dotnet ef database update <PreviousMigration>
3. Remove bad migration: dotnet ef migrations remove
4. Recreate migration with correct changes
```

### Frontend Issues

**Problem**: "NG0203: inject() must be called from an injection context"
```
Solution: Don't create effects in ngOnInit() or other lifecycle hooks.
Use class field initialization: private effect = effect(() => {...});
```

**Problem**: "Store not provided in root"
```
Solution: Ensure all stores use { providedIn: 'root' } in signalStore() config.
```

**Problem**: "ExpressionChangedAfterItHasBeenCheckedError"
```
Solution: 
1. Use ChangeDetectorRef.detectChanges() if updating view from effect
2. Consider using setTimeout() for async state updates
3. Use OnPush change detection strategy
```

**Problem**: HTTP interceptor infinite loop on 401
```
Solution: AuthInterceptor should skip token refresh requests to avoid recursion.
Check the excludeUrls array includes '/api/auth/refresh'.
```

### Build Issues

**Problem**: ".NET build fails with package restore errors"
```
Solution:
dotnet clean
dotnet restore
dotnet build
```

**Problem**: "Angular build fails with module not found"
```
Solution:
rm -rf node_modules package-lock.json
npm install
npm run build
```

**Problem**: "Docker build fails with COPY error"
```
Solution: Ensure .dockerignore doesn't exclude necessary files.
Check that node_modules is excluded to avoid context size issues.
```

## Git Workflow & Version Control

### Branching Strategy

**Main Branch**: `main` (or `master`) - Always production-ready
```bash
# Create feature branch
git checkout -b feature/your-feature-name
git checkout -b bugfix/issue-description
git checkout -b hotfix/critical-fix
```

**Branch Naming Conventions:**
- `feature/` - New features or enhancements
- `bugfix/` - Bug fixes
- `hotfix/` - Critical production fixes
- `refactor/` - Code refactoring
- `docs/` - Documentation updates
- `test/` - Test additions or updates

### Commit Message Format

Follow conventional commits pattern:
```
<type>(<scope>): <subject>

<body>

<footer>
```

**Types:**
- `feat:` - New feature
- `fix:` - Bug fix
- `docs:` - Documentation changes
- `style:` - Code style changes (formatting, no logic change)
- `refactor:` - Code refactoring
- `test:` - Adding or updating tests
- `chore:` - Maintenance tasks (build, dependencies)

**Examples:**
```bash
git commit -m "feat(applications): add pagination to applications list"
git commit -m "fix(auth): resolve token refresh infinite loop"
git commit -m "docs(readme): update setup instructions for SQLite"
git commit -m "test(deployments): add unit tests for deployment service"
```

### Pre-Commit Checklist

Before committing:
- [ ] Code compiles without errors: `dotnet build`
- [ ] All tests pass: `dotnet test` and `npm test`
- [ ] Code is formatted: `dotnet format` and `npm run lint -- --fix`
- [ ] No sensitive data (passwords, API keys) in code
- [ ] Update `TASKS.md` with progress
- [ ] Write descriptive commit message

### Pull Request Process

1. **Create PR** against `main` branch
2. **PR Title**: Use conventional commit format
3. **PR Description**: Include:
   - What changed and why
   - Related issues/tasks
   - Testing performed
   - Screenshots (for UI changes)
4. **Request Review** from at least one team member
5. **Address Feedback** promptly
6. **Merge** after approval (use squash merge for clean history)

### Working with .gitignore

**Critical**: Never commit:
- `bin/` and `obj/` directories (.NET build outputs)
- `node_modules/` (npm packages)
- `*.db`, `*.db-shm`, `*.db-wal` (SQLite database files)
- `.env` files (environment secrets)
- `.vs/`, `.vscode/` (IDE settings)
- `dist/` (Angular build output)

If accidentally committed:
```bash
# Remove from git but keep locally
git rm --cached <file>
git commit -m "chore: remove accidentally committed file"

# Update .gitignore to prevent future commits
echo "<pattern>" >> .gitignore
```

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

## Testing Strategy

### Running Tests

**Backend Tests:**
```bash
# Run all .NET tests
dotnet test

# Run tests with coverage
dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover

# Run specific test class
dotnet test --filter "FullyQualifiedName~YourTestClass"
```

**Frontend Tests:**
```bash
cd VersionLifecycle.Web/ClientApp

# Run Angular unit tests
npm test

# Run tests with coverage
npm test -- --code-coverage

# Run tests in headless mode (CI)
npm test -- --watch=false --browsers=ChromeHeadless
```

### Writing Tests

**Backend Unit Tests** (xUnit pattern):
```csharp
public class ApplicationServiceTests
{
    private readonly Mock<IRepository<Application>> _mockRepo;
    private readonly Mock<ITenantContext> _mockTenantContext;
    private readonly Mock<IMapper> _mockMapper;
    private readonly ApplicationService _service;

    public ApplicationServiceTests()
    {
        _mockRepo = new Mock<IRepository<Application>>();
        _mockTenantContext = new Mock<ITenantContext>();
        _mockMapper = new Mock<IMapper>();
        _service = new ApplicationService(_mockRepo.Object, _mockTenantContext.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnApplications()
    {
        // Arrange
        var applications = new List<Application> { new Application { Id = 1, Name = "Test" } };
        _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(applications);
        
        // Act
        var result = await _service.GetAllAsync();
        
        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
    }
}
```

**Frontend Component Tests** (Jasmine/Karma):
```typescript
describe('ApplicationsListComponent', () => {
  let component: ApplicationsListComponent;
  let fixture: ComponentFixture<ApplicationsListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ApplicationsListComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(ApplicationsListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should emit deleteApplication event when delete is clicked', () => {
    // Arrange
    const application = { id: 1, name: 'Test App' };
    let emittedId: number | undefined;
    component.deleteApplication.subscribe((id: number) => emittedId = id);

    // Act
    component.onDeleteClick(application);

    // Assert
    expect(emittedId).toBe(1);
  });
});
```

**Store Tests** (SignalStore):
```typescript
describe('ApplicationsStore', () => {
  let store: InstanceType<typeof ApplicationsStore>;
  let applicationService: jasmine.SpyObj<ApplicationService>;

  beforeEach(() => {
    const spy = jasmine.createSpyObj('ApplicationService', ['getApplications']);
    
    TestBed.configureTestingModule({
      providers: [
        ApplicationsStore,
        { provide: ApplicationService, useValue: spy }
      ]
    });
    
    store = TestBed.inject(ApplicationsStore);
    applicationService = TestBed.inject(ApplicationService) as jasmine.SpyObj<ApplicationService>;
  });

  it('should load applications', (done) => {
    // Arrange
    const mockApps = { items: [{ id: 1, name: 'Test' }], totalCount: 1 };
    applicationService.getApplications.and.returnValue(of(mockApps));

    // Act
    store.loadApplications();

    // Assert
    setTimeout(() => {
      expect(store.applications()).toEqual(mockApps.items);
      expect(store.loading()).toBeFalse();
      done();
    }, 100);
  });
});
```

### Test Data Setup

**In-Memory Database for Tests:**
```csharp
public class InMemoryDbContextFixture : IDisposable
{
    public AppDbContext Context { get; private set; }

    public InMemoryDbContextFixture()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        Context = new AppDbContext(options);
        Context.Database.EnsureCreated();
        SeedTestData();
    }

    private void SeedTestData()
    {
        var tenant = new Tenant { Id = "test-tenant", Name = "Test Tenant" };
        Context.Set<Tenant>().Add(tenant);
        Context.SaveChanges();
    }

    public void Dispose()
    {
        Context?.Dispose();
    }
}
```

### Integration Testing

**API Controller Tests:**
```csharp
public class ApplicationsControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ApplicationsControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetApplications_ReturnsSuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/api/applications");

        // Assert
        response.EnsureSuccessStatusCode();
    }
}
```

### Testing Best Practices

1. **Arrange-Act-Assert (AAA) Pattern**: Structure all tests with clear setup, execution, and verification
2. **Mock External Dependencies**: Use `Mock<T>` for repositories, services, and HTTP clients
3. **Test One Thing**: Each test should verify a single behavior or outcome
4. **Descriptive Names**: Use `MethodName_Scenario_ExpectedBehavior` naming convention
5. **Avoid Test Interdependence**: Tests should run independently and in any order
6. **Use Test Fixtures**: Share setup code with `IClassFixture` or `beforeEach`
7. **Test Edge Cases**: Include null checks, empty collections, and boundary values
8. **Keep Tests Fast**: Use in-memory databases and mocks to avoid slow I/O operations

## CI/CD & Deployment

### Continuous Integration

**Recommended GitHub Actions Workflow:**
```yaml
name: CI/CD Pipeline

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  backend-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '10.0.x'
      - run: dotnet restore
      - run: dotnet build --no-restore
      - run: dotnet test --no-build --verbosity normal

  frontend-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-node@v3
        with:
          node-version: '18'
      - run: cd VersionLifecycle.Web/ClientApp && npm ci
      - run: cd VersionLifecycle.Web/ClientApp && npm run build
      - run: cd VersionLifecycle.Web/ClientApp && npm test -- --watch=false --browsers=ChromeHeadless
```

### Environment Configuration

**Development:**
- SQLite database (no external dependencies)
- Automatic data seeding
- Detailed error messages
- CORS allows localhost origins
- Swagger UI enabled

**Production:**
- PostgreSQL database (via connection string)
- No data seeding
- Generic error messages (security)
- CORS configured for specific domains
- Swagger UI disabled (or secured)
- HTTPS required
- Environment variables for secrets

**Environment Variables (Production):**
```bash
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection="Host=...;Database=...;Username=...;Password=..."
Jwt__Key="<strong-secret-key>"
Jwt__Issuer="VersionLifecycleApp"
Jwt__Audience="VersionLifecycleUsers"
```

### Docker Deployment

**Build Multi-Container Stack:**
```bash
# Production build
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up --build -d

# View logs
docker-compose logs -f

# Scale API instances
docker-compose up --scale api=3
```

**Health Checks:**
- Backend: `GET /api/health` - Returns 200 if healthy
- Database: EF Core checks connection on startup
- Frontend: Nginx serves static files (no health check needed)

### Database Migration in Production

**IMPORTANT**: Run migrations before deploying new code
```bash
# Option 1: Run migration in existing container
docker exec versionlifecycle-api dotnet ef database update

# Option 2: Separate migration job in CI/CD
dotnet ef database update --connection "<prod-connection-string>"

# Option 3: SQL script generation for DBA approval
dotnet ef migrations script > migration.sql
```

### Monitoring & Logging

**Logging (Serilog):**
- Structured JSON logs
- Log to console (captured by Docker/Kubernetes)
- Consider: Elasticsearch, Application Insights, or CloudWatch

**Metrics to Track:**
- API response times (p50, p95, p99)
- Error rates by endpoint
- Active user sessions
- Database query performance
- Failed authentication attempts

**Alerting:**
- High error rates (>5% in 5 minutes)
- Slow response times (>2s average)
- Failed health checks
- Database connection failures

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
