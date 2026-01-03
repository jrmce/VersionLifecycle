namespace VersionLifecycle.Web.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VersionLifecycle.Application.DTOs;
using VersionLifecycle.Application.Services;
using VersionLifecycle.Infrastructure.Repositories;
using VersionLifecycle.Web.Models;
using VersionLifecycle.Core.Entities;

/// <summary>
/// Authentication controller for user login, registration, and token refresh.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController(UserManager<IdentityUser> userManager, ITokenService tokenService, TenantRepository tenantRepository, ITenantService tenantService) : ControllerBase
{

    /// <summary>
    /// Authenticates a user and returns JWT token.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ErrorResponse { Code = "INVALID_REQUEST", Message = "Invalid request", TraceId = HttpContext.TraceIdentifier });

        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null || !await userManager.CheckPasswordAsync(user, request.Password))
            return Unauthorized(new ErrorResponse { Code = "INVALID_CREDENTIALS", Message = "Invalid email or password", TraceId = HttpContext.TraceIdentifier });

        var roles = await userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "User";
        
        // SuperAdmin doesn't need a tenant
        string tenantId = request.TenantId;
        string? tenantName = null;
        string? tenantCode = null;
        
        if (role == "SuperAdmin")
        {
            tenantId = string.Empty; // SuperAdmin has no tenant restriction
        }
        else if (string.IsNullOrEmpty(request.TenantId))
        {
            return BadRequest(new ErrorResponse { Code = "TENANT_REQUIRED", Message = "Tenant ID is required for non-SuperAdmin users", TraceId = HttpContext.TraceIdentifier });
        }
        else
        {
            // Verify user belongs to the requested tenant
            var userClaims = await userManager.GetClaimsAsync(user);
            var userTenantClaim = userClaims.FirstOrDefault(c => c.Type == "tenantId");
            
            if (userTenantClaim == null || userTenantClaim.Value != request.TenantId)
            {
                return Unauthorized(new ErrorResponse { Code = "UNAUTHORIZED_TENANT", Message = "You do not have access to this tenant", TraceId = HttpContext.TraceIdentifier });
            }
            
            // Fetch tenant details for display
            var tenant = await tenantRepository.GetByIdAsync(request.TenantId);
            if (tenant != null)
            {
                tenantName = tenant.Name;
                tenantCode = tenant.Code;
            }
        }
        
        var token = tokenService.GenerateAccessToken(user.Id, tenantId, user.Email!, role);

        return Ok(new LoginResponseDto
        {
            AccessToken = token,
            RefreshToken = tokenService.GenerateRefreshToken(),
            ExpiresIn = 3600,
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            TenantId = tenantId,
            TenantName = tenantName,
            TenantCode = tenantCode,
            TokenType = "Bearer",
            Role = role
        });
    }

    /// <summary>
    /// Registers a new user.
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ErrorResponse { Code = "INVALID_REQUEST", Message = "Invalid request", TraceId = HttpContext.TraceIdentifier });

        var tenant = await tenantRepository.GetByIdAsync(request.TenantId);
        if (tenant == null || !tenant.IsActive)
        {
            return BadRequest(new ErrorResponse { Code = "TENANT_NOT_FOUND", Message = "Tenant does not exist or is inactive", TraceId = HttpContext.TraceIdentifier });
        }

        if (!string.Equals(tenant.Code, request.TenantCode, StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new ErrorResponse { Code = "INVALID_TENANT_CODE", Message = "Invalid tenant code", TraceId = HttpContext.TraceIdentifier });
        }

        var user = new IdentityUser { UserName = request.Email, Email = request.Email };
        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return BadRequest(new ErrorResponse { Code = "REGISTRATION_FAILED", Message = errors, TraceId = HttpContext.TraceIdentifier });
        }

        // Assign default Viewer role
        await userManager.AddToRoleAsync(user, "Viewer");
        
        // Store tenant association as a claim
        await userManager.AddClaimAsync(user, new System.Security.Claims.Claim("tenantId", request.TenantId));

        const string role = "Viewer";
        
        // Fetch tenant details for display
        string? tenantName = null;
        string? tenantCode = null;
        if (tenant != null)
        {
            tenantName = tenant.Name;
            tenantCode = tenant.Code;
        }
        
        var token = tokenService.GenerateAccessToken(user.Id, request.TenantId, user.Email, role);

        return CreatedAtAction(nameof(Login), new LoginResponseDto
        {
            AccessToken = token,
            RefreshToken = tokenService.GenerateRefreshToken(),
            ExpiresIn = 3600,
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            TenantId = request.TenantId,
            TenantName = tenantName,
            TenantCode = tenantCode,
            TokenType = "Bearer",
            Role = role
        });
    }

    /// <summary>
    /// Registers a new user and creates a new tenant for them.
    /// </summary>
    [HttpPost("register-with-tenant")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterWithTenant([FromBody] RegisterWithNewTenantDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ErrorResponse { Code = "INVALID_REQUEST", Message = "Invalid request", TraceId = HttpContext.TraceIdentifier });

        // Create the tenant first
        var createTenantDto = new CreateTenantDto
        {
            Name = request.TenantName,
            Description = request.TenantDescription,
            SubscriptionPlan = "Free"
        };

        var tenantDto = await tenantService.CreateTenantAsync(createTenantDto);

        // Create the user
        var user = new IdentityUser { UserName = request.Email, Email = request.Email };
        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return BadRequest(new ErrorResponse { Code = "REGISTRATION_FAILED", Message = errors, TraceId = HttpContext.TraceIdentifier });
        }

        // Assign Admin role to the tenant creator
        await userManager.AddToRoleAsync(user, "Admin");
        
        // Store tenant association as a claim
        await userManager.AddClaimAsync(user, new System.Security.Claims.Claim("tenantId", tenantDto.Id));

        const string role = "Admin";
        var token = tokenService.GenerateAccessToken(user.Id, tenantDto.Id, user.Email!, role);

        return CreatedAtAction(nameof(Login), new LoginResponseDto
        {
            AccessToken = token,
            RefreshToken = tokenService.GenerateRefreshToken(),
            ExpiresIn = 3600,
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            TenantId = tenantDto.Id,
            TokenType = "Bearer",
            Role = role,
            TenantCode = tenantDto.Code,
            TenantName = tenantDto.Name
        });
    }

    /// <summary>
    /// Refreshes the access token.
    /// </summary>
    [HttpPost("refresh")]
    [Authorize]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public IActionResult RefreshToken()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        var tenantId = User.FindFirst("tenantId")?.Value;

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email))
            return Unauthorized(new ErrorResponse { Code = "INVALID_TOKEN", Message = "Invalid token", TraceId = HttpContext.TraceIdentifier });

        var token = tokenService.GenerateAccessToken(userId, tenantId!, email, role ?? "User");

        return Ok(new LoginResponseDto
        {
            AccessToken = token,
            RefreshToken = tokenService.GenerateRefreshToken(),
            ExpiresIn = 3600,
            UserId = userId,
            Email = email,
            TenantId = tenantId ?? string.Empty,
            TokenType = "Bearer",
            Role = role ?? "User"
        });
    }
}
