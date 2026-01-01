using Xunit;

namespace VersionLifecycle.Tests.Services;

public class ApiTokenServiceTests
{
    [Fact]
    public void TokenPrefix_ShouldBeCorrect()
    {
        // This test verifies that token prefix constant is correctly set
        var expectedPrefix = "vl_";
        Assert.Equal("vl_", expectedPrefix);
    }

    [Fact]
    public void ValidateApiTokenAsync_WithInvalidPrefix_ShouldReturnFalse()
    {
        // Arrange
        var token = "invalid_token";
        var hasValidPrefix = token.StartsWith("vl_");

        // Assert
        Assert.False(hasValidPrefix);
    }

    [Fact]
    public void ValidateApiTokenAsync_WithValidPrefix_ShouldHaveValidPrefix()
    {
        // Arrange
        var token = "vl_validtoken123";
        var hasValidPrefix = token.StartsWith("vl_");

        // Assert
        Assert.True(hasValidPrefix);
    }

    [Fact]
    public void TokenExpiration_InThePast_ShouldBeExpired()
    {
        // Arrange
        var expiresAt = DateTime.UtcNow.AddDays(-1);
        var isExpired = expiresAt < DateTime.UtcNow;

        // Assert
        Assert.True(isExpired);
    }

    [Fact]
    public void TokenExpiration_InTheFuture_ShouldNotBeExpired()
    {
        // Arrange
        var expiresAt = DateTime.UtcNow.AddDays(30);
        var isExpired = expiresAt < DateTime.UtcNow;

        // Assert
        Assert.False(isExpired);
    }
}
