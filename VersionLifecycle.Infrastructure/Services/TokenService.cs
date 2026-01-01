namespace VersionLifecycle.Infrastructure.Services;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using VersionLifecycle.Application.Services;

/// <summary>
/// Service for JWT token generation and validation.
/// </summary>
public class TokenService(IConfiguration configuration) : ITokenService
{

    /// <summary>
    /// Generates a JWT access token.
    /// </summary>
    public string GenerateAccessToken(string userId, string tenantId, string email, string role)
    {
        var key = Encoding.ASCII.GetBytes(configuration["Jwt:Key"] ?? "your-super-secret-key-minimum-32-characters-long!");
        var issuer = configuration["Jwt:Issuer"] ?? "VersionLifecycle";
        var audience = configuration["Jwt:Audience"] ?? "VersionLifecycleClient";
        var expirationMinutes = int.Parse(configuration["Jwt:ExpirationMinutes"] ?? "60");
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Email, email),
            new("tenantId", tenantId),
            new(ClaimTypes.Role, role)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    /// <summary>
    /// Generates a refresh token.
    /// </summary>
    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
        }

        return Convert.ToBase64String(randomNumber);
    }

    /// <summary>
    /// Validates a JWT token.
    /// </summary>
    public bool ValidateToken(string token)
    {
        try
        {
            var key = Encoding.ASCII.GetBytes(configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured"));
            var tokenHandler = new JwtSecurityTokenHandler();

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return true;
        }
        catch
        {
            return false;
        }
    }
}
