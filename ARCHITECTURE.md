# Architecture Overview

## Introduction

Version Lifecycle Management is built using **Clean Architecture** principles with a clear separation of concerns across four distinct layers. The application supports **multi-tenancy** at the database level and uses modern patterns for both backend (.NET 10) and frontend (Angular 17+).

## Architecture Layers

### 1. Core Layer (`VersionLifecycle.Core`)

**Purpose:** Domain entities, business rules, and contracts - the heart of the application.

**Key Components:**
- **Entities/** - Domain models with business logic
  - `BaseEntity` - Base class providing `Id`, `TenantId`, audit properties (`CreatedAt`, `ModifiedAt`, `CreatedBy`, `ModifiedBy`), and soft delete (`IsDeleted`)
  - `Tenant` - Organization/team entity (special case: does not inherit `BaseEntity`)
  - `Application` - Software application aggregate root
  - `Version` - Application version/release
  - `Environment` - Deployment environment (Dev, Staging, Production, etc.)
  - `Deployment` - Deployment record linking Version + Environment
  - `DeploymentEvent` - Audit trail for deployment lifecycle
  - `Webhook` - Webhook registration for external notifications
  - `WebhookEvent` - Webhook delivery history

- **Enums/** - Status enumerations
  - `VersionStatus` - Draft, Released, Deprecated, Archived
  - `DeploymentStatus` - Pending, InProgress, Success, Failed, Cancelled

- **Interfaces/** - Core contracts
  - `IRepository<T>` - Generic repository interface
  - `ITenantContext` - Tenant resolution and context management

- **Exceptions/** - Domain-specific exceptions
  - `TenantIsolationException` - Multi-tenancy boundary violations
  - `InvalidDeploymentStatusException` - Invalid status transitions
  - `NotFoundException` - Entity not found

**Dependencies:** None (Core has zero external dependencies)

---

### 2. Application Layer (`VersionLifecycle.Application`)

**Purpose:** Business logic orchestration, DTOs, validation rules, and service contracts.

**Key Components:**
- **DTOs/** - Data Transfer Objects for API communication
  - Request DTOs: `CreateApplicationDto`, `UpdateApplicationDto`, etc.
  - Response DTOs: `ApplicationDto`, `VersionDto`, `DeploymentDto`, etc.
  - Auth DTOs: `LoginDto`, `RegisterDto`, `LoginResponseDto`
  - Pagination: `PaginatedResponse<T>`

- **Services/** - Service interfaces defining business operations
  - `IApplicationService`, `IVersionService`, `IDeploymentService`
  - `IEnvironmentService`, `IWebhookService`, `ITenantService`
  - `ITokenService` - JWT token generation/validation

- **Validators/** - FluentValidation rules for DTOs
  - Input validation (required fields, length constraints)
  - Business rule validation (semantic versioning, URL formats)
  - Security validation (password strength, webhook secrets)

- **Mapping/** - AutoMapper profiles
  - `MappingProfile` - Bidirectional mappings between entities and DTOs

**Dependencies:** Core layer only

---

### 3. Infrastructure Layer (`VersionLifecycle.Infrastructure`)

**Purpose:** Data access, external services, and cross-cutting concerns implementation.

**Key Components:**

#### Data Access (`Data/`)
- **AppDbContext** - Entity Framework Core DbContext
  - Fluent API entity configurations
  - Multi-tenant query filters (automatic `TenantId` filtering)
  - Audit property management (auto-sets `CreatedAt`, `ModifiedAt`, etc.)
  - Relationship mappings with cascading deletes
  - SQLite (development) / PostgreSQL (production) support

- **DataSeeder** - Development data seeding
  - Creates demo tenant, users, and sample application
  - Runs automatically in Development environment

- **DesignTimeDbContextFactory** - EF Core CLI support for migrations

#### Repositories (`Repositories/`)
- **GenericRepository<T>** - Base CRUD operations
  - `GetAllAsync()` - Retrieve all non-deleted entities (respects tenant filter)
  - `GetByIdAsync()` - Single entity retrieval
  - `AddAsync()`, `UpdateAsync()` - Persistence operations
  - `DeleteAsync()` - Soft delete (sets `IsDeleted = true`)
  - `ExistsAsync()`, `CountAsync()` - Existence checks

- **Specialized Repositories** - Domain-specific queries
  - `ApplicationRepository` - Eager loading of versions/environments
  - `VersionRepository` - Filter by application, version number lookups
  - `DeploymentRepository` - Paginated queries, event loading
  - `EnvironmentRepository` - Ordered by display order
  - `WebhookRepository` - Active webhooks, delivery history
  - `TenantRepository` - Tenant CRUD (does not use `GenericRepository<T>`)

#### Services (`Services/`)
- **ApplicationService**, **VersionService**, etc. - Business logic implementation
  - Orchestrate repository calls
  - Apply business rules
  - Use AutoMapper for DTO conversions
  - Inject `ITenantContext` for tenant awareness

- **TokenService** - JWT token operations
  - Generate access tokens with claims (user ID, tenant ID, roles)
  - Generate refresh tokens (secure random 64-byte)
  - Validate JWT signatures and expiration

#### Multi-Tenancy (`Multitenancy/`)
- **TenantContext** - Scoped service tracking current tenant and user
  - `CurrentTenantId` - Active tenant identifier
  - `CurrentUserId` - Active user identifier
  - `SetTenant()` - Populate context from middleware

**Dependencies:** Core, Application layers + EF Core, Npgsql

---

### 4. Web Layer (`VersionLifecycle.Web`)

**Purpose:** API endpoints, middleware, authentication, and frontend hosting.

**Key Components:**

#### Controllers (`Controllers/`)
- **AuthController** - Authentication endpoints
  - `POST /api/auth/login` - User login with tenant
  - `POST /api/auth/register` - New user registration
  - `POST /api/auth/refresh` - Token refresh

- **ApplicationsController** - Application CRUD
  - `GET /api/applications` - List with pagination
  - `POST /api/applications` - Create (Manager/Admin)
  - `PUT /api/applications/{id}` - Update (Manager/Admin)
  - `DELETE /api/applications/{id}` - Delete (Admin only)

- **VersionsController** - Nested under applications
  - `GET /api/applications/{applicationId}/versions`
  - Status transition support

- **DeploymentsController** - Deployment workflow
  - `POST /api/deployments` - Create pending deployment
  - `POST /api/deployments/{id}/confirm` - Confirm and transition to InProgress
  - `GET /api/deployments/{id}/events` - Event timeline

- **EnvironmentsController**, **WebhooksController**, **TenantsController** - Additional resources

#### Middleware (`Middleware/`)
- **TenantResolutionMiddleware** - Early-pipeline tenant detection
  - Extracts `tenantId` from JWT token (preferred)
  - Fallback: resolves from subdomain (e.g., `demo.example.com`)
  - Populates `ITenantContext` for scoped access

#### Frontend (`ClientApp/`)
- Angular 17+ single-page application
- Served via Nginx in production
- See Frontend Architecture section below

**Dependencies:** All layers + ASP.NET Core, Identity, Serilog

---

## Multi-Tenancy Implementation

### Database-Level Isolation

1. **TenantId Column** - Every entity (except `Tenant` itself) has a `TenantId` column inherited from `BaseEntity`

2. **Global Query Filters** - `AppDbContext` applies EF Core query filters automatically:
   ```csharp
   builder.Entity<T>().HasQueryFilter(e => e.TenantId == currentTenantId);
   ```
   This ensures queries always filter by tenant context.

3. **Automatic Population** - `AppDbContext.SaveChanges()` intercepts inserts and sets:
   - `TenantId` from `ITenantContext.CurrentTenantId`
   - `CreatedBy` from `ITenantContext.CurrentUserId`

### Request Flow

```
HTTP Request
  ↓
TenantResolutionMiddleware (extracts tenant from JWT/subdomain)
  ↓
ITenantContext.SetTenant(tenantId, userId)
  ↓
Authentication/Authorization
  ↓
Controller Action
  ↓
Service Layer (injects ITenantContext)
  ↓
Repository (queries filtered by TenantId via EF Core)
  ↓
Response
```

### Tenant Model Exception

`Tenant` entity does NOT inherit `BaseEntity`:
- Has `Id` property (string GUID) instead of `TenantId`
- Not subject to global query filters
- Uses `TenantRepository` (not `GenericRepository<Tenant>`)

---

## Frontend Architecture (Angular 17+)

### State Management: SignalStore

**Pattern:** Feature-scoped SignalStores manage local UI state and orchestrate data fetching.

**Key Stores:**
- **AuthStore** (`core/stores/auth.store.ts`)
  - Manages authentication state: `isAuthenticated`, `user`, `token`
  - Actions: `login()`, `register()`, `logout()`, `refreshToken()`
  - Persists token to localStorage
  - Auto-redirects on logout

- **ApplicationsStore** (`features/applications/applications.store.ts`)
  - Manages applications list, selected application, pagination
  - Actions: `loadApplications()`, `createApplication()`, `updateApplication()`, `deleteApplication()`
  - Computed signals: `isLoading()`, `hasNextPage()`, `currentPageNumber()`

- **DeploymentsStore** (`features/deployments/deployments.store.ts`)
  - Manages deployments list, filters, versions, environments
  - Actions: `loadDeployments()`, `loadVersions()`, `loadEnvironments()`, `createPendingDeployment()`, `confirmDeployment()`
  - Supports status filtering (Pending, InProgress, Success, etc.)

- **DashboardStore** (`features/dashboard/dashboard.store.ts`)
  - Aggregates data from multiple sources using RxJS `forkJoin`
  - Provides dashboard overview signals

**Benefits:**
- Eliminates manual subscription management (signals auto-track dependencies)
- Type-safe state access with TypeScript inference
- Single source of truth per feature
- Facilitates testing (stores can be mocked easily)

### Component Architecture: Presentational/Container Pattern

**Presentational Components** - Pure display logic
- Accept data via `@Input()` properties
- Emit user actions via `@Output()` events
- No direct HTTP calls or store injections
- Easily testable in isolation

**Container Components** - Data orchestration
- Inject SignalStores
- Subscribe to store signals (via async pipe in template)
- Pass data to presentational components
- Handle user events and dispatch store actions

**Example:**
```
applications-list.container.ts (injects ApplicationsStore)
  ↓ passes data + handles events
applications-list.component.ts (presentational, Inputs/Outputs)
```

### HTTP Interceptors

**AuthInterceptor** (`core/interceptors/auth.interceptor.ts`)
- Injects `Authorization: Bearer {token}` header on all requests
- Automatically refreshes token on 401 Unauthorized
- Retries failed request with new token

**ErrorInterceptor** (`core/interceptors/error.interceptor.ts`)
- Catches HTTP errors globally
- Displays user-friendly error messages
- Logs errors for debugging

### Routing & Guards

**AuthGuard** (`core/guards/auth.guard.ts`)
- Protects routes requiring authentication
- Redirects to `/login` if not authenticated
- Reads auth state from `AuthStore`

**Lazy Loading** - Feature modules loaded on-demand:
```typescript
{
  path: 'applications',
  loadComponent: () => import('./features/applications/list/applications-list.container')
}
```

---

## Repository Pattern

### Generic Repository

Base class providing standard CRUD operations:
```csharp
public class GenericRepository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public virtual async Task<IEnumerable<T>> GetAllAsync() { ... }
    public virtual async Task<T?> GetByIdAsync(int id) { ... }
    public virtual async Task<T> AddAsync(T entity) { ... }
    public virtual async Task<T> UpdateAsync(T entity) { ... }
    public virtual async Task<bool> DeleteAsync(int id) { ... }
}
```

**Features:**
- Automatic soft delete filtering (`!e.IsDeleted`)
- Tenant filtering applied via EF Core query filters
- `AsNoTracking()` for read-only queries

### Specialized Repositories

Extend `GenericRepository<T>` with domain-specific queries:
```csharp
public class VersionRepository : GenericRepository<Version>
{
    public async Task<IEnumerable<Version>> GetByApplicationIdAsync(int applicationId)
    {
        return await _dbSet
            .Where(v => v.ApplicationId == applicationId && !v.IsDeleted)
            .OrderByDescending(v => v.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }
}
```

---

## Key Design Decisions

### 1. Soft Deletes
- All entities have `IsDeleted` flag
- Repository queries filter out deleted entities
- Enables audit trails and data recovery

### 2. Audit Properties
- `CreatedAt`, `ModifiedAt`, `CreatedBy`, `ModifiedBy` tracked automatically
- Set in `AppDbContext.SaveChanges()` using EF Core change tracking

### 3. Pending Deployment Workflow
- Two-step deployment: Create (Pending) → Confirm (InProgress)
- Prevents accidental deployments
- Allows review/approval before execution

### 4. JWT Token Strategy
- Short-lived access tokens (60 minutes)
- Refresh tokens for seamless renewal
- Tokens include `tenantId` claim for multi-tenancy

### 5. SignalStore Over NgRx
- Simpler API for feature-scoped state
- Better TypeScript inference
- Less boilerplate than full NgRx setup
- Still reactive and testable

### 6. Standalone Components
- Modern Angular pattern (no `NgModule` required)
- Explicit imports in component metadata
- Better tree-shaking for smaller bundles

### 7. SQLite for Development
- No external database setup required
- Fast migrations and testing
- Automatic switch to PostgreSQL in production

---

## Database Schema Overview

### Core Relationships

```
Tenant (1) ──────< (n) Application (1) ──────< (n) Version
                          │                           │
                          │                           │
                          ├──────< (n) Environment    │
                          │              │            │
                          │              └────< (n) Deployment (n) >────┘
                          │                           │
                          │                           ├──< (n) DeploymentEvent
                          │
                          └──────< (n) Webhook
                                         │
                                         └──────< (n) WebhookEvent
```

### Indexes
- Unique constraints: `(TenantId, Name)` on Applications, Environments
- Unique constraint: `(ApplicationId, VersionNumber)` on Versions
- Unique constraint: `(ApplicationId, VersionId, EnvironmentId)` on Deployments
- Index on `DeployedAt` for timeline queries

---

## Testing Strategy

### Unit Tests
- **Services**: Mock repositories, test business logic in isolation
- **Repositories**: Use in-memory SQLite for fast tests
- **Components**: Shallow rendering with mocked stores

### Integration Tests
- **Controllers**: Test full request/response cycle with TestServer
- **Database**: Use test database with migrations applied

### E2E Tests
- **User Workflows**: Selenium/Playwright for full frontend flows
- **API Contracts**: Verify Swagger spec matches implementation

---

## Security Considerations

1. **Multi-Tenancy Enforcement**
   - Global query filters prevent cross-tenant data access
   - Middleware validates tenant on every request

2. **Authorization Policies**
   - `AdminOnly` - Full access to all resources
   - `ManagerOrAdmin` - Create/update operations
   - Viewer role (default) - Read-only access

3. **Input Validation**
   - FluentValidation on all DTOs
   - ASP.NET Core model validation
   - Client-side Angular form validation

4. **JWT Security**
   - Tokens signed with HS256 symmetric key
   - Short expiration (60 minutes)
   - Refresh tokens stored securely

5. **SQL Injection Protection**
   - Entity Framework parameterized queries
   - No raw SQL unless explicitly required

---

## Deployment Architecture

### Development
```
Angular Dev Server (localhost:4200)
  ↓ HTTP proxy
.NET API (localhost:5000)
  ↓
SQLite (versionlifecycle.db)
```

### Production (Docker Compose)
```
Nginx (port 80)
  ├─> /api/* → .NET API Container (port 5000)
  └─> /* → Static Angular build
           
.NET API Container
  ↓
PostgreSQL Container (port 5432)
```

---

## Performance Optimizations

1. **Lazy Loading** - Angular routes and components loaded on-demand
2. **AsNoTracking()** - EF Core read queries don't track changes
3. **Pagination** - All list endpoints support `skip`/`take` parameters
4. **Indexes** - Strategic database indexes on foreign keys and common filters
5. **Connection Pooling** - EF Core connection pool configured
6. **Gzip Compression** - Nginx compresses responses
7. **Bundle Optimization** - Angular production build with tree-shaking

---

## Future Enhancements

- **Caching**: Redis for frequently accessed data
- **Background Jobs**: Hangfire for webhook delivery, cleanup tasks
- **Real-time Updates**: SignalR for live deployment status
- **Audit Logging**: Comprehensive change tracking
- **Kubernetes**: Helm charts for orchestration
- **CI/CD**: GitHub Actions pipeline

---

For development setup, see [DEVELOPMENT.md](./DEVELOPMENT.md).  
For API endpoints, see Swagger at `/swagger`.
