#!/bin/bash

# Version Lifecycle Management Application - Complete Setup Script
# This script creates the entire .NET 8 + Angular project structure

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}Version Lifecycle Management App Setup${NC}"
echo -e "${GREEN}========================================${NC}\n"

# Step 1: Create .NET Solution
echo -e "${YELLOW}Step 1: Creating .NET Solution...${NC}"
mkdir -p VersionLifecycle && cd VersionLifecycle
dotnet new sln -n VersionLifecycle

# Step 2: Create Projects
echo -e "${YELLOW}Step 2: Creating .NET Projects...${NC}"
dotnet new classlib -n VersionLifecycle.Core -f net8.0
dotnet new classlib -n VersionLifecycle.Application -f net8.0
dotnet new classlib -n VersionLifecycle.Infrastructure -f net8.0
dotnet new webapi -n VersionLifecycle.Web -f net8.0
dotnet new xunit -n VersionLifecycle.Tests -f net8.0

# Step 3: Add projects to solution
echo -e "${YELLOW}Step 3: Adding projects to solution...${NC}"
dotnet sln add VersionLifecycle.Core/VersionLifecycle.Core.csproj
dotnet sln add VersionLifecycle.Application/VersionLifecycle.Application.csproj
dotnet sln add VersionLifecycle.Infrastructure/VersionLifecycle.Infrastructure.csproj
dotnet sln add VersionLifecycle.Web/VersionLifecycle.Web.csproj
dotnet sln add VersionLifecycle.Tests/VersionLifecycle.Tests.csproj

# Step 4: Add project references
echo -e "${YELLOW}Step 4: Adding project references...${NC}"
dotnet add VersionLifecycle.Application/VersionLifecycle.Application.csproj reference VersionLifecycle.Core/VersionLifecycle.Core.csproj
dotnet add VersionLifecycle.Infrastructure/VersionLifecycle.Infrastructure.csproj reference VersionLifecycle.Core/VersionLifecycle.Core.csproj
dotnet add VersionLifecycle.Infrastructure/VersionLifecycle.Infrastructure.csproj reference VersionLifecycle.Application/VersionLifecycle.Application.csproj
dotnet add VersionLifecycle.Web/VersionLifecycle.Web.csproj reference VersionLifecycle.Core/VersionLifecycle.Core.csproj
dotnet add VersionLifecycle.Web/VersionLifecycle.Web.csproj reference VersionLifecycle.Application/VersionLifecycle.Application.csproj
dotnet add VersionLifecycle.Web/VersionLifecycle.Web.csproj reference VersionLifecycle.Infrastructure/VersionLifecycle.Infrastructure.csproj
dotnet add VersionLifecycle.Tests/VersionLifecycle.Tests.csproj reference VersionLifecycle.Core/VersionLifecycle.Core.csproj
dotnet add VersionLifecycle.Tests/VersionLifecycle.Tests.csproj reference VersionLifecycle.Application/VersionLifecycle.Application.csproj
dotnet add VersionLifecycle.Tests/VersionLifecycle.Tests.csproj reference VersionLifecycle.Infrastructure/VersionLifecycle.Infrastructure.csproj
dotnet add VersionLifecycle.Tests/VersionLifecycle.Tests.csproj reference VersionLifecycle.Web/VersionLifecycle.Web.csproj

# Step 5: Add NuGet packages
echo -e "${YELLOW}Step 5: Adding NuGet packages...${NC}"

# Application packages
dotnet add VersionLifecycle.Application/VersionLifecycle.Application.csproj package FluentValidation
dotnet add VersionLifecycle.Application/VersionLifecycle.Application.csproj package AutoMapper

# Infrastructure packages
dotnet add VersionLifecycle.Infrastructure/VersionLifecycle.Infrastructure.csproj package Microsoft.EntityFrameworkCore
dotnet add VersionLifecycle.Infrastructure/VersionLifecycle.Infrastructure.csproj package Microsoft.EntityFrameworkCore.PostgreSQL
dotnet add VersionLifecycle.Infrastructure/VersionLifecycle.Infrastructure.csproj package Hangfire.Core
dotnet add VersionLifecycle.Infrastructure/VersionLifecycle.Infrastructure.csproj package Hangfire.PostgreSql
dotnet add VersionLifecycle.Infrastructure/VersionLifecycle.Infrastructure.csproj package Serilog
dotnet add VersionLifecycle.Infrastructure/VersionLifecycle.Infrastructure.csproj package Serilog.Sinks.Console
dotnet add VersionLifecycle.Infrastructure/VersionLifecycle.Infrastructure.csproj package Serilog.Sinks.File

# Web packages
dotnet add VersionLifecycle.Web/VersionLifecycle.Web.csproj package Microsoft.EntityFrameworkCore
dotnet add VersionLifecycle.Web/VersionLifecycle.Web.csproj package Microsoft.EntityFrameworkCore.PostgreSQL
dotnet add VersionLifecycle.Web/VersionLifecycle.Web.csproj package Hangfire.AspNetCore
dotnet add VersionLifecycle.Web/VersionLifecycle.Web.csproj package Hangfire.PostgreSql
dotnet add VersionLifecycle.Web/VersionLifecycle.Web.csproj package Serilog.AspNetCore
dotnet add VersionLifecycle.Web/VersionLifecycle.Web.csproj package Swashbuckle.AspNetCore
dotnet add VersionLifecycle.Web/VersionLifecycle.Web.csproj package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add VersionLifecycle.Web/VersionLifecycle.Web.csproj package System.IdentityModel.Tokens.Jwt
dotnet add VersionLifecycle.Web/VersionLifecycle.Web.csproj package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add VersionLifecycle.Web/VersionLifecycle.Web.csproj package Microsoft.AspNetCore.RateLimiting

# Test packages
dotnet add VersionLifecycle.Tests/VersionLifecycle.Tests.csproj package Moq
dotnet add VersionLifecycle.Tests/VersionLifecycle.Tests.csproj package Microsoft.EntityFrameworkCore.InMemory

# Step 6: Build solution
echo -e "${YELLOW}Step 6: Building solution...${NC}"
dotnet build

# Step 7: Create Angular app
echo -e "${YELLOW}Step 7: Creating Angular application...${NC}"
npm install -g @angular/cli
ng new VersionLifecycle.Web --routing --skip-git --style=css

# Step 8: Install Angular dependencies
echo -e "${YELLOW}Step 8: Installing Angular dependencies...${NC}"
cd VersionLifecycle.Web
npm install d3 @types/d3
npm install @angular/cdk
npm install --save-dev @angular/eslint @eslint-scope
npm install --save-dev jest @types/jest jest-preset-angular
npm install --save-dev @angular/material

cd ..

echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}Setup Complete!${NC}"
echo -e "${GREEN}========================================${NC}\n"

echo -e "${YELLOW}Next Steps:${NC}"
echo "1. Configure your database connection in appsettings.json"
echo "2. Run: dotnet ef migrations add InitialCreate --project VersionLifecycle.Infrastructure --startup-project VersionLifecycle.Web"
echo "3. Run: dotnet ef database update --startup-project VersionLifecycle.Web"
echo "4. Run: dotnet run --project VersionLifecycle.Web"
echo "5. In another terminal, cd VersionLifecycle.Web && ng serve"
echo ""
echo -e "${YELLOW}Docker:${NC}"
echo "Run: docker-compose up --build"
