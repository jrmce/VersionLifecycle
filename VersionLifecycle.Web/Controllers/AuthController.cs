namespace VersionLifecycle.Web.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VersionLifecycle.Application.DTOs;
using VersionLifecycle.Application.Services;
using VersionLifecycle.Web.Models;

/// <summary>
/// Authentication controller for user login, registration, and token refresh.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ITokenService _tokenService;

    public AuthController(UserManager<IdentityUser> userManager, ITokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

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

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            return Unauthorized(new ErrorResponse { Code = "INVALID_CREDENTIALS", Message = "Invalid email or password", TraceId = HttpContext.TraceIdentifier });

        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.GenerateAccessToken(user.Id, request.TenantId, user.Email!, roles.FirstOrDefault() ?? "User");

        return Ok(new LoginResponseDto
        {
            AccessToken = token,
            RefreshToken = _tokenService.GenerateRefreshToken(),
            ExpiresIn = 3600,
            TokenType = "Bearer"
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

        var user = new IdentityUser { UserName = request.Email, Email = request.Email };
        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return BadRequest(new ErrorResponse { Code = "REGISTRATION_FAILED", Message = errors, TraceId = HttpContext.TraceIdentifier });
        }

        // Assign default Viewer role
        await _userManager.AddToRoleAsync(user, "Viewer");

        var token = _tokenService.GenerateAccessToken(user.Id, request.TenantId, user.Email, "Viewer");

        return CreatedAtAction(nameof(Login), new LoginResponseDto
        {
            AccessToken = token,
            RefreshToken = _tokenService.GenerateRefreshToken(),
            ExpiresIn = 3600,
            TokenType = "Bearer"
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

        var token = _tokenService.GenerateAccessToken(userId, tenantId!, email, role ?? "User");

        return Ok(new LoginResponseDto
        {
            AccessToken = token,
            RefreshToken = _tokenService.GenerateRefreshToken(),
            ExpiresIn = 3600,
            TokenType = "Bearer"
        });
    }
}
