using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using OrchardCore.RateLimits;

namespace OrchardCore.Tests.Modules.OrchardCore.RateLimits;

public class RateLimitPartitionHelpersTests
{
    [Fact]
    public async Task GetSlidingWindowLimiter_Rejected_TellsTheWindowAsRetryAfter()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var limiter = PartitionedRateLimiter.Create<string, string>(key => RateLimitPartitionHelpers.GetSlidingWindowLimiter(key, _ => new SlidingWindowRateLimiterOptions
        {
            PermitLimit = 1,
            QueueLimit = 0,
            Window = TimeSpan.FromSeconds(90),
            SegmentsPerWindow = 3,
        }));

        using var acquired = await limiter.AcquireAsync("a", 1, cancellationToken);
        using var rejected = await limiter.AcquireAsync("a", 1, cancellationToken);
        using var otherPartition = await limiter.AcquireAsync("b", 1, cancellationToken);

        Assert.True(acquired.IsAcquired);
        Assert.False(rejected.IsAcquired);
        Assert.True(otherPartition.IsAcquired);
        Assert.True(rejected.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter));
        Assert.Equal(TimeSpan.FromSeconds(90), retryAfter);
    }

    [Fact]
    public async Task GetSlidingWindowLimiter_WindowElapsed_ReplenishesThePermits()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var partition = RateLimitPartitionHelpers.GetSlidingWindowLimiter("a", _ => new SlidingWindowRateLimiterOptions
        {
            PermitLimit = 1,
            QueueLimit = 0,
            Window = TimeSpan.FromMilliseconds(50),
            SegmentsPerWindow = 1,
        });
        var limiter = Assert.IsAssignableFrom<ReplenishingRateLimiter>(partition.Factory("a"));

        using var acquired = await limiter.AcquireAsync(1, cancellationToken);
        await Task.Delay(200, cancellationToken);
        limiter.TryReplenish();
        using var afterTheWindow = await limiter.AcquireAsync(1, cancellationToken);

        Assert.False(limiter.IsAutoReplenishing);
        Assert.True(afterTheWindow.IsAcquired);
        await limiter.DisposeAsync();
    }

    [Fact]
    public void GetClientPartitionKey_SignedInUser_ReturnsUserKey()
    {
        var context = CreateContext("203.0.113.10");
        context.User = CreateUser("user-id");

        Assert.Equal(RateLimitPartitionHelpers.GetUserPartitionKey("user-id"), RateLimitPartitionHelpers.GetClientPartitionKey(context));
    }

    [Fact]
    public void GetClientPartitionKey_AnonymousRequest_ReturnsIpAddress()
    {
        var context = CreateContext("203.0.113.10");

        Assert.Equal("203.0.113.10", RateLimitPartitionHelpers.GetClientPartitionKey(context));
    }

    [Fact]
    public void GetClientPartitionKey_UnauthenticatedIdentity_ReturnsIpAddress()
    {
        var context = CreateContext("203.0.113.10");

        // An identity without an authentication type isn't authenticated.
        context.User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "user-id")]));

        Assert.Equal("203.0.113.10", RateLimitPartitionHelpers.GetClientPartitionKey(context));
    }

    [Fact]
    public void CreateSlidingWindowPolicy_UsersSharingAnIpAddress_UseDifferentPartitions()
    {
        var partitioner = RateLimitPartitionHelpers.CreateSlidingWindowPolicy("policy", 5, RateLimitPartitionHelpers.GetClientPartitionKey);

        var firstUser = CreateContext("203.0.113.10");
        firstUser.User = CreateUser("first-user");

        var secondUser = CreateContext("203.0.113.10");
        secondUser.User = CreateUser("second-user");

        Assert.NotEqual(partitioner(firstUser).PartitionKey, partitioner(secondUser).PartitionKey);
    }

    [Fact]
    public void CreateSlidingWindowPerIpPolicy_SignedInUser_UsesIpAddressPartition()
    {
        var context = CreateContext("203.0.113.10");
        context.User = CreateUser("user-id");

        var partition = RateLimitPartitionHelpers.CreateSlidingWindowPerIpPolicy("policy", 5)(context);

        Assert.Equal("policy:203.0.113.10", partition.PartitionKey);
    }

    private static DefaultHttpContext CreateContext(string ipAddress)
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse(ipAddress);

        return context;
    }

    private static ClaimsPrincipal CreateUser(string userId)
    {
        return new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId)], "Test"));
    }
}
