# Frontend build stage - Build Angular app
FROM node:22-alpine AS frontend-build
WORKDIR /app
COPY ["VersionLifecycle.Web/ClientApp/package*.json", "./"]
RUN npm ci
COPY ["VersionLifecycle.Web/ClientApp/", "./"]
RUN npm run build
# Verify the build output
RUN find /app/dist -type f -name "index.html" 2>/dev/null || echo "index.html not found in dist"

# Base image for .NET runtime (.NET 10 to match net10.0 target)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Builder stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder
WORKDIR /src

# Copy solution and project files
COPY ["VersionLifecycle.sln", "."]
COPY ["VersionLifecycle.Core/VersionLifecycle.Core.csproj", "VersionLifecycle.Core/"]
COPY ["VersionLifecycle.Application/VersionLifecycle.Application.csproj", "VersionLifecycle.Application/"]
COPY ["VersionLifecycle.Infrastructure/VersionLifecycle.Infrastructure.csproj", "VersionLifecycle.Infrastructure/"]
COPY ["VersionLifecycle.Web/VersionLifecycle.Web.csproj", "VersionLifecycle.Web/"]
COPY ["VersionLifecycle.Tests/VersionLifecycle.Tests.csproj", "VersionLifecycle.Tests/"]

# Restore dependencies
RUN dotnet restore "VersionLifecycle.sln"

# Copy source code
COPY . .

# Build
RUN dotnet build "VersionLifecycle.sln" -c Release -o /app/build

# Publish
FROM builder AS publish
RUN dotnet publish "VersionLifecycle.Web/VersionLifecycle.Web.csproj" -c Release -o /app/publish

# Runtime stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Copy frontend build output to wwwroot (where .NET serves static files)
# Angular 17+ outputs to dist/ClientApp/browser/
RUN mkdir -p /app/wwwroot
COPY --from=frontend-build /app/dist/ClientApp/browser/ ./wwwroot/

# Debug: verify wwwroot contents
RUN echo "=== wwwroot contents ===" && ls -la /app/wwwroot/ && echo "=== index.html check ===" && test -f /app/wwwroot/index.html && echo "✓ index.html found" || echo "✗ index.html NOT found"

# Install curl for healthcheck
RUN apt-get update && apt-get install -y --no-install-recommends curl ca-certificates \
    && rm -rf /var/lib/apt/lists/*

# Create non-root user for security (use high UID to avoid collisions)
RUN useradd -m -u 2000 appuser && chown -R appuser:appuser /app
USER appuser

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost/api/health || exit 1

ENTRYPOINT ["dotnet", "VersionLifecycle.Web.dll"]

