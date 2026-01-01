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
    Task<ApplicationDto?> GetApplicationAsync(int id);
    Task<ApplicationDto> CreateApplicationAsync(CreateApplicationDto dto);
    Task<ApplicationDto> UpdateApplicationAsync(int id, UpdateApplicationDto dto);
    Task DeleteApplicationAsync(int id);
}

/// <summary>
/// Interface for version service operations.
/// </summary>
public interface IVersionService
{
    Task<IEnumerable<VersionDto>> GetVersionsByApplicationAsync(int applicationId);
    Task<VersionDto?> GetVersionAsync(int id);
    Task<VersionDto> CreateVersionAsync(CreateVersionDto dto);
    Task<VersionDto> UpdateVersionAsync(int id, UpdateVersionDto dto);
    Task DeleteVersionAsync(int id);
}

/// <summary>
/// Interface for deployment service operations.
/// </summary>
public interface IDeploymentService
{
    Task<PaginatedResponse<DeploymentDto>> GetDeploymentsAsync(int skip = 0, int take = 25, string? statusFilter = null);
    Task<DeploymentDto?> GetDeploymentAsync(int id);
    Task<DeploymentDto> CreatePendingDeploymentAsync(CreatePendingDeploymentDto dto);
    Task<DeploymentDto> ConfirmDeploymentAsync(int id, ConfirmDeploymentDto dto);
    Task<DeploymentDto> UpdateDeploymentStatusAsync(int id, UpdateDeploymentStatusDto dto);
    Task<DeploymentDto> PromoteDeploymentAsync(int id, PromoteDeploymentDto dto);
    Task<IEnumerable<DeploymentEventDto>> GetDeploymentHistoryAsync(int deploymentId);
}

/// <summary>
/// Interface for environment service operations.
/// </summary>
public interface IEnvironmentService
{
    Task<IEnumerable<EnvironmentDto>> GetEnvironmentsAsync();
    Task<EnvironmentDto?> GetEnvironmentAsync(int id);
    Task<EnvironmentDto> CreateEnvironmentAsync(CreateEnvironmentDto dto);
    Task<EnvironmentDto> UpdateEnvironmentAsync(int id, UpdateEnvironmentDto dto);
    Task DeleteEnvironmentAsync(int id);

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
    Task<IEnumerable<WebhookDto>> GetWebhooksAsync(int applicationId);
    Task<WebhookDto?> GetWebhookAsync(int id);
    Task<WebhookDto> CreateWebhookAsync(CreateWebhookDto dto);
    Task<WebhookDto> UpdateWebhookAsync(int id, UpdateWebhookDto dto);
    Task DeleteWebhookAsync(int id);
    Task<IEnumerable<WebhookEventDto>> GetDeliveryHistoryAsync(int webhookId, int take = 50);
    Task<WebhookDto> TestWebhookAsync(int id);
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
    Task<ApiTokenDto?> GetApiTokenAsync(int id);
    Task<ApiTokenCreatedDto> CreateApiTokenAsync(CreateApiTokenDto dto);
    Task<ApiTokenDto> UpdateApiTokenAsync(int id, UpdateApiTokenDto dto);
    Task RevokeApiTokenAsync(int id);
    Task<(bool isValid, string? tenantId, string? userId)> ValidateApiTokenAsync(string token);
}

/// <summary>
/// DTO for deployment events.
/// </summary>
public class DeploymentEventDto
{
    public int Id { get; set; }
    public int DeploymentId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string? Message { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// DTO for webhook events.
/// </summary>
public class WebhookEventDto
{
    public int Id { get; set; }
    public int WebhookId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string DeliveryStatus { get; set; } = string.Empty;
    public int? ResponseStatusCode { get; set; }
    public int RetryCount { get; set; }
    public DateTime? DeliveredAt { get; set; }
}
