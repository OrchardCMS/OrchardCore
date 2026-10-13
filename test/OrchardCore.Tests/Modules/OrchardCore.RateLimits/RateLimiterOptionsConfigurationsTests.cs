using System.Text.Json.Nodes;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using OrchardCore.Entities;
using OrchardCore.RateLimits;
using OrchardCore.RateLimits.Core;
using OrchardCore.RateLimits.Models;
using OrchardCore.RateLimits.Services;
using OrchardCore.Users.Endpoints.EmailAuthenticator;

namespace OrchardCore.Tests.Modules.OrchardCore.RateLimits;

public class RateLimiterOptionsConfigurationsTests
{
    [Fact]
    public async Task Apply_CustomRouteLimitForMatchingRouteNameAndMethod_Succeeds()
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
    public async Task Only_ApplyCustomRouteLimitToMatchingHttpMethods_Succeeds()
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
    public async Task Apply_CustomRouteLimitToEndpointNameMetadata_Succeeds()
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
    public async Task Apply_CustomGroupLimitToMatchingEndpointGroup_Succeeds()
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
    public async Task Apply_EnabledGlobalPolicy_Succeeds()
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
    public async Task Ignore_DisabledPolicies_Succeeds()
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
    public async Task Apply_EnabledEndpointPolicy_Succeeds()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var policy = CreateGlobalFixedWindowPolicy(OrchardCoreConstants.AuthenticationSchemes.Api, 1, 60);
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
    public async Task Apply_EnabledSlidingWindowEndpointPolicy_Succeeds()
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
    public async Task Apply_EnabledTokenBucketEndpointPolicy_Succeeds()
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
    public async Task Apply_EnabledConcurrencyEndpointPolicy_Succeeds()
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
    public async Task Apply_EnabledGroupPolicyToAnyMatchingGroup_Succeeds()
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
    public async Task Handle_EnabledPoliciesWithStringifiedNumericLimiterValues_Succeeds()
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

    [Fact]
    public async Task Apply_EnabledPoliciesChanged_AppliesNewPoliciesAndKeepsBuiltInLimits()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var rateLimitsOptions = new RateLimitsOptions();
        rateLimitsOptions.AddRouteRateLimit("Login", HttpMethods.Post, RateLimitPartitionHelpers.CreateFixedWindowPerIpPolicy("login", 2, TimeSpan.FromMinutes(1)));

        var policiesOptions = new TestOptionsMonitor<RateLimitPoliciesOptions>(CreatePoliciesOptions([]));
        var options = Configure(rateLimitsOptions, policiesOptions);
        var globalLimiter = options.GlobalLimiter;

        using var firstLoginLease = await globalLimiter.AcquireAsync(CreateHttpContext("Login", HttpMethods.Post), 1, cancellationToken);
        using var firstLease = await globalLimiter.AcquireAsync(CreateHttpContext("AnyRoute", HttpMethods.Get), 1, cancellationToken);
        using var secondLease = await globalLimiter.AcquireAsync(CreateHttpContext("AnyRoute", HttpMethods.Get), 1, cancellationToken);

        Assert.True(firstLoginLease.IsAcquired);
        Assert.True(firstLease.IsAcquired);
        Assert.True(secondLease.IsAcquired);

        policiesOptions.CurrentValue = CreatePoliciesOptions([CreateGlobalFixedWindowPolicy("Global", 1, 60)]);

        using var thirdLease = await globalLimiter.AcquireAsync(CreateHttpContext("AnyRoute", HttpMethods.Get), 1, cancellationToken);
        using var fourthLease = await globalLimiter.AcquireAsync(CreateHttpContext("AnyRoute", HttpMethods.Get), 1, cancellationToken);

        Assert.True(thirdLease.IsAcquired);
        Assert.False(fourthLease.IsAcquired);

        policiesOptions.CurrentValue = CreatePoliciesOptions([]);

        // The built-in route limit keeps its count while the policies change.
        using var secondLoginLease = await globalLimiter.AcquireAsync(CreateHttpContext("Login", HttpMethods.Post), 1, cancellationToken);
        using var thirdLoginLease = await globalLimiter.AcquireAsync(CreateHttpContext("Login", HttpMethods.Post), 1, cancellationToken);
        using var fifthLease = await globalLimiter.AcquireAsync(CreateHttpContext("AnyRoute", HttpMethods.Get), 1, cancellationToken);

        Assert.True(secondLoginLease.IsAcquired);
        Assert.False(thirdLoginLease.IsAcquired);
        Assert.True(fifthLease.IsAcquired);
        Assert.Same(globalLimiter, options.GlobalLimiter);
    }

    private static RateLimiterOptions Configure(
        RateLimitsOptions rateLimitsOptions,
        IEnumerable<RateLimitPolicy> enabledPolicies = null)
        => Configure(rateLimitsOptions, new TestOptionsMonitor<RateLimitPoliciesOptions>(CreatePoliciesOptions(enabledPolicies ?? [])));

    private static RateLimiterOptions Configure(
        RateLimitsOptions rateLimitsOptions,
        IOptionsMonitor<RateLimitPoliciesOptions> policiesOptions)
    {
        var options = new RateLimiterOptions();

        // The service provider is kept, as the limiters of the policies are built when they are first used.
        new RateLimiterOptionsConfigurations(
            Options.Create(rateLimitsOptions),
            policiesOptions,
            CreateServiceProvider()).Configure(options);

        return options;
    }

    private static RateLimitPoliciesOptions CreatePoliciesOptions(IEnumerable<RateLimitPolicy> enabledPolicies)
        => new()
        {
            EnabledPolicies = [.. enabledPolicies.Where(static policy => policy.IsEnabled)],
        };

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

    private sealed class TestOptionsMonitor<TOptions> : IOptionsMonitor<TOptions>
    {
        public TestOptionsMonitor(TOptions currentValue)
        {
            CurrentValue = currentValue;
        }

        public TOptions CurrentValue { get; set; }

        public TOptions Get(string name)
            => CurrentValue;

        public IDisposable OnChange(Action<TOptions, string> listener)
            => null;
    }
}
