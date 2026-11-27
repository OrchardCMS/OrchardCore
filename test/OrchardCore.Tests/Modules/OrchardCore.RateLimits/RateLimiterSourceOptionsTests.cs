using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.RateLimits.Core;
using OrchardCore.RateLimits.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.RateLimits;

public class RateLimiterSourceOptionsTests
{
    [Theory]
    [InlineData(FixedWindowRateLimiterSource.SourceName, "Fixed window", typeof(FixedWindowRateLimiterSource))]
    [InlineData(SlidingWindowRateLimiterSource.SourceName, "Sliding window", typeof(SlidingWindowRateLimiterSource))]
    [InlineData(ConcurrencyRateLimiterSource.SourceName, "Concurrency", typeof(ConcurrencyRateLimiterSource))]
    [InlineData(TokenBucketRateLimiterSource.SourceName, "Token bucket", typeof(TokenBucketRateLimiterSource))]
    public void AddRateLimiterSource_BuiltInSource_DescribesItWithItsStaticMembers(string name, string displayName, Type sourceType)
    {
        // Arrange
        var services = new ServiceCollection()
            .AddOptions()
            .AddRateLimiterSource<FixedWindowRateLimiterSource>(FixedWindowRateLimiterSource.SourceName)
            .AddRateLimiterSource<SlidingWindowRateLimiterSource>(SlidingWindowRateLimiterSource.SourceName)
            .AddRateLimiterSource<ConcurrencyRateLimiterSource>(ConcurrencyRateLimiterSource.SourceName)
            .AddRateLimiterSource<TokenBucketRateLimiterSource>(TokenBucketRateLimiterSource.SourceName)
            .BuildServiceProvider();

        // Act
        var options = services.GetRequiredService<IOptions<RateLimiterSourceOptions>>().Value;

        // Assert
        var descriptor = Assert.Contains(name, options.Sources);
        Assert.Equal(displayName, descriptor.DisplayName.Value);
        Assert.Equal(sourceType, descriptor.DisplayName.Type);
        Assert.False(string.IsNullOrEmpty(descriptor.Description.Value));
        Assert.Equal(sourceType, descriptor.Description.Type);
    }
}
