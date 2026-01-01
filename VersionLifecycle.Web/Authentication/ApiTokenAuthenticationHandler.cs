namespace VersionLifecycle.Web.Authentication;

using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using VersionLifecycle.Application.Services;
using VersionLifecycle.Core.Interfaces;

/// <summary>
/// Custom authentication handler for API token (Bearer token) authentication.
/// </summary>
public class ApiTokenAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IApiTokenService _apiTokenService;
    private readonly ITenantContext _tenantContext;

    public ApiTokenAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IApiTokenService apiTokenService,
        ITenantContext tenantContext)
        : base(options, logger, encoder)
    {
        _apiTokenService = apiTokenService;
        _tenantContext = tenantContext;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Read Authorization header (may be empty if not present)
        string? authHeader = Request.Headers["Authorization"];
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.NoResult();
        }

        var token = authHeader.Substring("Bearer ".Length).Trim();

        // Check if it's an API token (starts with "vl_")
        if (!token.StartsWith("vl_"))
        {
            // Not an API token, let JWT handler process it
            return AuthenticateResult.NoResult();
        }

        // Validate API token
        var (isValid, tenantId, userId) = await _apiTokenService.ValidateApiTokenAsync(token);

        if (!isValid || string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(userId))
        {
            return AuthenticateResult.Fail("Invalid or expired API token");
        }

        // Set tenant context for the request
        _tenantContext.SetTenant(tenantId, userId);

        // Create claims for the API token
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim("tenantId", tenantId),
            new Claim(ClaimTypes.Role, "Admin"), // API tokens have Admin privileges
            new Claim("authType", "ApiToken")
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}
