using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System.Text.Json.Nodes;
using OrchardCore.Entities;
using OrchardCore.Modules;
using OrchardCore.RateLimits;
using OrchardCore.RateLimits.Models;
using OrchardCore.RateLimits.Services;
using OrchardCore.Users.Endpoints.EmailAuthenticator;
using OrchardCore.RateLimits.Core;

namespace OrchardCore.Tests.Modules.OrchardCore.RateLimits;

public class RateLimiterOptionsConfigurationsTests
{
    [Fact]
    public async Task ShouldApplyCustomRouteLimitForMatchingRouteNameAndMethod()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var rateLimitsOptions = new RateLimitsOptions();
        rateLimitsOptions.AddRouteRateLimit("Login", HttpMethods.Post, RateLimitPartitionHelpers.CreateFixedWindowPerIpPolicy("login", 1, TimeSpan.FromMinutes(1)));

        var options = Configure(rateLimitsOptions);

        using var firstLease = await options.GlobalLimiter.AcquireAsync(CreateHttpContext("Login", HttpMethods.Post), 1, cancellationToken);
        using var secondLease = await options.GlobalLimiter.AcquireAsync(CreateHttpContext("Login", HttpMethods.Post), 1, cancellationToken);

        Assert.True(firstLease.IsAcquired);
        Assert.False(secondLease.IsAcquired);
    }

    [Fact]
    public async Task ShouldOnlyApplyCustomRouteLimitToMatchingHttpMethods()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var rateLimitsOptions = new RateLimitsOptions();
        rateLimitsOptions.AddRouteRateLimit("Login", HttpMethods.Post, RateLimitPartitionHelpers.CreateFixedWindowPerIpPolicy("login", 1, TimeSpan.FromMinutes(1)));

        var options = Configure(rateLimitsOptions, publishedPolicies:
        [
            CreateGlobalFixedWindowPolicy("Global", 2, 60),
        ]);

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
        var rateLimitsOptions = new RateLimitsOptions();
        rateLimitsOptions.AddRouteRateLimit(SendCode.RouteName, HttpMethods.Post, RateLimitPartitionHelpers.CreateFixedWindowPerIpPolicy("send-code", 1, TimeSpan.FromMinutes(1)));

        var options = Configure(rateLimitsOptions);

        using var firstLease = await options.GlobalLimiter.AcquireAsync(CreateHttpContextFromEndpointName(SendCode.RouteName, HttpMethods.Post), 1, cancellationToken);
        using var secondLease = await options.GlobalLimiter.AcquireAsync(CreateHttpContextFromEndpointName(SendCode.RouteName, HttpMethods.Post), 1, cancellationToken);

        Assert.True(firstLease.IsAcquired);
        Assert.False(secondLease.IsAcquired);
    }

    [Fact]
    public async Task ShouldApplyPublishedGlobalPolicy()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var options = Configure(
            new RateLimitsOptions(),
            publishedPolicies:
            [
                CreateGlobalFixedWindowPolicy("Global", 1, 60),
            ]);

        using var firstLease = await options.GlobalLimiter.AcquireAsync(CreateHttpContext("AnyRoute", HttpMethods.Get), 1, cancellationToken);
        using var secondLease = await options.GlobalLimiter.AcquireAsync(CreateHttpContext("AnyRoute", HttpMethods.Get), 1, cancellationToken);

        Assert.True(firstLease.IsAcquired);
        Assert.False(secondLease.IsAcquired);
    }

    [Fact]
    public async Task ShouldIgnoreUnpublishedDraftPolicies()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var options = Configure(new RateLimitsOptions());

        using var firstLease = await options.GlobalLimiter.AcquireAsync(CreateHttpContext("AnyRoute", HttpMethods.Get), 1, cancellationToken);
        using var secondLease = await options.GlobalLimiter.AcquireAsync(CreateHttpContext("AnyRoute", HttpMethods.Get), 1, cancellationToken);

        Assert.True(firstLease.IsAcquired);
        Assert.True(secondLease.IsAcquired);
    }

    [Fact]
    public async Task ShouldApplyPublishedEndpointPolicy()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var policy = CreateGlobalFixedWindowPolicy("Api", 1, 60);
        policy.Scope = RateLimitPolicyScope.Endpoint;
        policy.Path = "/api";

        var options = Configure(new RateLimitsOptions(), [policy]);

        using var firstLease = await options.GlobalLimiter.AcquireAsync(CreateHttpContext("AnyRoute", HttpMethods.Get, "/api/users"), 1, cancellationToken);
        using var secondLease = await options.GlobalLimiter.AcquireAsync(CreateHttpContext("AnyRoute", HttpMethods.Get, "/api/users"), 1, cancellationToken);
        using var thirdLease = await options.GlobalLimiter.AcquireAsync(CreateHttpContext("AnyRoute", HttpMethods.Get, "/home"), 1, cancellationToken);
        using var fourthLease = await options.GlobalLimiter.AcquireAsync(CreateHttpContext("AnyRoute", HttpMethods.Get, "/home"), 1, cancellationToken);

        Assert.True(firstLease.IsAcquired);
        Assert.False(secondLease.IsAcquired);
        Assert.True(thirdLease.IsAcquired);
        Assert.True(fourthLease.IsAcquired);
    }

    [Fact]
    public async Task ShouldHandlePublishedPoliciesWithStringifiedNumericLimiterValues()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var limiter = new RateLimitLimiter
        {
            Id = IdGenerator.GenerateId(),
            Source = FixedWindowRateLimiterSource.SourceName,
            Properties = new JsonObject
            {
                [nameof(FixedWindowRateLimiterData)] = new JsonObject
                {
                    [nameof(FixedWindowRateLimiterData.PermitLimit)] = "1",
                    [nameof(FixedWindowRateLimiterData.QueueLimit)] = "0",
                    [nameof(FixedWindowRateLimiterData.WindowSeconds)] = "60",
                },
            },
        };

        var options = Configure(
            new RateLimitsOptions(),
            publishedPolicies:
            [
                new RateLimitPolicy
                {
                    Name = "Global",
                    Scope = RateLimitPolicyScope.Global,
                    Limiters = [limiter],
                },
            ]);

        using var firstLease = await options.GlobalLimiter.AcquireAsync(CreateHttpContext("AnyRoute", HttpMethods.Get), 1, cancellationToken);
        using var secondLease = await options.GlobalLimiter.AcquireAsync(CreateHttpContext("AnyRoute", HttpMethods.Get), 1, cancellationToken);

        Assert.True(firstLease.IsAcquired);
        Assert.False(secondLease.IsAcquired);
    }

    private static RateLimiterOptions Configure(
        RateLimitsOptions rateLimitsOptions,
        IEnumerable<RateLimitPolicy> publishedPolicies = null)
    {
        var options = new RateLimiterOptions();
        using var serviceProvider = CreateServiceProvider();
        new RateLimiterOptionsConfigurations(
            Options.Create(rateLimitsOptions),
            CreatePolicyStore(publishedPolicies ?? []),
            serviceProvider).Configure(options);

        return options;
    }

    private static IRateLimitPolicyStore CreatePolicyStore(IEnumerable<RateLimitPolicy> publishedPolicies)
    {
        var store = new Mock<IRateLimitPolicyStore>();
        store.Setup(x => x.GetAllAsync(PolicyVersion.Published)).Returns(() =>
            ValueTask.FromResult<IReadOnlyCollection<RateLimitPolicy>>([.. publishedPolicies]));

        return store.Object;
    }

    private static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();

        services.AddSingleton(new FixedWindowRateLimiterSource(Mock.Of<IStringLocalizer<FixedWindowRateLimiterSource>>()));
        services.AddSingleton(new SlidingWindowRateLimiterSource(Mock.Of<IStringLocalizer<SlidingWindowRateLimiterSource>>()));
        services.AddSingleton(new ConcurrencyRateLimiterSource(Mock.Of<IStringLocalizer<ConcurrencyRateLimiterSource>>()));
        services.AddSingleton(new TokenBucketRateLimiterSource(Mock.Of<IStringLocalizer<TokenBucketRateLimiterSource>>()));
        services.AddKeyedSingleton<IRateLimiterSource>(FixedWindowRateLimiterSource.SourceName, static (sp, _) => sp.GetRequiredService<FixedWindowRateLimiterSource>());
        services.AddKeyedSingleton<IRateLimiterSource>(SlidingWindowRateLimiterSource.SourceName, static (sp, _) => sp.GetRequiredService<SlidingWindowRateLimiterSource>());
        services.AddKeyedSingleton<IRateLimiterSource>(ConcurrencyRateLimiterSource.SourceName, static (sp, _) => sp.GetRequiredService<ConcurrencyRateLimiterSource>());
        services.AddKeyedSingleton<IRateLimiterSource>(TokenBucketRateLimiterSource.SourceName, static (sp, _) => sp.GetRequiredService<TokenBucketRateLimiterSource>());

        return services.BuildServiceProvider();
    }

    private static RateLimitPolicy CreateGlobalFixedWindowPolicy(string name, int permitLimit, int windowSeconds)
    {
        var limiter = new RateLimitLimiter
        {
            Id = IdGenerator.GenerateId(),
            Source = FixedWindowRateLimiterSource.SourceName,
        };

        limiter.Put(new FixedWindowRateLimiterData
        {
            PermitLimit = permitLimit,
            QueueLimit = 0,
            WindowSeconds = windowSeconds,
        });

        return new RateLimitPolicy
        {
            Name = name,
            Scope = RateLimitPolicyScope.Global,
            Limiters = [limiter],
        };
    }

    private static DefaultHttpContext CreateHttpContext(string routeName, string httpMethod, string path = "/")
    {
        var context = new DefaultHttpContext();
        context.Request.Method = httpMethod;
        context.Request.Path = path;
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
