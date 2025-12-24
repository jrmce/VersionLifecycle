using Xunit;

namespace VersionLifecycle.Tests;

public class BasicTests
{
    [Fact]
    public void TrueIsTrue()
    {
        // This is a placeholder test to ensure the test project compiles
        // and test discovery works properly
        Assert.True(true);
    }

    [Fact]
    public void FalseIsFalse()
    {
        Assert.False(false);
    }

    [Theory]
    [InlineData(1, 1, 2)]
    [InlineData(2, 2, 4)]
    [InlineData(5, 5, 10)]
    public void AdditionWorks(int a, int b, int expected)
    {
        var result = a + b;
        Assert.Equal(expected, result);
    }
}
