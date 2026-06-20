using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System.Threading.RateLimiting;
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

        var options = Configure(rateLimitsOptions, enabledPolicies:
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
    public async Task ShouldApplyCustomGroupLimitToMatchingEndpointGroup()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var rateLimitsOptions = new RateLimitsOptions();
        rateLimitsOptions.AddGroupRateLimit("authentication", RateLimitPartitionHelpers.CreateFixedWindowPerIpPolicy("authentication", 1, TimeSpan.FromMinutes(1)));

        var options = Configure(rateLimitsOptions);

        using var firstLease = await options.GlobalLimiter.AcquireAsync(CreateHttpContext("Login", HttpMethods.Post, metadata: new RateLimitGroupAttribute("authentication")), 1, cancellationToken);
        using var secondLease = await options.GlobalLimiter.AcquireAsync(CreateHttpContext("Login", HttpMethods.Post, metadata: new RateLimitGroupAttribute("authentication")), 1, cancellationToken);
        using var thirdLease = await options.GlobalLimiter.AcquireAsync(CreateHttpContext("Login", HttpMethods.Post, metadata: new RateLimitGroupAttribute("registration")), 1, cancellationToken);

        Assert.True(firstLease.IsAcquired);
        Assert.False(secondLease.IsAcquired);
        Assert.True(thirdLease.IsAcquired);
    }

    [Fact]
    public async Task ShouldApplyEnabledGlobalPolicy()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var options = Configure(
            new RateLimitsOptions(),
            enabledPolicies:
            [
                CreateGlobalFixedWindowPolicy("Global", 1, 60),
            ]);

        using var firstLease = await options.GlobalLimiter.AcquireAsync(CreateHttpContext("AnyRoute", HttpMethods.Get), 1, cancellationToken);
        using var secondLease = await options.GlobalLimiter.AcquireAsync(CreateHttpContext("AnyRoute", HttpMethods.Get), 1, cancellationToken);

        Assert.True(firstLease.IsAcquired);
        Assert.False(secondLease.IsAcquired);
    }

    [Fact]
    public async Task ShouldIgnoreDisabledPolicies()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var disabledPolicy = CreateGlobalFixedWindowPolicy("Disabled", 1, 60);
        disabledPolicy.IsEnabled = false;
        disabledPolicy.EnabledUtc = null;

        var options = Configure(new RateLimitsOptions(), [disabledPolicy]);

        using var firstLease = await options.GlobalLimiter.AcquireAsync(CreateHttpContext("AnyRoute", HttpMethods.Get), 1, cancellationToken);
        using var secondLease = await options.GlobalLimiter.AcquireAsync(CreateHttpContext("AnyRoute", HttpMethods.Get), 1, cancellationToken);

        Assert.True(firstLease.IsAcquired);
        Assert.True(secondLease.IsAcquired);
    }

    [Fact]
    public async Task ShouldApplyEnabledFixedWindowEndpointPolicy()
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
    public async Task ShouldApplyEnabledSlidingWindowEndpointPolicy()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var policy = CreateEndpointSlidingWindowPolicy("Health", "/health/live", 10, 59, 1);
        var options = Configure(new RateLimitsOptions(), [policy]);

        for (var i = 0; i < 10; i++)
        {
            using var lease = await options.GlobalLimiter.AcquireAsync(CreateHttpContext("Health.Live", HttpMethods.Get, "/health/live"), 1, cancellationToken);
            Assert.True(lease.IsAcquired);
        }

        using var rejectedLease = await options.GlobalLimiter.AcquireAsync(CreateHttpContext("Health.Live", HttpMethods.Get, "/health/live"), 1, cancellationToken);
        Assert.False(rejectedLease.IsAcquired);
    }

    [Fact]
    public async Task ShouldApplyEnabledTokenBucketEndpointPolicy()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var policy = CreateEndpointTokenBucketPolicy("ApiBurst", "/api", 1, 1, 60);
        var options = Configure(new RateLimitsOptions(), [policy]);

        using var firstLease = await options.GlobalLimiter.AcquireAsync(CreateHttpContext("Api.Route", HttpMethods.Get, "/api/users"), 1, cancellationToken);
        using var secondLease = await options.GlobalLimiter.AcquireAsync(CreateHttpContext("Api.Route", HttpMethods.Get, "/api/users"), 1, cancellationToken);
        using var thirdLease = await options.GlobalLimiter.AcquireAsync(CreateHttpContext("Home.Route", HttpMethods.Get, "/home"), 1, cancellationToken);

        Assert.True(firstLease.IsAcquired);
        Assert.False(secondLease.IsAcquired);
        Assert.True(thirdLease.IsAcquired);
    }

    [Fact]
    public async Task ShouldApplyEnabledConcurrencyEndpointPolicy()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var policy = CreateEndpointConcurrencyPolicy("ExpensiveApi", "/api/expensive", 1);
        var options = Configure(new RateLimitsOptions(), [policy]);

        using var firstLease = await options.GlobalLimiter.AcquireAsync(CreateHttpContext("Api.Expensive", HttpMethods.Get, "/api/expensive"), 1, cancellationToken);
        using var secondLease = await options.GlobalLimiter.AcquireAsync(CreateHttpContext("Api.Expensive", HttpMethods.Get, "/api/expensive"), 1, cancellationToken);
        using var thirdLease = await options.GlobalLimiter.AcquireAsync(CreateHttpContext("Home.Route", HttpMethods.Get, "/home"), 1, cancellationToken);

        Assert.True(firstLease.IsAcquired);
        Assert.False(secondLease.IsAcquired);
        Assert.True(thirdLease.IsAcquired);
    }

    [Fact]
    public async Task ShouldApplyEnabledGroupPolicyToAnyMatchingGroup()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var policy = CreateGlobalFixedWindowPolicy("Authentication", 1, 60);
        policy.Scope = RateLimitPolicyScope.Group;
        policy.GroupName = "authentication";

        var options = Configure(new RateLimitsOptions(), [policy]);

        using var firstLease = await options.GlobalLimiter.AcquireAsync(CreateHttpContext("AnyRoute", HttpMethods.Get, metadata: new RateLimitGroupAttribute("users", "authentication")), 1, cancellationToken);
        using var secondLease = await options.GlobalLimiter.AcquireAsync(CreateHttpContext("AnyRoute", HttpMethods.Get, metadata: new RateLimitGroupAttribute("authentication")), 1, cancellationToken);
        using var thirdLease = await options.GlobalLimiter.AcquireAsync(CreateHttpContext("AnyRoute", HttpMethods.Get, metadata: new RateLimitGroupAttribute("users")), 1, cancellationToken);

        Assert.True(firstLease.IsAcquired);
        Assert.False(secondLease.IsAcquired);
        Assert.True(thirdLease.IsAcquired);
    }

    [Fact]
    public async Task ShouldHandleEnabledPoliciesWithStringifiedNumericLimiterValues()
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
            enabledPolicies:
            [
                new RateLimitPolicy
                {
                    Name = "Global",
                    IsEnabled = true,
                    EnabledUtc = DateTime.UtcNow,
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
        IEnumerable<RateLimitPolicy> enabledPolicies = null)
    {
        var options = new RateLimiterOptions();
        using var serviceProvider = CreateServiceProvider();
        new RateLimiterOptionsConfigurations(
            Options.Create(rateLimitsOptions),
            CreatePolicyStore(enabledPolicies ?? []),
            serviceProvider).Configure(options);

        return options;
    }

    private static IRateLimitPolicyStore CreatePolicyStore(IEnumerable<RateLimitPolicy> enabledPolicies)
    {
        var store = new Mock<IRateLimitPolicyStore>();
        store.Setup(x => x.GetAllAsync(PolicyVersion.Enabled)).Returns(() =>
            ValueTask.FromResult<IReadOnlyCollection<RateLimitPolicy>>([.. enabledPolicies.Where(static policy => policy.IsEnabled)]));

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
            IsEnabled = true,
            EnabledUtc = DateTime.UtcNow,
            Scope = RateLimitPolicyScope.Global,
            Limiters = [limiter],
        };
    }

    private static RateLimitPolicy CreateEndpointSlidingWindowPolicy(string name, string path, int permitLimit, int windowSeconds, int segmentsPerWindow)
    {
        var limiter = new RateLimitLimiter
        {
            Id = IdGenerator.GenerateId(),
            Source = SlidingWindowRateLimiterSource.SourceName,
        };

        limiter.Put(new SlidingWindowRateLimiterData
        {
            PermitLimit = permitLimit,
            QueueLimit = 0,
            WindowSeconds = windowSeconds,
            SegmentsPerWindow = segmentsPerWindow,
        });

        return CreateEndpointPolicy(name, path, limiter);
    }

    private static RateLimitPolicy CreateEndpointTokenBucketPolicy(string name, string path, int tokenLimit, int tokensPerPeriod, int replenishmentPeriodSeconds)
    {
        var limiter = new RateLimitLimiter
        {
            Id = IdGenerator.GenerateId(),
            Source = TokenBucketRateLimiterSource.SourceName,
        };

        limiter.Put(new TokenBucketRateLimiterData
        {
            TokenLimit = tokenLimit,
            QueueLimit = 0,
            TokensPerPeriod = tokensPerPeriod,
            ReplenishmentPeriodSeconds = replenishmentPeriodSeconds,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
        });

        return CreateEndpointPolicy(name, path, limiter);
    }

    private static RateLimitPolicy CreateEndpointConcurrencyPolicy(string name, string path, int permitLimit)
    {
        var limiter = new RateLimitLimiter
        {
            Id = IdGenerator.GenerateId(),
            Source = ConcurrencyRateLimiterSource.SourceName,
        };

        limiter.Put(new ConcurrencyRateLimiterData
        {
            PermitLimit = permitLimit,
            QueueLimit = 0,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
        });

        return CreateEndpointPolicy(name, path, limiter);
    }

    private static RateLimitPolicy CreateEndpointPolicy(string name, string path, RateLimitLimiter limiter)
    {
        return new RateLimitPolicy
        {
            Name = name,
            IsEnabled = true,
            EnabledUtc = DateTime.UtcNow,
            Scope = RateLimitPolicyScope.Endpoint,
            Path = path,
            Limiters = [limiter],
        };
    }

    private static DefaultHttpContext CreateHttpContext(string routeName, string httpMethod, string path = "/", params object[] metadata)
    {
        var context = new DefaultHttpContext();
        context.Request.Method = httpMethod;
        context.Request.Path = path;
        context.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");

        var allMetadata = new List<object>(metadata ?? [])
        {
            new RouteNameMetadata(routeName),
        };

        context.SetEndpoint(new Endpoint(
            _ => Task.CompletedTask,
            new EndpointMetadataCollection([.. allMetadata]),
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
