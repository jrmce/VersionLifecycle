using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using AutoMapper;
using Serilog;
using System.Text;
using VersionLifecycle.Application.Services;
using VersionLifecycle.Core.Interfaces;
using VersionLifecycle.Infrastructure.Data;
using VersionLifecycle.Infrastructure.Multitenancy;
using VersionLifecycle.Infrastructure.Repositories;
using VersionLifecycle.Infrastructure.Services;
using VersionLifecycle.Web.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/app-.txt", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Add DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var environment = builder.Environment.EnvironmentName;

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (environment == "Development" || connectionString.StartsWith("Data Source="))
    {
        // Use SQLite in Development or when Data Source connection string is specified
        options.UseSqlite(connectionString ?? "Data Source=versionlifecycle.db");
    }
    else
    {
        // Use PostgreSQL for production
        options.UseNpgsql(connectionString);
    }
});

// Add Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Add Tenant Context
builder.Services.AddScoped<ITenantContext, TenantContext>();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(VersionLifecycle.Application.Mapping.MappingProfile));

// Add Repositories
builder.Services.AddScoped(typeof(GenericRepository<>));
builder.Services.AddScoped<ApplicationRepository>();
builder.Services.AddScoped<VersionRepository>();
builder.Services.AddScoped<DeploymentRepository>();
builder.Services.AddScoped<EnvironmentRepository>();
builder.Services.AddScoped<WebhookRepository>();
builder.Services.AddScoped<TenantRepository>();

// Add Services - Using ApplicationServices class which implements all interfaces
builder.Services.AddScoped<ITokenService>(sp =>
    new TokenService(builder.Configuration));

// Register ApplicationService which implements IApplicationService
builder.Services.AddScoped<ApplicationService>();
builder.Services.AddScoped<IApplicationService>(sp => sp.GetRequiredService<ApplicationService>());

// Register VersionService which implements IVersionService
builder.Services.AddScoped<VersionService>();
builder.Services.AddScoped<IVersionService>(sp => sp.GetRequiredService<VersionService>());

// Register DeploymentService which implements IDeploymentService
builder.Services.AddScoped<DeploymentService>();
builder.Services.AddScoped<IDeploymentService>(sp => sp.GetRequiredService<DeploymentService>());

// Register EnvironmentService which implements IEnvironmentService
builder.Services.AddScoped<EnvironmentService>();
builder.Services.AddScoped<IEnvironmentService>(sp => sp.GetRequiredService<EnvironmentService>());

// Register WebhookService which implements IWebhookService
builder.Services.AddScoped<WebhookService>();
builder.Services.AddScoped<IWebhookService>(sp => sp.GetRequiredService<WebhookService>());

// Register TenantService which implements ITenantService
builder.Services.AddScoped<TenantService>();
builder.Services.AddScoped<ITenantService>(sp => sp.GetRequiredService<TenantService>());

// Add JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "your-super-secret-key-minimum-32-characters-long!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "VersionLifecycle";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "VersionLifecycleClient";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero
    };
});

// Add Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ManagerOrAdmin", policy => policy.RequireRole("Manager", "Admin"));
});

// Add Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Version Lifecycle API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] { }
        }
    });
});

// Add CORS for frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

var app = builder.Build();

// Middleware
app.UseTenantResolution();
app.UseCors("AllowFrontend");
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Version Lifecycle API v1"));
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Health check endpoint
app.MapGet("/api/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

// Auto-migrate on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    // TODO: Seed data in development environment - disabled due to FK constraint issues with SQLite
    // if (app.Environment.IsDevelopment())
    // {
    //     var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    //     var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    //     var seeder = new DataSeeder(db, userManager, roleManager);
    //     await seeder.SeedAsync();
    // }
}

await app.RunAsync();
