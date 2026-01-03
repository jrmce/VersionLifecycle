namespace VersionLifecycle.Application.Services;

using VersionLifecycle.Application.DTOs;

/// <summary>
/// Interface for token generation and management.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates a JWT access token.
    /// </summary>
    string GenerateAccessToken(string userId, string tenantId, string email, string role);

    /// <summary>
    /// Generates a refresh token.
    /// </summary>
    string GenerateRefreshToken();

    /// <summary>
    /// Validates a token.
    /// </summary>
    bool ValidateToken(string token);
}

/// <summary>
/// Interface for application service operations.
/// </summary>
public interface IApplicationService
{
    Task<PaginatedResponse<ApplicationDto>> GetApplicationsAsync(int skip = 0, int take = 25);
    Task<ApplicationDto?> GetApplicationAsync(Guid externalId);
    Task<ApplicationDto> CreateApplicationAsync(CreateApplicationDto dto);
    Task<ApplicationDto> UpdateApplicationAsync(Guid externalId, UpdateApplicationDto dto);
    Task DeleteApplicationAsync(Guid externalId);
}

/// <summary>
/// Interface for version service operations.
/// </summary>
public interface IVersionService
{
    Task<IEnumerable<VersionDto>> GetVersionsByApplicationAsync(Guid applicationExternalId);
    Task<VersionDto?> GetVersionAsync(Guid externalId);
    Task<VersionDto> CreateVersionAsync(CreateVersionDto dto);
    Task<VersionDto> UpdateVersionAsync(Guid externalId, UpdateVersionDto dto);
    Task DeleteVersionAsync(Guid externalId);
}

/// <summary>
/// Interface for deployment service operations.
/// </summary>
public interface IDeploymentService
{
    Task<PaginatedResponse<DeploymentDto>> GetDeploymentsAsync(int skip = 0, int take = 25, string? statusFilter = null);
    Task<DeploymentDto?> GetDeploymentAsync(Guid externalId);
    Task<DeploymentDto> CreatePendingDeploymentAsync(CreatePendingDeploymentDto dto);
    Task<DeploymentDto> ConfirmDeploymentAsync(Guid externalId, ConfirmDeploymentDto dto);
    Task<DeploymentDto> UpdateDeploymentStatusAsync(Guid externalId, UpdateDeploymentStatusDto dto);
    Task<DeploymentDto> PromoteDeploymentAsync(Guid externalId, PromoteDeploymentDto dto);
    Task<IEnumerable<DeploymentEventDto>> GetDeploymentHistoryAsync(Guid deploymentExternalId);
}

/// <summary>
/// Interface for environment service operations.
/// </summary>
public interface IEnvironmentService
{
    Task<IEnumerable<EnvironmentDto>> GetEnvironmentsAsync();
    Task<EnvironmentDto?> GetEnvironmentAsync(Guid externalId);
    Task<EnvironmentDto> CreateEnvironmentAsync(CreateEnvironmentDto dto);
    Task<EnvironmentDto> UpdateEnvironmentAsync(Guid externalId, UpdateEnvironmentDto dto);
    Task DeleteEnvironmentAsync(Guid externalId);

    /// <summary>
    /// Gets a dashboard view of environments with their latest deployments per application.
    /// </summary>
    Task<IEnumerable<EnvironmentDeploymentOverviewDto>> GetEnvironmentDeploymentOverviewAsync();
}

/// <summary>
/// Interface for webhook service operations.
/// </summary>
public interface IWebhookService
{
    Task<IEnumerable<WebhookDto>> GetWebhooksAsync(Guid applicationExternalId);
    Task<WebhookDto?> GetWebhookAsync(Guid externalId);
    Task<WebhookDto> CreateWebhookAsync(CreateWebhookDto dto);
    Task<WebhookDto> UpdateWebhookAsync(Guid externalId, UpdateWebhookDto dto);
    Task DeleteWebhookAsync(Guid externalId);
    Task<IEnumerable<WebhookEventDto>> GetDeliveryHistoryAsync(Guid webhookExternalId, int take = 50);
    Task<WebhookDto> TestWebhookAsync(Guid externalId);
}

/// <summary>
/// Interface for tenant service operations.
/// </summary>
public interface ITenantService
{
    Task<IEnumerable<TenantDto>> GetTenantsAsync(bool activeOnly = true);
    Task<IEnumerable<TenantDto>> GetAllTenantsAsync(bool activeOnly = false);
    Task<IEnumerable<TenantLookupDto>> GetTenantLookupsAsync(bool activeOnly = true);
    Task<TenantDto?> GetTenantAsync(string tenantId);
    Task<TenantStatsDto> GetTenantStatsAsync(string tenantId);
    Task<TenantDto> CreateTenantAsync(CreateTenantDto dto);
    Task<TenantDto> UpdateTenantAsync(string tenantId, CreateTenantDto dto);
}

/// <summary>
/// Interface for API token service operations.
/// </summary>
public interface IApiTokenService
{
    Task<IEnumerable<ApiTokenDto>> GetApiTokensAsync();
    Task<ApiTokenDto?> GetApiTokenAsync(Guid externalId);
    Task<ApiTokenCreatedDto> CreateApiTokenAsync(CreateApiTokenDto dto);
    Task<ApiTokenDto> UpdateApiTokenAsync(Guid externalId, UpdateApiTokenDto dto);
    Task RevokeApiTokenAsync(Guid externalId);
    Task<(bool isValid, string? tenantId, string? userId)> ValidateApiTokenAsync(string token);
}

/// <summary>
/// DTO for deployment events.
/// </summary>
public class DeploymentEventDto
{
    public Guid Id { get; set; }
    public Guid DeploymentId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string? Message { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// DTO for webhook events.
/// </summary>
public class WebhookEventDto
{
    public Guid Id { get; set; }
    public Guid WebhookId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string DeliveryStatus { get; set; } = string.Empty;
    public int? ResponseStatusCode { get; set; }
    public int RetryCount { get; set; }
    public DateTime? DeliveredAt { get; set; }
}
