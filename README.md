# Version Lifecycle Management Application

A comprehensive application for tracking software development project versions and their deployment lifecycle across multiple environments.

## Features

- **Multi-Tenant Architecture**: Isolated data and configurations per tenant
- **Application Management**: Create and manage software applications
- **Version Tracking**: Track versions through different statuses (Draft, Released, Deprecated, Archived)
- **Environment Management**: Create custom deployment environments (Dev, Test, Staging, Prod, etc.)
- **Deployment Timeline**: Interactive visualization of deployments across environments
- **Pending Deployment Workflow**: Drag-to-drop deployments with confirmation workflow
- **Webhook Integration**: Automatic notifications to external systems on deployment events
- **Role-Based Access Control**: Admin, Manager, and Viewer roles with fine-grained permissions
- **REST API**: Full-featured API for programmatic access
- **Real-time Dashboards**: Monitor version lifecycle status

## Technology Stack

### Backend
- **.NET 8** with Clean Architecture
- **ASP.NET Core** Web API
- **Entity Framework Core** with PostgreSQL
- **Hangfire** for background job processing
- **Serilog** for structured logging
- **JWT** Authentication with Identity

### Frontend
- **Angular 17+** with lazy loading
- **D3.js** for timeline visualization
- **Angular Material** for UI components
- **Jest** for unit testing
- **TypeScript** for type safety

### Infrastructure
- **PostgreSQL** database
- **Docker & Docker Compose** for containerization
- **Nginx** reverse proxy
- **GitHub Actions** for CI/CD

## Project Structure

```
VersionLifecycle/
├── VersionLifecycle.Core/              # Domain entities and interfaces
│   ├── Entities/                       # Tenant, Application, Version, Deployment, etc.
│   ├── Enums/                          # VersionStatus, DeploymentStatus
│   ├── Exceptions/                     # Custom exceptions
│   └── Interfaces/                     # Repository and service contracts
├── VersionLifecycle.Application/       # Business logic and DTOs
│   ├── DTOs/                           # Data transfer objects
│   ├── Services/                       # Application services
│   └── Validators/                     # FluentValidation rules
├── VersionLifecycle.Infrastructure/    # Data access and external services
│   ├── Data/                           # DbContext and configurations
│   ├── Repositories/                   # Data access implementations
│   ├── Services/                       # JWT, Webhooks, Hangfire
│   └── Multitenancy/                   # Tenant context management
├── VersionLifecycle.Web/               # ASP.NET Core API
│   ├── Controllers/                    # API endpoints
│   ├── Middleware/                     # Custom middleware
│   ├── Authorization/                  # Authorization handlers
│   └── Models/                         # Response models
├── VersionLifecycle.Tests/             # Unit tests
├── VersionLifecycle.Web/ (Angular)    # Angular frontend
│   ├── src/app/core/                   # Core services and guards
│   ├── src/app/shared/                 # Shared components
│   ├── src/app/features/               # Feature modules
│   └── src/environments/               # Environment configuration
├── Dockerfile                          # .NET API container
├── docker-compose.yml                  # Multi-container orchestration
├── nginx.conf                          # Reverse proxy configuration
└── README.md                           # This file
```

## Quick Start

### Prerequisites

- .NET 8 SDK
- Node.js 18+ with npm
- PostgreSQL 14+
- Docker & Docker Compose (for containerized deployment)

### Local Development

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
- Clean Architecture explanation
- Multi-tenancy implementation
- Database schema overview
- API authentication flow
- Webhook system architecture
- Testing strategy

## Deployment Guide

See [DEPLOYMENT.md](./DEPLOYMENT.md) for:
- Production deployment steps
- Environment configuration
- Scaling considerations
- Monitoring setup
- Backup procedures
- Performance tuning

## Contributing

1. Create a feature branch
2. Commit your changes
3. Push to the branch
4. Create a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For issues, questions, or suggestions, please create an issue in the repository.
