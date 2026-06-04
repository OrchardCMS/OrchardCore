using Microsoft.AspNetCore.RateLimiting;
using OrchardCore.RateLimits;
using OrchardCore.Users.Endpoints.EmailAuthenticator;

namespace OrchardCore.Tests.Modules.OrchardCore.RateLimits;

public class RateLimiterOptionsConfigurationsTests
{
    [Fact]
    public async Task ShouldApplyCustomRouteLimitForMatchingRouteNameAndMethod()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var globalOptions = Options.Create(new GlobalRateLimitOptions
        {
            PermitLimit = 3,
        });

        var rateLimitsOptions = new RateLimitsOptions();
        rateLimitsOptions.AddRouteRateLimit("Login", HttpMethods.Post, RateLimitPartitionHelpers.CreateFixedWindowPerIpPolicy("login", 1, TimeSpan.FromMinutes(1)));

        var options = Configure(globalOptions, Options.Create(rateLimitsOptions));

        using var firstLease = await options.GlobalLimiter.AcquireAsync(CreateHttpContext("Login", HttpMethods.Post), 1, cancellationToken);
        using var secondLease = await options.GlobalLimiter.AcquireAsync(CreateHttpContext("Login", HttpMethods.Post), 1, cancellationToken);

        Assert.True(firstLease.IsAcquired);
        Assert.False(secondLease.IsAcquired);
    }

    [Fact]
    public async Task ShouldOnlyApplyCustomRouteLimitToMatchingHttpMethods()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var globalOptions = Options.Create(new GlobalRateLimitOptions
        {
            PermitLimit = 2,
        });

        var rateLimitsOptions = new RateLimitsOptions();
        rateLimitsOptions.AddRouteRateLimit("Login", HttpMethods.Post, RateLimitPartitionHelpers.CreateFixedWindowPerIpPolicy("login", 1, TimeSpan.FromMinutes(1)));

        var options = Configure(globalOptions, Options.Create(rateLimitsOptions));

        using var firstLease = await options.GlobalLimiter.AcquireAsync(CreateHttpContext("Login", HttpMethods.Get), 1, cancellationToken);
        using var secondLease = await options.GlobalLimiter.AcquireAsync(CreateHttpContext("Login", HttpMethods.Get), 1, cancellationToken);
        using var thirdLease = await options.GlobalLimiter.AcquireAsync(CreateHttpContext("Login", HttpMethods.Get), 1, cancellationToken);

        Assert.True(firstLease.IsAcquired);
        Assert.True(secondLease.IsAcquired);
        Assert.False(thirdLease.IsAcquired);
    }

    [Fact]
    public async Task ShouldApplyCustomRouteLimitToEndpointNameMetadata()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var globalOptions = Options.Create(new GlobalRateLimitOptions
        {
            PermitLimit = 3,
        });

        var rateLimitsOptions = new RateLimitsOptions();
        rateLimitsOptions.AddRouteRateLimit(SendCode.RouteName, HttpMethods.Post, RateLimitPartitionHelpers.CreateFixedWindowPerIpPolicy("send-code", 1, TimeSpan.FromMinutes(1)));

        var options = Configure(globalOptions, Options.Create(rateLimitsOptions));

        using var firstLease = await options.GlobalLimiter.AcquireAsync(CreateHttpContextFromEndpointName(SendCode.RouteName, HttpMethods.Post), 1, cancellationToken);
        using var secondLease = await options.GlobalLimiter.AcquireAsync(CreateHttpContextFromEndpointName(SendCode.RouteName, HttpMethods.Post), 1, cancellationToken);

        Assert.True(firstLease.IsAcquired);
        Assert.False(secondLease.IsAcquired);
    }

    private static RateLimiterOptions Configure(
        IOptions<GlobalRateLimitOptions> globalOptions,
        IOptions<RateLimitsOptions> rateLimitsOptions)
    {
        var options = new RateLimiterOptions();
        new RateLimiterOptionsConfigurations(globalOptions, rateLimitsOptions).Configure(options);
        return options;
    }

    private static DefaultHttpContext CreateHttpContext(string routeName, string httpMethod)
    {
        var context = new DefaultHttpContext();
        context.Request.Method = httpMethod;
        context.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
        context.SetEndpoint(new Endpoint(
            _ => Task.CompletedTask,
            new EndpointMetadataCollection(new RouteNameMetadata(routeName)),
            routeName));

        return context;
    }

    private static DefaultHttpContext CreateHttpContextFromEndpointName(string endpointName, string httpMethod)
    {
        var context = new DefaultHttpContext();
        context.Request.Method = httpMethod;
        context.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
        context.SetEndpoint(new Endpoint(
            _ => Task.CompletedTask,
            new EndpointMetadataCollection(new EndpointNameMetadata(endpointName)),
            endpointName));

        return context;
    }
}
