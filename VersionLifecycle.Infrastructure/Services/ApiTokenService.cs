namespace VersionLifecycle.Infrastructure.Services;

using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using Microsoft.Extensions.Logging;
using VersionLifecycle.Application.DTOs;
using VersionLifecycle.Application.Services;
using VersionLifecycle.Core.Entities;
using VersionLifecycle.Core.Exceptions;
using VersionLifecycle.Core.Interfaces;
using VersionLifecycle.Infrastructure.Repositories;

/// <summary>
/// Service for API token management and validation.
/// </summary>
public class ApiTokenService(
    ApiTokenRepository repository,
    ITenantContext tenantContext,
    IMapper mapper,
    ILogger<ApiTokenService> logger) : IApiTokenService
{
    private const string TokenPrefix = "vl_";
    private const int TokenLength = 64; // 64 bytes = 512 bits

    /// <summary>
    /// Gets all API tokens for the current tenant.
    /// </summary>
    public async Task<IEnumerable<ApiTokenDto>> GetApiTokensAsync()
    {
        var tokens = await repository.GetActiveTokensAsync();
        return mapper.Map<IEnumerable<ApiTokenDto>>(tokens);
    }

    /// <summary>
    /// Gets a specific API token by ID.
    /// </summary>
    public async Task<ApiTokenDto?> GetApiTokenAsync(int id)
    {
        var token = await repository.GetByIdAsync(id);
        return token == null ? null : mapper.Map<ApiTokenDto>(token);
    }

    /// <summary>
    /// Creates a new API token with secure random generation.
    /// </summary>
    public async Task<ApiTokenCreatedDto> CreateApiTokenAsync(CreateApiTokenDto dto)
    {
        // Generate cryptographically secure random token
        var tokenBytes = new byte[TokenLength];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(tokenBytes);
        }
        
        var tokenString = Convert.ToBase64String(tokenBytes)
            .Replace("+", "")
            .Replace("/", "")
            .Replace("=", "");
        
        var fullToken = $"{TokenPrefix}{tokenString}";
        var tokenHash = ComputeSha256Hash(fullToken);
        var displayPrefix = fullToken.Substring(0, Math.Min(12, fullToken.Length)); // Show first 12 chars

        var apiToken = new ApiToken
        {
            Name = dto.Name,
            Description = dto.Description,
            TokenHash = tokenHash,
            TokenPrefix = displayPrefix,
            ExpiresAt = dto.ExpiresAt,
            IsActive = true
        };

        await repository.AddAsync(apiToken);

        logger.LogInformation("API token created: {TokenName} (ID: {TokenId}) for tenant {TenantId}", 
            dto.Name, apiToken.Id, tenantContext.CurrentTenantId);

        return new ApiTokenCreatedDto
        {
            Id = apiToken.Id,
            Name = apiToken.Name,
            Description = apiToken.Description,
            Token = fullToken, // Return plaintext token only once!
            TokenPrefix = apiToken.TokenPrefix,
            ExpiresAt = apiToken.ExpiresAt,
            CreatedAt = apiToken.CreatedAt
        };
    }

    /// <summary>
    /// Updates an API token (name, description, active status).
    /// </summary>
    public async Task<ApiTokenDto> UpdateApiTokenAsync(int id, UpdateApiTokenDto dto)
    {
        var token = await repository.GetByIdAsync(id);
        if (token == null)
            throw new NotFoundException($"API token with ID {id} not found");

        if (!string.IsNullOrEmpty(dto.Name))
            token.Name = dto.Name;
        
        if (dto.Description != null)
            token.Description = dto.Description;
        
        if (dto.IsActive.HasValue)
            token.IsActive = dto.IsActive.Value;

        await repository.UpdateAsync(token);

        logger.LogInformation("API token updated: {TokenName} (ID: {TokenId})", token.Name, token.Id);

        return mapper.Map<ApiTokenDto>(token);
    }

    /// <summary>
    /// Revokes an API token (soft delete).
    /// </summary>
    public async Task RevokeApiTokenAsync(int id)
    {
        var token = await repository.GetByIdAsync(id);
        if (token == null)
            throw new NotFoundException($"API token with ID {id} not found");

        token.IsDeleted = true;
        await repository.UpdateAsync(token);

        logger.LogInformation("API token revoked: {TokenName} (ID: {TokenId})", token.Name, token.Id);
    }

    /// <summary>
    /// Validates an API token and returns tenant/user context.
    /// </summary>
    public async Task<(bool isValid, string? tenantId, string? userId)> ValidateApiTokenAsync(string token)
    {
        if (string.IsNullOrEmpty(token) || !token.StartsWith(TokenPrefix))
        {
            return (false, null, null);
        }

        var tokenHash = ComputeSha256Hash(token);
        var apiToken = await repository.GetByTokenHashAsync(tokenHash);

        if (apiToken == null)
        {
            logger.LogWarning("Invalid API token attempted: {TokenPrefix}...", token.Substring(0, Math.Min(12, token.Length)));
            return (false, null, null);
        }

        // Check expiration
        if (apiToken.ExpiresAt.HasValue && apiToken.ExpiresAt.Value < DateTime.UtcNow)
        {
            logger.LogWarning("Expired API token used: {TokenName} (ID: {TokenId})", apiToken.Name, apiToken.Id);
            return (false, null, null);
        }

        // Update last used timestamp asynchronously
        _ = Task.Run(() => repository.UpdateLastUsedAsync(apiToken.Id));

        logger.LogDebug("API token validated: {TokenName} (ID: {TokenId}) for tenant {TenantId}", 
            apiToken.Name, apiToken.Id, apiToken.TenantId);

        // Return tenant ID and use token ID as userId
        return (true, apiToken.TenantId, $"apitoken_{apiToken.Id}");
    }

    /// <summary>
    /// Computes SHA256 hash of a string.
    /// </summary>
    private static string ComputeSha256Hash(string input)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
