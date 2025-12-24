# Development Guide

This guide provides detailed instructions for setting up and running the Version Lifecycle Management application locally.

## Prerequisites

- **Operating System**: Windows 10+, macOS, or Linux
- **.NET SDK**: 10.0 or later ([Download](https://dotnet.microsoft.com/download/dotnet/10.0))
- **Node.js**: 18.x or later with npm ([Download](https://nodejs.org/))
- **Database**: 
  - **Development**: SQLite (automatic, no setup required)
  - **Production**: PostgreSQL 14.x or later ([Download](https://www.postgresql.org/download/))
- **Git**: Latest version
- **IDE**: Visual Studio 2022, VS Code, or JetBrains Rider

## Setup Steps

### 1. Clone the Repository

```bash
git clone <repository-url>
cd VersionLifecycle
```

### 2. Database Setup

#### Option A: SQLite (Default for Development)

No setup required! The application automatically uses SQLite in development mode.

Database file location: `VersionLifecycle.Web/versionlifecycle.db`

The connection string in `appsettings.json` is already configured:
```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=versionlifecycle.db"
}
```

#### Option B: PostgreSQL (Production or Advanced Development)

If you prefer PostgreSQL for local development:

#### Option B: PostgreSQL (Production or Advanced Development)

If you prefer PostgreSQL for local development:

```bash
# Create database
createdb versionlifecycle

# (Optional) Create user if needed
createuser -P versionlifecycle_user
```

Update the connection string in `VersionLifecycle.Web/appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=versionlifecycle;Username=postgres;Password=your_password"
}
```

#### Option C: Docker PostgreSQL

```bash
docker run --name versionlifecycle-postgres -e POSTGRES_PASSWORD=postgres -p 5432:5432 -d postgres:15-alpine
```

### 3. Run Database Migrations

```bash
# Restore tools
dotnet tool restore

# Create initial migration
dotnet ef migrations add InitialCreate --project VersionLifecycle.Infrastructure --startup-project VersionLifecycle.Web

# Apply migrations
dotnet ef database update --project VersionLifecycle.Infrastructure --startup-project VersionLifecycle.Web
```

### 4. Configure JWT Secret

Update JWT secret in `VersionLifecycle.Web/appsettings.json`:

```json
"Jwt": {
  "Key": "your-super-secret-key-at-least-32-characters-long",
  "Issuer": "VersionLifecycleApp",
  "Audience": "VersionLifecycleUsers",
  "ExpirationMinutes": 60
}
```

> **Important**: For production, use a strong random secret and store it securely (Azure Key Vault, AWS Secrets Manager, etc.)

### 5. Run the .NET API

```bash
dotnet run --project VersionLifecycle.Web
```

The API will start on `http://localhost:5000` (or `https://localhost:5001`)

On first run, the application will:
- Apply database migrations automatically
- Seed test data (in Development mode):
  - Demo tenant: `demo-tenant-001`
  - 3 users: admin@example.com, manager@example.com, viewer@example.com (all password: `Admin123!`, `Manager123!`, `Viewer123!`)
  - Sample "Payment Service" application with versions and deployments

**Available endpoints:**
- Health check: http://localhost:5000/api/health
- Swagger UI: http://localhost:5000/swagger

### 6. Set Up Angular Frontend

In a new terminal:

```bash
cd VersionLifecycle.Web
npm install
ng serve
```

The frontend will start on `http://localhost:4200`

## Development Workflow

### Creating a New Feature

1. Create a feature branch: `git checkout -b feature/your-feature`
2. Make changes to both backend and frontend as needed
3. Run tests: `dotnet test` and `npm run test`
4. Commit: `git commit -m "feat: description"`
5. Push: `git push origin feature/your-feature`
6. Create Pull Request

### Database Migrations

To add a new migration after modifying entities:

```bash
dotnet ef migrations add YourMigrationName --project VersionLifecycle.Infrastructure --startup-project VersionLifecycle.Web
dotnet ef database update --startup-project VersionLifecycle.Web
```

### Running Tests

```bash
# All .NET tests
dotnet test

# Specific test class
dotnet test --filter ClassName=YourTestClass

# With coverage
dotnet test /p:CollectCoverage=true

# Angular tests
cd VersionLifecycle.Web
npm run test

# With coverage
npm run test -- --coverage
```

### Debugging

#### Visual Studio / Rider
- Open `VersionLifecycle.sln`
- Set breakpoints
- Press F5 to start debugging

#### VS Code

Create `.vscode/launch.json`:
```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": ".NET Core Launch (web)",
      "type": "csharp",
      "request": "launch",
      "program": "${workspaceFolder}/VersionLifecycle.Web/bin/Debug/net8.0/VersionLifecycle.Web.dll",
      "args": [],
      "cwd": "${workspaceFolder}/VersionLifecycle.Web",
      "stopAtEntry": false,
      "serverReadyAction": {
        "pattern": "\\bNow listening on:\\s+(https?://\\S+)",
        "uriFormat": "{1}",
        "action": "openExternally"
      }
    }
  ]
}
```

Press F5 to start debugging.

## Docker Development

### Build and Run Locally

```bash
# Copy environment file
cp .env.example .env

# Update values in .env as needed
nano .env

# Build and start containers
docker-compose up --build

# In another terminal, run migrations
docker exec versionlifecycle-api dotnet ef database update
```

Access:
- Frontend: http://localhost
- API: http://localhost/api
- Swagger: http://localhost/api/swagger

### Stop Services

```bash
docker-compose down
```

### View Logs

```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f api
docker-compose logs -f web
docker-compose logs -f postgres
```

## Environment Variables

Create a `.env` file for Docker:

```bash
# Database
POSTGRES_PASSWORD=your_secure_password

# JWT
JWT_SECRET=your_jwt_secret_key_change_this

# Application
ASPNETCORE_ENVIRONMENT=Development
API_BASE_URL=http://localhost:5000
```

## Common Issues and Solutions

### Issue: "Unable to connect to database"

**Solution:**
```bash
# Check PostgreSQL is running
psql -U postgres -h localhost

# Verify connection string in appsettings.json
# Ensure database exists: createdb versionlifecycle
```

### Issue: "Migration failed: Column does not exist"

**Solution:**
```bash
# Revert last migration (if not in production)
dotnet ef migrations remove

# Create new migration with correct changes
dotnet ef migrations add CorrectMigrationName
dotnet ef database update
```

### Issue: "Port 5000 already in use"

**Solution:**
```bash
# Change port in launchSettings.json or use
dotnet run --urls="http://localhost:5001"
```

### Issue: "Angular build fails"

**Solution:**
```bash
# Clear node_modules and reinstall
rm -rf node_modules package-lock.json
npm install

# Clear Angular cache
ng cache clean

# Rebuild
ng build
```

### Issue: "JWT token expired"

**Solution:**
- Token expires after 60 minutes (configured in appsettings.json)
- Use refresh token to get new access token
- Adjust `ExpirationMinutes` if needed

## Performance Optimization

### Database Optimization

1. **Add indexes** for frequently queried columns
2. **Enable connection pooling** in appsettings.json
3. **Use pagination** in list endpoints (default: 25 items)
4. **Monitor slow queries** using PostgreSQL logs

### API Optimization

1. **Enable response compression** (already in Program.cs)
2. **Use caching** headers for static content
3. **Implement rate limiting** (configured in Program.cs)
4. **Monitor with Hangfire dashboard** at `/hangfire`

### Frontend Optimization

1. **Lazy load feature modules** (already configured)
2. **Use OnPush change detection** (configured in components)
3. **Enable production build**: `ng build --configuration production`
4. **Analyze bundle size**: `npm run build -- --stats-json && webpack-bundle-analyzer dist/...`

## Security Checklist

Before deploying to production:

- [ ] Change JWT secret to a strong random value
- [ ] Change database password
- [ ] Enable HTTPS/TLS
- [ ] Configure CORS properly (not `*`)
- [ ] Set appropriate CORS headers at Nginx level
- [ ] Enable rate limiting in production
- [ ] Implement proper logging and monitoring
- [ ] Regular security updates for dependencies
- [ ] Use environment variables for secrets (never commit to git)
- [ ] Implement API request validation and sanitization
- [ ] Enable audit logging for sensitive operations

## Useful Commands

```bash
# .NET CLI
dotnet build                                          # Build solution
dotnet run --project VersionLifecycle.Web           # Run API
dotnet test                                          # Run all tests
dotnet clean                                         # Clean build files
dotnet tool install -g dotnet-ef                    # Install EF tool
dotnet watch run --project VersionLifecycle.Web    # Hot reload

# Angular CLI
ng serve                                             # Start dev server
ng build                                             # Build for production
ng test                                              # Run tests
ng lint                                              # Check code style
ng generate component path/to/component            # Generate component

# Docker
docker-compose up --build                           # Build and start
docker-compose down                                 # Stop and remove
docker-compose logs -f                              # View logs
docker-compose exec api dotnet ef ...              # Run migration in container
```

## Getting Help

1. Check existing issues in the repository
2. Review code comments and docstrings
3. Check the [Architecture Guide](./ARCHITECTURE.md)
4. Review test files for usage examples
5. Create a new issue with detailed description

## Next Steps

- Familiarize yourself with [Clean Architecture](./ARCHITECTURE.md)
- Review the API [endpoints documentation](./README.md#api-documentation)
- Understand the [multi-tenancy implementation](./ARCHITECTURE.md#multi-tenancy)
- Explore the [database schema](./ARCHITECTURE.md#database-schema)
- Run the application and test features locally
