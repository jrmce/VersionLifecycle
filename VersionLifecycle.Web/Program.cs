using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
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
using VersionLifecycle.Web.Services;

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
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Serialize enums as strings instead of integers
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();

// Add DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    // Always use PostgreSQL for consistency between Development and Production
    options.UseNpgsql(connectionString ?? "Host=localhost;Database=versionlifecycle;Username=postgres;Password=postgres");
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
builder.Services.AddScoped<ApiTokenRepository>();

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

// Register WebhookDeliveryService
builder.Services.AddScoped<WebhookDeliveryService>();

// Register BackgroundTaskRunner for fire-and-forget tasks with proper scope management
builder.Services.AddScoped<IBackgroundTaskRunner, BackgroundTaskRunner>();

// Register WebhookEvent repository
builder.Services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));

// Add HttpClient for webhooks
builder.Services.AddHttpClient("WebhookClient", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Add Webhook retry background service
builder.Services.AddHostedService<WebhookRetryBackgroundService>();

// Register TenantService which implements ITenantService
builder.Services.AddScoped<TenantService>();
builder.Services.AddScoped<ITenantService>(sp => sp.GetRequiredService<TenantService>());

// Register ApiTokenService which implements IApiTokenService
builder.Services.AddScoped<ApiTokenService>();
builder.Services.AddScoped<IApiTokenService>(sp => sp.GetRequiredService<ApiTokenService>());

// Add JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "your-super-secret-key-minimum-32-characters-long!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "VersionLifecycle";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "VersionLifecycleClient";

builder.Services.AddAuthentication(options =>
{
    // Use policy scheme to handle both JWT and API tokens
    options.DefaultAuthenticateScheme = "MultiAuthScheme";
    options.DefaultChallengeScheme = "MultiAuthScheme";
})
.AddPolicyScheme("MultiAuthScheme", "Bearer or API Token", options =>
{
    options.ForwardDefaultSelector = context =>
    {
        // Check if the token starts with "vl_" for API tokens
        var authHeader = context.Request.Headers["Authorization"].ToString();
        if (authHeader.StartsWith("Bearer vl_", StringComparison.OrdinalIgnoreCase))
        {
            return "ApiToken";
        }
        return JwtBearerDefaults.AuthenticationScheme;
    };
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
})
.AddScheme<AuthenticationSchemeOptions, VersionLifecycle.Web.Authentication.ApiTokenAuthenticationHandler>(
    "ApiToken", 
    options => { });

// Configure authorization with all policies
builder.Services.AddAuthorizationBuilder()
    // Configure authorization with all policies
    .AddPolicy("SuperAdminOnly", policy => policy.RequireRole("SuperAdmin"))
    // Configure authorization with all policies
    .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin", "SuperAdmin"))
    // Configure authorization with all policies
    .AddPolicy("ManagerOrAdmin", policy => policy.RequireRole("Manager", "Admin", "SuperAdmin"));

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
            Array.Empty<string>()
        }
    });
});

// Add CORS for frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost", "http://localhost:80", "http://localhost:4200", "https://localhost:4200")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

var app = builder.Build();

// Middleware - IMPORTANT: Order matters!
// 1. CORS must come first for preflight requests
app.UseCors("AllowFrontend");

// 2. Swagger (before authentication)
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Version Lifecycle API v1"));

// 3. Static files (wwwroot for Angular frontend)
app.UseStaticFiles();

// 3a. Fallback to index.html for SPA routing
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        const int durationInSeconds = 60 * 60 * 24 * 365; // 1 year
        ctx.Context.Response.Headers.Append("Cache-Control", $"public, max-age={durationInSeconds}");
    }
});

app.MapFallbackToFile("index.html");

// 4. HTTPS Redirection
app.UseHttpsRedirection();

// 5. Authentication must come before tenant resolution (needs User.Claims)
app.UseAuthentication();

// 6. Tenant Resolution (needs authenticated user)
app.UseTenantResolution();

// 7. Authorization
app.UseAuthorization();

// 8. Controllers
app.MapControllers();

// Health check endpoint
app.MapGet("/api/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

// Auto-migrate on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    // In Development, always start fresh
    if (app.Environment.IsDevelopment())
    {
        // await db.Database.EnsureDeletedAsync();
        // await db.Database.MigrateAsync();
    }
    else
    {
        // In Production, just apply migrations
        await db.Database.MigrateAsync();
    }

    // Ensure essential Identity roles exist in all environments
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var requiredRoles = new[] { "SuperAdmin", "Admin", "Manager", "Viewer" };
    foreach (var role in requiredRoles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // Seed data in development environment
    if (app.Environment.IsDevelopment())
    {
        // Seed each section in its own scope to ensure proper transaction commits
        using var seedScope = app.Services.CreateScope();
        var seedDb = seedScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = seedScope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var seeder = new DataSeeder(seedDb, userManager, roleManager);
        await seeder.SeedAsync();
    }
}

await app.RunAsync();
