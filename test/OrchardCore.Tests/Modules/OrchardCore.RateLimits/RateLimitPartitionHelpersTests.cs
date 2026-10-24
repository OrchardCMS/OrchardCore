using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using OrchardCore.RateLimits;

namespace OrchardCore.Tests.Modules.OrchardCore.RateLimits;

public class RateLimitPartitionHelpersTests
{
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
