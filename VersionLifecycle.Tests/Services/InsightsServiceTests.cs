using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Moq;
using VersionLifecycle.Application.DTOs;
using VersionLifecycle.Core.Enums;
using VersionLifecycle.Infrastructure.Services;
using VersionLifecycle.Tests.Fixtures;
using Xunit;
using AppEntity = VersionLifecycle.Core.Entities.Application;
using VersionEntity = VersionLifecycle.Core.Entities.Version;

namespace VersionLifecycle.Tests.Services;

public class InsightsServiceTests : IDisposable
{
    private readonly InMemoryDbContextFixture _fixture;

    public InsightsServiceTests()
    {
        _fixture = new InMemoryDbContextFixture();
    }

    public void Dispose()
    {
        _fixture.Dispose();
    }

    [Fact]
    public async Task AskQuestionAsync_WhenNoChatClient_ReturnsConfigurationMessage()
    {
        // Arrange
        using var dbContext = _fixture.CreateDbContext();
        var logger = Mock.Of<ILogger<InsightsService>>();
        var service = new InsightsService(dbContext, null, logger);

        var query = new InsightsQueryDto { Question = "What applications do I have?" };

        // Act
        var result = await service.AskQuestionAsync(query);

        // Assert
        Assert.Equal(query.Question, result.Question);
        Assert.Contains("not configured", result.Answer);
    }

    [Fact]
    public async Task AskQuestionAsync_WhenEmptyQuestion_ReturnsPromptMessage()
    {
        // Arrange
        using var dbContext = _fixture.CreateDbContext();
        var logger = Mock.Of<ILogger<InsightsService>>();
        var service = new InsightsService(dbContext, null, logger);

        var query = new InsightsQueryDto { Question = "   " };

        // Act
        var result = await service.AskQuestionAsync(query);

        // Assert
        Assert.Contains("Please provide a question", result.Answer);
    }

    [Fact]
    public void IsAvailable_WhenNoChatClient_ReturnsFalse()
    {
        // Arrange
        using var dbContext = _fixture.CreateDbContext();
        var logger = Mock.Of<ILogger<InsightsService>>();
        var service = new InsightsService(dbContext, null, logger);

        // Assert
        Assert.False(service.IsAvailable);
    }

    [Fact]
    public void IsAvailable_WhenChatClientProvided_ReturnsTrue()
    {
        // Arrange
        using var dbContext = _fixture.CreateDbContext();
        var logger = Mock.Of<ILogger<InsightsService>>();
        var mockChatClient = Mock.Of<IChatClient>();
        var service = new InsightsService(dbContext, mockChatClient, logger);

        // Assert
        Assert.True(service.IsAvailable);
    }

    [Fact]
    public async Task AskQuestionAsync_WithChatClient_ReturnsAnswer()
    {
        // Arrange
        using var dbContext = _fixture.CreateDbContext();
        var logger = Mock.Of<ILogger<InsightsService>>();

        var mockChatClient = new Mock<IChatClient>();
        var chatResponse = new ChatResponse(new ChatMessage(ChatRole.Assistant, "You have 1 application called TestApp."));
        mockChatClient
            .Setup(c => c.GetResponseAsync(
                It.IsAny<IList<ChatMessage>>(),
                It.IsAny<ChatOptions?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(chatResponse);

        // Seed data
        dbContext.Applications.Add(new AppEntity
        {
            Name = "TestApp",
            Description = "A test application",
            TenantId = "test-tenant",
            CreatedBy = "test-user"
        });
        await dbContext.SaveChangesAsync();

        var service = new InsightsService(dbContext, mockChatClient.Object, logger);
        var query = new InsightsQueryDto { Question = "What applications do I have?" };

        // Act
        var result = await service.AskQuestionAsync(query);

        // Assert
        Assert.Equal(query.Question, result.Question);
        Assert.Equal("You have 1 application called TestApp.", result.Answer);
        Assert.True(result.GeneratedAt <= DateTime.UtcNow);
    }

    [Fact]
    public async Task AskQuestionAsync_WhenChatClientThrows_ReturnsErrorMessage()
    {
        // Arrange
        using var dbContext = _fixture.CreateDbContext();
        var logger = Mock.Of<ILogger<InsightsService>>();

        var mockChatClient = new Mock<IChatClient>();
        mockChatClient
            .Setup(c => c.GetResponseAsync(
                It.IsAny<IList<ChatMessage>>(),
                It.IsAny<ChatOptions?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("API error"));

        var service = new InsightsService(dbContext, mockChatClient.Object, logger);
        var query = new InsightsQueryDto { Question = "What applications do I have?" };

        // Act
        var result = await service.AskQuestionAsync(query);

        // Assert
        Assert.Contains("error occurred", result.Answer);
    }

    [Fact]
    public async Task GatherTenantDataContextAsync_IncludesApplicationsAndVersions()
    {
        // Arrange
        using var dbContext = _fixture.CreateDbContext();
        var logger = Mock.Of<ILogger<InsightsService>>();
        var service = new InsightsService(dbContext, null, logger);

        var app = new AppEntity
        {
            Name = "MyApp",
            Description = "My application",
            RepositoryUrl = "https://github.com/test/myapp",
            TenantId = "test-tenant",
            CreatedBy = "test-user"
        };
        dbContext.Applications.Add(app);
        await dbContext.SaveChangesAsync();

        dbContext.Versions.Add(new VersionEntity
        {
            ApplicationId = app.Id,
            VersionNumber = "1.0.0",
            Status = VersionStatus.Released,
            ReleaseNotes = "Initial release",
            TenantId = "test-tenant",
            CreatedBy = "test-user"
        });
        await dbContext.SaveChangesAsync();

        // Act
        var context = await service.GatherTenantDataContextAsync();

        // Assert
        Assert.Single(context.Applications);
        Assert.Contains("MyApp", context.Applications[0]);
        Assert.Contains("1.0.0", context.Applications[0]);
        Assert.Contains("1 applications", context.Summary);
    }

    [Fact]
    public void BuildSystemPrompt_ContainsExpectedSections()
    {
        // Arrange
        var context = new InsightsService.TenantDataContext
        {
            Applications = ["Application: TestApp - A test app"],
            Environments = ["Environment: Production"],
            Deployments = ["Deployment: TestApp v1.0.0 → Production"],
            Summary = "Total: 1 applications, 1 versions, 1 environments, 1 deployments"
        };

        // Act
        var prompt = InsightsService.BuildSystemPrompt(context);

        // Assert
        Assert.Contains("version lifecycle management", prompt);
        Assert.Contains("Applications & Versions", prompt);
        Assert.Contains("Environments", prompt);
        Assert.Contains("Recent Deployments", prompt);
        Assert.Contains("TestApp", prompt);
        Assert.Contains("Production", prompt);
    }
}
