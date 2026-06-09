using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OrchardCore.Entities;
using OrchardCore.RateLimits;
using OrchardCore.RateLimits.Settings;
using OrchardCore.Settings;

namespace OrchardCore.Tests.Modules.OrchardCore.RateLimits;

public class RateLimiterMiddlewareTests
{
    [Fact]
    public async Task ShouldNotCountStaticAssetsAgainstGlobalRateLimit()
    {
        using var staticAssetDirectory = new StaticAssetDirectory();
        using var host = await CreateHostAsync(
            staticAssetDirectory.Path,
            globalOptions: new GlobalRateLimitOptions
            {
                PermitLimit = 1,
                Window = TimeSpan.FromMinutes(1),
            },
            configureRouteLimits: null,
            rateLimitsSettings: null,
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
    public async Task ShouldApplyShortRouteLimitToNamedTenantRoute()
    {
        using var staticAssetDirectory = new StaticAssetDirectory();
        using var host = await CreateHostAsync(
            staticAssetDirectory.Path,
            globalOptions: new GlobalRateLimitOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
            },
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
            rateLimitsSettings: null,
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
    public async Task ShouldApplyRouteSpecificLimitsWithoutGlobalLimiterWhenDisabledInSiteSettings()
    {
        using var staticAssetDirectory = new StaticAssetDirectory();
        using var host = await CreateHostAsync(
            staticAssetDirectory.Path,
            globalOptions: new GlobalRateLimitOptions
            {
                PermitLimit = 1,
                Window = TimeSpan.FromMinutes(1),
            },
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
            rateLimitsSettings: new RateLimitsSettings
            {
                EnableGlobalRateLimiter = false,
            },
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

    private static async Task<IHost> CreateHostAsync(
        string staticAssetPath,
        GlobalRateLimitOptions globalOptions,
        Action<RateLimitsOptions> configureRouteLimits,
        RateLimitsSettings rateLimitsSettings,
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
                    services.AddSingleton<ISiteService>(CreateSiteService(rateLimitsSettings));
                    services.Configure<GlobalRateLimitOptions>(options =>
                    {
                        options.PermitLimit = globalOptions.PermitLimit;
                        options.Window = globalOptions.Window;
                        options.QueueLimit = globalOptions.QueueLimit;
                    });

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
                        endpoints.MapGet("/tenant/other", () => Results.Text("other-tenant-response"))
                            .WithName("OtherTenantRoute");
                    });
                });
            });

        var host = await builder.StartAsync(cancellationToken);
        return host;
    }

    private static ISiteService CreateSiteService(RateLimitsSettings settings)
    {
        var siteSettings = new SiteSettings();

        if (settings != null)
        {
            siteSettings.Put(settings);
        }

        return Mock.Of<ISiteService>(service => service.GetSiteSettingsAsync() == Task.FromResult<ISite>(siteSettings));
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
}
