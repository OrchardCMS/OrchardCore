using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using OrchardCore.Entities;
using OrchardCore.Modules;
using OrchardCore.Modules.FileProviders;
using OrchardCore.RateLimits;
using OrchardCore.RateLimits.Core;
using OrchardCore.RateLimits.Models;
using OrchardCore.RateLimits.Services;
using OrchardCore.Seo;
using OrchardCore.Seo.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.RateLimits;

public class RateLimiterMiddlewareTests
{
    [Fact]
    public async Task Not_CountStaticAssetsAgainstGlobalRateLimit_Succeeds()
    {
        using var staticAssetDirectory = new StaticAssetDirectory();
        using var host = await CreateHostAsync(
            staticAssetDirectory.Path,
            configureRouteLimits: null,
            enabledPolicies:
            [
                CreateGlobalFixedWindowPolicy("Global", 1, 60),
            ],
            cancellationToken: TestContext.Current.CancellationToken);

        var client = host.GetTestClient();

        for (var i = 0; i < 3; i++)
        {
            var assetResponse = await client.GetAsync("/asset.txt", TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, assetResponse.StatusCode);
            Assert.Equal("static-asset", await assetResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        }

        var firstTenantResponse = await client.GetAsync("/tenant/limited", TestContext.Current.CancellationToken);
        var secondTenantResponse = await client.GetAsync("/tenant/limited", TestContext.Current.CancellationToken);
        var assetAfterLimitResponse = await client.GetAsync("/asset.txt", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, firstTenantResponse.StatusCode);
        Assert.Equal("tenant-response", await firstTenantResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        Assert.Equal(HttpStatusCode.TooManyRequests, secondTenantResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, assetAfterLimitResponse.StatusCode);
    }

    [Fact]
    public async Task Apply_ShortRouteLimitToNamedTenantRoute_Succeeds()
    {
        using var staticAssetDirectory = new StaticAssetDirectory();
        using var host = await CreateHostAsync(
            staticAssetDirectory.Path,
            configureRouteLimits: options =>
            {
                options.AddRouteRateLimit(
                    "TenantLimitedRoute",
                    HttpMethods.Get,
                    RateLimitPartitionHelpers.CreateFixedWindowPerIpPolicy(
                        "tenant-limited",
                        1,
                        TimeSpan.FromSeconds(5)));
            },
            enabledPolicies:
            [
                CreateGlobalFixedWindowPolicy("Global", 10, 60),
            ],
            cancellationToken: TestContext.Current.CancellationToken);

        var client = host.GetTestClient();

        var firstTenantResponse = await client.GetAsync("/tenant/limited", TestContext.Current.CancellationToken);
        var secondTenantResponse = await client.GetAsync("/tenant/limited", TestContext.Current.CancellationToken);
        var otherTenantResponse = await client.GetAsync("/tenant/other", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, firstTenantResponse.StatusCode);
        Assert.Equal("tenant-response", await firstTenantResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        Assert.Equal(HttpStatusCode.TooManyRequests, secondTenantResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, otherTenantResponse.StatusCode);
        Assert.Equal("other-tenant-response", await otherTenantResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Apply_EndpointSpecificPoliciesWithoutGlobalPolicies_Succeeds()
    {
        using var staticAssetDirectory = new StaticAssetDirectory();
        using var host = await CreateHostAsync(
            staticAssetDirectory.Path,
            configureRouteLimits: null,
            enabledPolicies:
            [
                CreateEndpointFixedWindowPolicy("TenantLimitedPolicy", "/tenant/limited", 1, 60),
            ],
            cancellationToken: TestContext.Current.CancellationToken);

        var client = host.GetTestClient();

        var firstLimitedResponse = await client.GetAsync("/tenant/limited", TestContext.Current.CancellationToken);
        var secondLimitedResponse = await client.GetAsync("/tenant/limited", TestContext.Current.CancellationToken);
        var firstOtherResponse = await client.GetAsync("/tenant/other", TestContext.Current.CancellationToken);
        var secondOtherResponse = await client.GetAsync("/tenant/other", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, firstLimitedResponse.StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, secondLimitedResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, firstOtherResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, secondOtherResponse.StatusCode);
        Assert.Equal("other-tenant-response", await secondOtherResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Apply_GroupSpecificPoliciesToGroupedEndpoints_Succeeds()
    {
        using var staticAssetDirectory = new StaticAssetDirectory();
        using var host = await CreateHostAsync(
            staticAssetDirectory.Path,
            configureRouteLimits: options =>
            {
                options.AddGroupRateLimit(
                    "tenant-authentication",
                    RateLimitPartitionHelpers.CreateFixedWindowPerIpPolicy(
                        "tenant-authentication",
                        1,
                        TimeSpan.FromSeconds(5)));
            },
            enabledPolicies: [],
            cancellationToken: TestContext.Current.CancellationToken);

        var client = host.GetTestClient();

        var firstGroupedResponse = await client.GetAsync("/tenant/grouped", TestContext.Current.CancellationToken);
        var secondGroupedResponse = await client.GetAsync("/tenant/grouped", TestContext.Current.CancellationToken);
        var otherTenantResponse = await client.GetAsync("/tenant/other", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, firstGroupedResponse.StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, secondGroupedResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, otherTenantResponse.StatusCode);
        Assert.Equal("other-tenant-response", await otherTenantResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ShouldApplyEndpointSpecificPoliciesToRobotsTxt()
    {
        using var host = await CreateSeoHostAsync(
            [
                CreateEndpointFixedWindowPolicy("Robots", "/robots.txt", 1, 60),
            ],
            TestContext.Current.CancellationToken);

        var client = host.GetTestClient();

        var firstRobotsResponse = await client.GetAsync("/robots.txt", TestContext.Current.CancellationToken);
        var secondRobotsResponse = await client.GetAsync("/robots.txt", TestContext.Current.CancellationToken);
        var otherResponse = await client.GetAsync("/other", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, firstRobotsResponse.StatusCode);
        Assert.Equal("User-agent: *\nDisallow:\n", (await firstRobotsResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)).Replace("\r\n", "\n"));
        Assert.Equal(HttpStatusCode.TooManyRequests, secondRobotsResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, otherResponse.StatusCode);
        Assert.Equal("other-response", await otherResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    private static async Task<IHost> CreateHostAsync(
        string staticAssetPath,
        Action<RateLimitsOptions> configureRouteLimits,
        IEnumerable<RateLimitPolicy> enabledPolicies,
        CancellationToken cancellationToken)
    {
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

                    if (configureRouteLimits != null)
                    {
                        services.Configure(configureRouteLimits);
                    }
                });

                webBuilder.Configure(app =>
                {
                    app.UseStaticFiles(new StaticFileOptions
                    {
                        FileProvider = new PhysicalFileProvider(staticAssetPath),
                        ContentTypeProvider = new FileExtensionContentTypeProvider(),
                    });

                    app.UseRouting();
                    app.UseRateLimiter();

                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapGet("/tenant/limited", () => Results.Text("tenant-response"))
                            .WithName("TenantLimitedRoute");
                        endpoints.MapGet("/tenant/grouped", () => Results.Text("tenant-grouped-response"))
                            .WithName("TenantGroupedRoute")
                            .WithRateLimitGroups("tenant-authentication", "tenant-shared");
                        endpoints.MapGet("/tenant/other", () => Results.Text("other-tenant-response"))
                            .WithName("OtherTenantRoute");
                    });
                });
            });

        return await builder.StartAsync(cancellationToken);
    }

    private static async Task<IHost> CreateSeoHostAsync(
        IEnumerable<RateLimitPolicy> enabledPolicies,
        CancellationToken cancellationToken)
    {
        var seoStartup = new global::OrchardCore.Seo.Startup();
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
                    services.AddSingleton<IStaticFileProvider>(new TestStaticFileProvider());
                    services.AddSingleton<IRobotsProvider>(new TestRobotsProvider());
                });

                webBuilder.Configure(app =>
                {
                    app.UseRouting();

                    var routes = (IEndpointRouteBuilder)app.Properties["__EndpointRouteBuilder"];
                    seoStartup.Configure(app, routes, app.ApplicationServices);

                    app.UseRateLimiter();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapGet("/other", () => Results.Text("other-response"));
                    });
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

    private static RateLimitPolicy CreateGlobalFixedWindowPolicy(string name, int permitLimit, int windowSeconds)
    {
        var limiter = CreateFixedWindowLimiter(permitLimit, windowSeconds);

        return new RateLimitPolicy
        {
            Name = name,
            IsEnabled = true,
            EnabledUtc = DateTime.UtcNow,
            Scope = RateLimitPolicyScope.Global,
            Limiters = [limiter],
        };
    }

    private static RateLimitPolicy CreateEndpointFixedWindowPolicy(string name, string path, int permitLimit, int windowSeconds)
    {
        var limiter = CreateFixedWindowLimiter(permitLimit, windowSeconds);

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

    private static RateLimitLimiter CreateFixedWindowLimiter(int permitLimit, int windowSeconds)
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

        return limiter;
    }

    private sealed class StaticAssetDirectory : IDisposable
    {
        public StaticAssetDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());
            Directory.CreateDirectory(Path);
            File.WriteAllText(System.IO.Path.Combine(Path, "asset.txt"), "static-asset");
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }

    private sealed class TestStaticFileProvider : IStaticFileProvider
    {
        public IDirectoryContents GetDirectoryContents(string subpath) => NotFoundDirectoryContents.Singleton;

        public IFileInfo GetFileInfo(string subpath) => new NotFoundFileInfo(subpath);

        public IChangeToken Watch(string filter) => NullChangeToken.Singleton;
    }

    private sealed class TestRobotsProvider : IRobotsProvider
    {
        public Task<string> GetContentAsync() => Task.FromResult("User-agent: *\nDisallow:");
    }
}
