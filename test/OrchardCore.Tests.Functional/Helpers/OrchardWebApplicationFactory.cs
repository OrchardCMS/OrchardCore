using System.Collections.Concurrent;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OrchardCore.Tests.Functional.Helpers;

public sealed class OrchardWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>
    where TProgram : class
{
    private readonly string _appDataPath;
    private readonly ConcurrentBag<LogEntry> _logEntries = [];

    public string ServerAddress { get; private set; } = string.Empty;

    public IReadOnlyCollection<LogEntry> LogEntries => _logEntries;

    public OrchardWebApplicationFactory(string appDataPath)
    {
        _appDataPath = appDataPath;
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

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ORCHARD_APP_DATA", _appDataPath);

        builder.ConfigureLogging(logging =>
        {
            logging.AddProvider(new InMemoryLoggerProvider(_logEntries));
        });

        // Forward database configuration from environment variables if present.
        var connectionString = System.Environment.GetEnvironmentVariable("OrchardCore__ConnectionString");
        if (!string.IsNullOrEmpty(connectionString))
        {
            builder.UseSetting("OrchardCore:ConnectionString", connectionString);
        }

        var databaseProvider = System.Environment.GetEnvironmentVariable("OrchardCore__DatabaseProvider");
        if (!string.IsNullOrEmpty(databaseProvider))
        {
            builder.UseSetting("OrchardCore:DatabaseProvider", databaseProvider);
        }
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        // Replace TestServer with Kestrel so Playwright can connect via real HTTP.
        builder.ConfigureWebHost(webHostBuilder =>
        {
            webHostBuilder.UseKestrel();
            webHostBuilder.UseUrls("http://127.0.0.1:0");
        });

        var host = builder.Build();
        host.Start();

        var server = host.Services.GetRequiredService<IServer>();
        var addresses = server.Features.Get<IServerAddressesFeature>();
        ServerAddress = addresses!.Addresses.First();

        return host;
    }
}
