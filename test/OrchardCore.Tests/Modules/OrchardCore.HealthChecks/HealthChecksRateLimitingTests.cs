using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Entities;
using OrchardCore.Modules;
using OrchardCore.RateLimits;
using OrchardCore.RateLimits.Core;
using OrchardCore.RateLimits.Models;
using OrchardCore.RateLimits.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.HealthChecks;

public class HealthChecksRateLimitingTests
{
    [Fact]
    public async Task ShouldApplyEndpointSpecificPoliciesToHealthChecks()
    {
        using var host = await CreateHealthChecksHostAsync(
            [
                CreateEndpointSlidingWindowPolicy("LiveHealth", "/health/live", 10, 59, 1),
            ],
            TestContext.Current.CancellationToken);

        var client = host.GetTestClient();

        for (var i = 0; i < 10; i++)
        {
            var response = await client.GetAsync("/health/live", TestContext.Current.CancellationToken);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        var rejectedResponse = await client.GetAsync("/health/live", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.TooManyRequests, rejectedResponse.StatusCode);
    }

    private static async Task<IHost> CreateHealthChecksHostAsync(
        IEnumerable<RateLimitPolicy> enabledPolicies,
        CancellationToken cancellationToken)
    {
        var shellConfiguration = new ShellConfiguration(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["OrchardCore_HealthChecks:Url"] = "/health/live",
            })
            .Build());

        var healthChecksStartup = new global::OrchardCore.HealthChecks.Startup(shellConfiguration);
        var builder = Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseTestServer();
                webBuilder.ConfigureServices(services =>
                {
                    services.AddRouting();
                    services.AddRateLimiter();
                    services.AddTransient<IConfigureOptions<RateLimiterOptions>, RateLimiterOptionsConfigurations>();
                    services.AddSingleton(CreatePolicyStore(enabledPolicies ?? []));
                    services.AddSingleton(new FixedWindowRateLimiterSource(Mock.Of<IStringLocalizer<FixedWindowRateLimiterSource>>()));
                    services.AddSingleton(new SlidingWindowRateLimiterSource(Mock.Of<IStringLocalizer<SlidingWindowRateLimiterSource>>()));
                    services.AddSingleton(new ConcurrencyRateLimiterSource(Mock.Of<IStringLocalizer<ConcurrencyRateLimiterSource>>()));
                    services.AddSingleton(new TokenBucketRateLimiterSource(Mock.Of<IStringLocalizer<TokenBucketRateLimiterSource>>()));
                    services.AddKeyedSingleton<IRateLimiterSource>(FixedWindowRateLimiterSource.SourceName, static (sp, _) => sp.GetRequiredService<FixedWindowRateLimiterSource>());
                    services.AddKeyedSingleton<IRateLimiterSource>(SlidingWindowRateLimiterSource.SourceName, static (sp, _) => sp.GetRequiredService<SlidingWindowRateLimiterSource>());
                    services.AddKeyedSingleton<IRateLimiterSource>(ConcurrencyRateLimiterSource.SourceName, static (sp, _) => sp.GetRequiredService<ConcurrencyRateLimiterSource>());
                    services.AddKeyedSingleton<IRateLimiterSource>(TokenBucketRateLimiterSource.SourceName, static (sp, _) => sp.GetRequiredService<TokenBucketRateLimiterSource>());

                    healthChecksStartup.ConfigureServices(services);
                });

                webBuilder.Configure(app =>
                {
                    app.UseRouting();

                    var routes = (IEndpointRouteBuilder)app.Properties["__EndpointRouteBuilder"];
                    healthChecksStartup.Configure(app, routes, app.ApplicationServices);

                    app.UseRateLimiter();
                    app.UseEndpoints(_ => { });
                });
            });

        return await builder.StartAsync(cancellationToken);
    }

    private static IRateLimitPolicyStore CreatePolicyStore(IEnumerable<RateLimitPolicy> enabledPolicies)
    {
        var store = new Mock<IRateLimitPolicyStore>();
        store.Setup(x => x.GetAllAsync(PolicyVersion.Enabled)).Returns(() =>
            ValueTask.FromResult<IReadOnlyCollection<RateLimitPolicy>>([.. enabledPolicies.Where(static policy => policy.IsEnabled)]));

        return store.Object;
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
}
