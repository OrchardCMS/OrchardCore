using System.Collections.Concurrent;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Logging;

namespace OrchardCore.Tests.Functional.Helpers;

public sealed class OrchardTestServer : IDisposable
{
    private readonly WebApplication _app;
    private readonly ConcurrentBag<LogEntry> _logEntries;

    public string ServerAddress { get; }

    private OrchardTestServer(WebApplication app, string serverAddress, ConcurrentBag<LogEntry> logEntries)
    {
        _app = app;
        ServerAddress = serverAddress;
        _logEntries = logEntries;
    }

    public static async Task<OrchardTestServer> StartCmsAsync(string contentRoot, string appDataPath)
    {
        var logEntries = new ConcurrentBag<LogEntry>();

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ApplicationName = "OrchardCore.Cms.Web",
            ContentRootPath = contentRoot,
        });

        builder.Host.UseNLogHost();

        builder.Services
            .AddOrchardCms()
            .AddSetupFeatures("OrchardCore.AutoSetup");

        ConfigureCommon(builder, appDataPath, logEntries);

        var app = builder.Build();
        app.UseStaticFiles();
        app.UseOrchardCore();

        await app.StartAsync();

        return new OrchardTestServer(app, GetListeningAddress(app), logEntries);
    }

    public static async Task<OrchardTestServer> StartMvcAsync(string contentRoot, string appDataPath)
    {
        var logEntries = new ConcurrentBag<LogEntry>();

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ApplicationName = "OrchardCore.Mvc.Web",
            ContentRootPath = contentRoot,
        });

        builder.Services
            .AddOrchardCore()
            .AddMvc();

        ConfigureCommon(builder, appDataPath, logEntries);

        var app = builder.Build();
        app.UseStaticFiles();
        app.UseOrchardCore();

        await app.StartAsync();

        return new OrchardTestServer(app, GetListeningAddress(app), logEntries);
    }

    public void AssertNoLoggedErrors()
    {
        var errors = _logEntries
            .Where(e => e.LogLevel >= LogLevel.Error)
            .ToList();

        if (errors.Count > 0)
        {
            var messages = string.Join(
                System.Environment.NewLine,
                errors.Select(e => $"[{e.LogLevel}] {e.Category}: {e.Message}{(e.Exception is not null ? $" -> {e.Exception}" : string.Empty)}"));

            throw new Xunit.Sdk.XunitException(
                $"Expected no logged errors, but found {errors.Count}:{System.Environment.NewLine}{messages}");
        }
    }

    public void Dispose()
    {
        _app?.DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    private static void ConfigureCommon(WebApplicationBuilder builder, string appDataPath, ConcurrentBag<LogEntry> logEntries)
    {
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.WebHost.UseSetting("suppressHostingStartup", "true");

        // OrchardCore reads ORCHARD_APP_DATA from System.Environment for NLog path setup.
        System.Environment.SetEnvironmentVariable("ORCHARD_APP_DATA", appDataPath);

        // ShellOptionsSetup also reads the env var, but it's process-wide and races under parallel execution.
        // PostConfigure overrides whatever ShellOptionsSetup set, ensuring each instance uses its own path.
        var resolvedAppDataPath = Path.IsPathRooted(appDataPath)
            ? appDataPath
            : Path.Combine(builder.Environment.ContentRootPath, appDataPath);

        builder.Services.PostConfigure<ShellOptions>(options =>
        {
            options.ShellsApplicationDataPath = resolvedAppDataPath;
        });

        // Forward database configuration from environment variables if present.
        var connectionString = System.Environment.GetEnvironmentVariable("OrchardCore__ConnectionString");
        if (!string.IsNullOrEmpty(connectionString))
        {
            builder.Configuration["OrchardCore:ConnectionString"] = connectionString;
        }

        var databaseProvider = System.Environment.GetEnvironmentVariable("OrchardCore__DatabaseProvider");
        if (!string.IsNullOrEmpty(databaseProvider))
        {
            builder.Configuration["OrchardCore:DatabaseProvider"] = databaseProvider;
        }

        builder.Logging.AddProvider(new InMemoryLoggerProvider(logEntries));
    }

    private static string GetListeningAddress(WebApplication app)
    {
        var server = app.Services.GetRequiredService<IServer>();
        var addresses = server.Features.Get<IServerAddressesFeature>();
        return addresses!.Addresses.First();
    }
}
