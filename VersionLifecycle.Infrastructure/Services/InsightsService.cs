namespace VersionLifecycle.Infrastructure.Services;

using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using VersionLifecycle.Application.DTOs;
using VersionLifecycle.Application.Services;
using VersionLifecycle.Infrastructure.Data;

/// <summary>
/// AI-powered insights service that answers natural language questions about tenant data.
/// </summary>
public class InsightsService(
    AppDbContext dbContext,
    IChatClient? chatClient,
    ILogger<InsightsService> logger) : IInsightsService
{
    private const int MaxApplications = 50;
    private const int MaxVersionsPerApp = 20;
    private const int MaxDeployments = 100;
    private const int MaxEnvironments = 20;

    public bool IsAvailable => chatClient != null;

    public async Task<InsightsResponseDto> AskQuestionAsync(InsightsQueryDto query)
    {
        if (string.IsNullOrWhiteSpace(query.Question))
        {
            return new InsightsResponseDto
            {
                Question = query.Question,
                Answer = "Please provide a question to ask.",
                GeneratedAt = DateTime.UtcNow
            };
        }

        if (chatClient == null)
        {
            return new InsightsResponseDto
            {
                Question = query.Question,
                Answer = "The AI insights feature is not configured. Please set the AI:Provider and AI:ApiKey configuration values to enable this feature.",
                GeneratedAt = DateTime.UtcNow
            };
        }

        try
        {
            var dataContext = await GatherTenantDataContextAsync();
            var systemPrompt = BuildSystemPrompt(dataContext);

            var messages = new List<ChatMessage>
            {
                new(ChatRole.System, systemPrompt),
                new(ChatRole.User, query.Question)
            };

            var response = await chatClient.GetResponseAsync(messages);
            var answer = response.Text ?? "I was unable to generate a response. Please try rephrasing your question.";

            return new InsightsResponseDto
            {
                Question = query.Question,
                Answer = answer,
                GeneratedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing insights query: {Question}", query.Question);
            return new InsightsResponseDto
            {
                Question = query.Question,
                Answer = "An error occurred while processing your question. Please try again later.",
                GeneratedAt = DateTime.UtcNow
            };
        }
    }

    internal async Task<TenantDataContext> GatherTenantDataContextAsync()
    {
        var applications = await dbContext.Applications
            .Where(a => !a.IsDeleted)
            .OrderBy(a => a.Name)
            .Take(MaxApplications)
            .Select(a => new { a.Id, a.Name, a.Description, a.RepositoryUrl, a.CreatedAt })
            .ToListAsync();

        var appIds = applications.Select(a => a.Id).ToList();

        var versions = await dbContext.Versions
            .Where(v => !v.IsDeleted && appIds.Contains(v.ApplicationId))
            .OrderByDescending(v => v.CreatedAt)
            .Take(MaxApplications * MaxVersionsPerApp)
            .Select(v => new { v.ApplicationId, v.VersionNumber, v.Status, v.ReleaseNotes, v.CreatedAt })
            .ToListAsync();

        var environments = await dbContext.Environments
            .Where(e => !e.IsDeleted)
            .OrderBy(e => e.Order)
            .Take(MaxEnvironments)
            .Select(e => new { e.Id, e.Name, e.Description, e.Order })
            .ToListAsync();

        var deployments = await dbContext.Deployments
            .Where(d => !d.IsDeleted)
            .OrderByDescending(d => d.CreatedAt)
            .Take(MaxDeployments)
            .Include(d => d.Application)
            .Include(d => d.Version)
            .Include(d => d.Environment)
            .Select(d => new
            {
                ApplicationName = d.Application != null ? d.Application.Name : "Unknown",
                VersionNumber = d.Version != null ? d.Version.VersionNumber : "Unknown",
                EnvironmentName = d.Environment != null ? d.Environment.Name : "Unknown",
                d.Status,
                d.DeployedAt,
                d.CompletedAt,
                d.DurationMs,
                d.Notes,
                d.CreatedAt
            })
            .ToListAsync();

        var context = new TenantDataContext();

        foreach (var app in applications)
        {
            var appVersions = versions
                .Where(v => v.ApplicationId == app.Id)
                .Take(MaxVersionsPerApp)
                .Select(v => $"  - Version {v.VersionNumber} (Status: {v.Status}, Created: {v.CreatedAt:yyyy-MM-dd})" +
                             (string.IsNullOrEmpty(v.ReleaseNotes) ? "" : $" - Notes: {v.ReleaseNotes}"))
                .ToList();

            context.Applications.Add($"Application: {app.Name}" +
                (string.IsNullOrEmpty(app.Description) ? "" : $" - {app.Description}") +
                (string.IsNullOrEmpty(app.RepositoryUrl) ? "" : $" (Repo: {app.RepositoryUrl})") +
                $" (Created: {app.CreatedAt:yyyy-MM-dd})" +
                (appVersions.Count > 0 ? "\n" + string.Join("\n", appVersions) : " - No versions"));
        }

        foreach (var env in environments)
        {
            context.Environments.Add($"Environment: {env.Name}" +
                (string.IsNullOrEmpty(env.Description) ? "" : $" - {env.Description}") +
                $" (Order: {env.Order})");
        }

        foreach (var dep in deployments)
        {
            context.Deployments.Add(
                $"Deployment: {dep.ApplicationName} v{dep.VersionNumber} → {dep.EnvironmentName} " +
                $"(Status: {dep.Status}, Deployed: {dep.DeployedAt.ToString("yyyy-MM-dd HH:mm")}" +
                (dep.CompletedAt.HasValue ? $", Completed: {dep.CompletedAt:yyyy-MM-dd HH:mm}" : "") +
                (dep.DurationMs.HasValue ? $", Duration: {dep.DurationMs}ms" : "") +
                (string.IsNullOrEmpty(dep.Notes) ? "" : $", Notes: {dep.Notes}") +
                ")");
        }

        context.Summary = $"Total: {applications.Count} applications, {versions.Count} versions, " +
                          $"{environments.Count} environments, {deployments.Count} deployments";

        return context;
    }

    internal static string BuildSystemPrompt(TenantDataContext dataContext)
    {
        var sb = new StringBuilder();
        sb.AppendLine("You are an AI assistant for a version lifecycle management system. You answer questions about the user's applications, versions, deployments, and environments.");
        sb.AppendLine();
        sb.AppendLine("IMPORTANT RULES:");
        sb.AppendLine("- Only answer questions based on the data provided below.");
        sb.AppendLine("- If the data doesn't contain enough information to answer, say so clearly.");
        sb.AppendLine("- Be concise and precise in your answers.");
        sb.AppendLine("- When referring to dates, use human-readable formats.");
        sb.AppendLine("- Do not make up or infer data that is not explicitly provided.");
        sb.AppendLine("- Format your response in plain text. Use bullet points or numbered lists when listing multiple items.");
        sb.AppendLine();
        sb.AppendLine("=== DATA CONTEXT ===");
        sb.AppendLine();
        sb.AppendLine(dataContext.Summary);
        sb.AppendLine();

        if (dataContext.Applications.Count > 0)
        {
            sb.AppendLine("--- Applications & Versions ---");
            foreach (var app in dataContext.Applications)
            {
                sb.AppendLine(app);
            }
            sb.AppendLine();
        }

        if (dataContext.Environments.Count > 0)
        {
            sb.AppendLine("--- Environments ---");
            foreach (var env in dataContext.Environments)
            {
                sb.AppendLine(env);
            }
            sb.AppendLine();
        }

        if (dataContext.Deployments.Count > 0)
        {
            sb.AppendLine("--- Recent Deployments ---");
            foreach (var dep in dataContext.Deployments)
            {
                sb.AppendLine(dep);
            }
            sb.AppendLine();
        }

        sb.AppendLine("=== END DATA CONTEXT ===");

        return sb.ToString();
    }

    /// <summary>
    /// Internal DTO for holding gathered tenant data.
    /// </summary>
    internal class TenantDataContext
    {
        public List<string> Applications { get; set; } = [];
        public List<string> Environments { get; set; } = [];
        public List<string> Deployments { get; set; } = [];
        public string Summary { get; set; } = string.Empty;
    }
}
