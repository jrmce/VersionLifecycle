# Version Lifecycle Management Application

A comprehensive, production-ready application for tracking software development project versions and their deployment lifecycle across multiple environments. **Now with complete Angular frontend!**

## Status

**🚀 OPERATIONAL** - All core features implemented and functional (Dec 23, 2025)

- ✅ **Phase 1-2: Backend API** - COMPLETE
- ✅ **Phase 3: Angular Frontend** - COMPLETE  
- ✅ **Phase 4: Local Testing & Integration** - COMPLETE
  - Migrated to .NET 10
  - SQLite for local development
  - SignalStore state management
  - All services and repositories wired
- ✅ **Phase 5: Ready for Production Deployment**

## Features

### Backend Features
- **Multi-Tenant Architecture**: Isolated data and configurations per tenant
- **Application Management**: Create and manage software applications
- **Version Tracking**: Track versions through different statuses (Draft, Released, Deprecated, Archived)
- **Environment Management**: Create custom deployment environments (Dev, Test, Staging, Prod, etc.)
- **Deployment Workflow**: Pending → InProgress → Success/Failed/Cancelled states
- **Event Timeline**: Track all deployment events with timestamps and messages
- **Webhook Integration**: Automatic notifications to external systems on deployment events
- **Role-Based Access Control**: Secure API endpoints with JWT authentication
- **REST API**: 30+ RESTful endpoints for programmatic access
- **Health Checks**: Kubernetes-ready health endpoints

### Frontend Features
- **User Authentication**: Login and registration with JWT tokens
- **Dashboard**: Overview of applications and recent deployments
- **Applications Management**: Create, read, update, delete applications
- **Deployments Management**:
  - List deployments with status filtering
  - Multi-step deployment creation wizard
  - Visual environment timeline representation
  - Event timeline tracking
- **Responsive Design**: Mobile-friendly interface
- **Real-time Error Handling**: Global error interceptor with user feedback
- **Automatic Token Refresh**: Seamless JWT token renewal
- **Form Validation**: Real-time validation with user feedback
- **Lazy-Loaded Routes**: Optimized bundle with code splitting

## Technology Stack

### Backend
- **.NET 10** with Clean Architecture
- **ASP.NET Core** Web API with 7 controllers
- **Entity Framework Core** with PostgreSQL (production) / SQLite (development)
- **JWT** Authentication with automatic refresh
- **Serilog** for structured logging
- **Fluent Validation** for DTOs
- **AutoMapper** for DTO transformations

### Frontend
- **Angular 17+** with standalone components
- **TypeScript 5** for type safety
- **RxJS** for reactive programming
- **SCSS** for advanced styling
- **Reactive Forms** for form handling
- **HTTP Interceptors** for JWT and error handling
- **Lazy Loading** with feature routes

### Infrastructure
- **PostgreSQL** database with multi-tenant support
- **Docker & Docker Compose** for containerization
- **Nginx** reverse proxy with SPA routing
- **Node.js** for frontend build tools

## Project Structure

```
VersionLifecycle/
├── VersionLifecycle.Core/                          # Domain layer
│   ├── Entities/                                   # 8 domain models
│   ├── Enums/                                      # Status enumerations
│   ├── Exceptions/                                 # Custom exceptions
│   └── Interfaces/                                 # Contracts
├── VersionLifecycle.Application/                   # Business logic
│   ├── DTOs/                                       # 8 DTO classes
│   ├── Services/                                   # Service interfaces
│   ├── Validators/                                 # FluentValidation rules
│   └── Mapping/                                    # AutoMapper profiles
├── VersionLifecycle.Infrastructure/                # Data access
│   ├── Data/                                       # DbContext and seeding
│   ├── Multitenancy/                               # Tenant isolation
│   ├── Repositories/                               # Generic repository
│   └── Services/                                   # TokenService, etc.
├── VersionLifecycle.Web/                           # API layer
│   ├── Controllers/                                # 7 API controllers
│   ├── Middleware/                                 # Middleware components
│   ├── Authorization/                              # Auth handlers
│   └── ClientApp/                                  # 🆕 Angular Frontend
│       ├── src/
│       │   ├── app/
│       │   │   ├── core/                           # Services, guards, models
│       │   │   ├── features/                       # Feature components
│       │   │   ├── app.ts                          # Root component
│       │   │   └── app.scss                        # Global styles
│       │   └── environments/                       # Environment config
│       └── dist/                                   # Production build
├── VersionLifecycle.Tests/                         # Unit tests
├── Dockerfile                                      # .NET API container
├── docker-compose.yml                              # Multi-container orchestration
├── nginx.conf                                      # Reverse proxy configuration
└── Documentation/
    ├── PHASE_1_2_COMPLETE.md                       # Backend details
    ├── PHASE_3_COMPLETE.md                         # 🆕 Frontend details
    ├── README.md                                   # This file
    ├── DEVELOPMENT.md                              # Setup guide
    └── NEXT_STEPS.md                               # Implementation guide
```

## Current Build Status

### Backend (Phase 1-2)
✅ **Complete and Functional**
- 25+ C# classes and services
- 7 API controllers with 30+ endpoints
- Complete data layer with EF Core
- JWT authentication and authorization
- Data seeding with sample data
- All CRUD operations implemented

### Frontend (Phase 3)
✅ **Complete and Successfully Built**
- 15+ standalone Angular components
- 6 API services with full CRUD support
- Authentication system with JWT token refresh
- Dashboard with overview data
- Dashboard, Applications, and Deployments features
- Responsive SCSS styling
- **Build Output**: 265.66 kB initial bundle (73.09 kB compressed)
- Lazy-loaded feature chunks for optimized loading
- No compilation errors

### Integration Status
✅ Services communicate via HTTP interceptors
✅ Authentication flow fully implemented
✅ Error handling in place
✅ Form validation working
✅ Routing with guards protecting routes

## Quick Start

### Prerequisites

- .NET 10 SDK
- Node.js 18+ with npm
- PostgreSQL 14+ (production) or SQLite (development)
- Docker & Docker Compose (for containerized deployment)

### Development Credentials

The application seeds test data in development mode:

**Tenant:** `demo-tenant-001`

**Users:**
- Admin: `admin@example.com` / `Admin123!`
- Manager: `manager@example.com` / `Manager123!`
- Viewer: `viewer@example.com` / `Viewer123!`

**Sample Application:** "Payment Service" with 3 environments, 3 versions, and deployment history

### Local Development - Option 1: Docker Compose (Recommended)

```bash
# Clone the repository
git clone https://github.com/jrmce/VersionLifecycle.git
cd VersionLifecycle

# Start all services (PostgreSQL, API, Nginx)
docker-compose up --build

# Access the application
# Frontend: http://localhost:5000
# API: http://localhost:5000/api
# Swagger: http://localhost:5000/swagger
```

### Local Development - Option 2: Manual Setup

**Backend:**
```bash
cd VersionLifecycle.Web

# Update appsettings.json with your database connection
dotnet restore
dotnet build
dotnet run

# API will be available at https://localhost:5001
```

**Frontend:**
```bash
cd VersionLifecycle.Web/ClientApp

# Install dependencies
npm install

# Run development server
npm start

# Frontend will be available at http://localhost:4200
# It will proxy API requests to https://localhost:5001
```

### Testing the Application

1. **Register a new user**
   - Navigate to http://localhost:5000 (or frontend URL)
   - Click "Register" to create a new account
   - Use any tenant name, email, and strong password

2. **Create an application**
   - Log in with your credentials
   - Click "Applications" → "New Application"
   - Fill in the details and submit

3. **Create a version**
   - Go to your application
   - Add a version with semantic versioning (e.g., 1.0.0)

4. **Create a deployment**
   - Click "Deployments" → "New Deployment"
   - Select application and version
   - Confirm the deployment

5. **View deployment timeline**
   - Click on a deployment to see its event timeline

## Documentation

- **[ARCHITECTURE.md](ARCHITECTURE.md)** - Architecture overview and design decisions
- **[DEVELOPMENT.md](DEVELOPMENT.md)** - Development setup and troubleshooting
- **[Archived Implementation Docs](docs/archive/)** - Phase completion records (historical reference)

## API Documentation

The backend API includes Swagger/OpenAPI documentation available at:
- **Development**: https://localhost:5001/swagger
- **Production**: http://localhost:5000/swagger

### Main Endpoints

**Authentication:**
- `POST /api/auth/login` - User login
- `POST /api/auth/register` - User registration
- `POST /api/auth/refresh` - Refresh JWT token

**Applications:**
- `GET /api/applications` - List applications (paginated)
- `GET /api/applications/{id}` - Get application details
- `POST /api/applications` - Create application
- `PUT /api/applications/{id}` - Update application
- `DELETE /api/applications/{id}` - Delete application

**Deployments:**
- `GET /api/deployments` - List deployments (paginated, filterable by status)
- `GET /api/deployments/{id}` - Get deployment details
- `POST /api/deployments` - Create pending deployment
- `POST /api/deployments/{id}/confirm` - Confirm deployment
- `GET /api/deployments/{id}/events` - Get deployment event timeline

**Versions, Environments, Webhooks:**
- Similar RESTful endpoints for each resource

See [NEXT_STEPS.md](NEXT_STEPS.md) for complete endpoint documentation.

## Key Project Statistics

- **Backend**: 25+ classes, 3,000+ lines of code
- **Frontend**: 15+ components, 3,500+ lines of code
- **Database**: 8 entities with multi-tenant support
- **API**: 30+ RESTful endpoints
- **Tests**: Ready for implementation in Phase 4
- **Build Output**: 265.66 kB initial bundle (73.09 kB compressed)

## What's Next

### Phase 4: Testing
- Unit tests for backend services and controllers
- Component tests for Angular services and components
- Integration tests for API endpoints
- E2E tests for user workflows

### Phase 5: Production Deployment
- Kubernetes configuration
- CI/CD pipeline setup (GitHub Actions)
- Environment-specific configurations
- Performance monitoring and alerting
- Automated backups and disaster recovery

## Support & Contribution

For issues, questions, or contributions, please open an issue on GitHub.

## License

This project is provided as-is for development and learning purposes.

## Project Timeline

**Current Status (Dec 22, 2025):**
- Phase 1-2 (Backend): Complete ✅
- Phase 3 (Frontend): Complete ✅
- Phase 4 (Testing): Ready to start 📋
- Phase 5 (Deployment): Ready to start 🚀

**Estimated Time to Production:**
- Phase 4 Testing: 8-10 hours
- Phase 5 Deployment: 4-6 hours
- **Total**: 12-16 hours to production-ready state

## Quick Start

### Prerequisites

- .NET 8 SDK
````

1. **Clone and navigate to project**
   ```bash
   cd VersionLifecycle
   ```

2. **Set up database**
   ```bash
   # Update connection string in appsettings.json
   dotnet ef database update --project VersionLifecycle.Infrastructure --startup-project VersionLifecycle.Web
   ```

3. **Run the .NET API**
   ```bash
   dotnet run --project VersionLifecycle.Web
   # API available at http://localhost:5000
   # Swagger docs at http://localhost:5000/swagger
   ```

4. **Run the Angular frontend** (in another terminal)
   ```bash
   cd VersionLifecycle.Web
   npm install
   ng serve
   # Frontend available at http://localhost:4200
   ```

### Docker Deployment

1. **Create .env file**
   ```bash
   cp .env.example .env
   # Edit .env with your values
   ```

2. **Start services**
   ```bash
   docker-compose up --build
   ```

3. **Access the application**
   - Frontend: http://localhost
   - API: http://localhost/api
   - Swagger: http://localhost/api/swagger
   - Hangfire Dashboard: http://localhost/hangfire (admin only)

## API Documentation

### Authentication

All API endpoints (except `/api/auth/*`) require JWT authentication.

**Login**
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "password",
  "tenantId": "tenant-id"
}
```

**Response**
```json
{
  "accessToken": "eyJhbGc...",
  "refreshToken": "eyJhbGc...",
  "expiresIn": 3600,
  "userId": "user-id",
  "email": "user@example.com",
  "tenantId": "tenant-id"
}
```

### Core Endpoints

**Applications**
- `GET /api/applications` - List all applications
- `POST /api/applications` - Create application
- `GET /api/applications/{id}` - Get application detail
- `PUT /api/applications/{id}` - Update application
- `DELETE /api/applications/{id}` - Delete application

**Versions**
- `GET /api/applications/{appId}/versions` - List versions
- `POST /api/applications/{appId}/versions` - Create version
- `GET /api/applications/{appId}/versions/{verId}` - Get version detail
- `PATCH /api/applications/{appId}/versions/{verId}` - Update version

**Deployments**
- `GET /api/deployments` - List deployments (with filters)
- `POST /api/deployments` - Create pending deployment
- `GET /api/deployments/{id}` - Get deployment detail
- `PATCH /api/deployments/{id}/confirm` - Confirm pending deployment
- `GET /api/deployments/{id}/history` - Get deployment events

**Environments**
- `GET /api/applications/{appId}/environments` - List environments
- `POST /api/applications/{appId}/environments` - Create environment
- `PUT /api/applications/{appId}/environments/{id}` - Update environment
- `DELETE /api/applications/{appId}/environments/{id}` - Delete environment

**Webhooks** (Admin only)
- `GET /api/webhooks` - List webhooks
- `POST /api/webhooks` - Register webhook
- `GET /api/webhooks/{id}/deliveries` - Get delivery history
- `DELETE /api/webhooks/{id}` - Remove webhook

## Testing

### Run .NET Unit Tests
```bash
dotnet test VersionLifecycle.Tests
```

### Run Angular Tests
```bash
cd VersionLifecycle.Web
npm run test
```

### Run with Coverage
```bash
# .NET
dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover

# Angular
npm run test -- --coverage
```

## Development Guide

See [DEVELOPMENT.md](./DEVELOPMENT.md) for:
- Detailed local setup instructions
- Running migrations
- Database seeding
- Debug configurations
- Common issues and solutions

## Architecture Guide

See [ARCHITECTURE.md](./ARCHITECTURE.md) for:
- Clean Architecture layers
- Multi-tenancy implementation
- State management (SignalStore)
- Repository pattern
- Key design decisions

## License

This project is provided as-is for development and learning purposes.

## Project Timeline

**Status (Dec 23, 2025):** Fully operational with all core features implemented
4. Create a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For issues, questions, or suggestions, please create an issue in the repository.
# VersionLifecycle
