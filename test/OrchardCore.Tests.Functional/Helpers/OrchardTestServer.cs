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

public sealed class OrchardTestServer : IAsyncDisposable
{
    // Serializes builder creation because env vars (ORCHARD_APP_DATA, OrchardCore__ConnectionString)
    // are process-wide and must not race between parallel fixtures.
    private static readonly SemaphoreSlim _builderLock = new(1, 1);

    // Capture the original connection string before any fixture modifies the env var.
    private static readonly string _originalConnectionString =
        System.Environment.GetEnvironmentVariable("OrchardCore__ConnectionString");

    private static readonly string _originalDatabaseProvider =
        System.Environment.GetEnvironmentVariable("OrchardCore__DatabaseProvider");

    private readonly WebApplication _app;
    private readonly ConcurrentBag<LogEntry> _logEntries;

    public string ServerAddress { get; }

    private OrchardTestServer(WebApplication app, string serverAddress, ConcurrentBag<LogEntry> logEntries)
    {
        _app = app;
        ServerAddress = serverAddress;
        _logEntries = logEntries;
    }

    public static async Task<OrchardTestServer> StartCmsAsync(string contentRoot, string appDataPath, string instanceId = null)
    {
        var logEntries = new ConcurrentBag<LogEntry>();

        // Lock around builder creation to safely set process-wide env vars.
        await _builderLock.WaitAsync();
        WebApplication app;
        try
        {
            SetupEnvironment(appDataPath, instanceId);

            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                ApplicationName = "OrchardCore.Cms.Web",
                ContentRootPath = contentRoot,
            });

            builder.Host.UseNLogHost();

            builder.Services
                .AddOrchardCms()
                .AddSetupFeatures("OrchardCore.AutoSetup");

            ConfigureServices(builder, appDataPath, logEntries);

            app = builder.Build();
        }
        finally
        {
            _builderLock.Release();
        }

        app.UseStaticFiles();
        app.UseOrchardCore();

        await app.StartAsync();

        return new OrchardTestServer(app, GetListeningAddress(app), logEntries);
    }

    public static async Task<OrchardTestServer> StartMvcAsync(string contentRoot, string appDataPath)
    {
        var logEntries = new ConcurrentBag<LogEntry>();

        await _builderLock.WaitAsync();
        WebApplication app;
        try
        {
            SetupEnvironment(appDataPath, null);

            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                ApplicationName = "OrchardCore.Mvc.Web",
                ContentRootPath = contentRoot,
            });

            builder.Services
                .AddOrchardCore()
                .AddMvc();

            ConfigureServices(builder, appDataPath, logEntries);

            app = builder.Build();
        }
        finally
        {
            _builderLock.Release();
        }

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

    public async ValueTask DisposeAsync()
    {
        if (_app is not null)
        {
            await _app.StopAsync();
            await _app.DisposeAsync();
        }
    }

    /// <summary>
    /// Sets process-wide environment variables before the builder captures them.
    /// Must be called under <see cref="_builderLock"/>.
    /// </summary>
    private static void SetupEnvironment(string appDataPath, string instanceId)
    {
        System.Environment.SetEnvironmentVariable("ORCHARD_APP_DATA", appDataPath);

        // When a shared database server is configured, give each fixture its own database.
        // Always compute from the original connection string, not the current env var
        // (which may have been modified by a previous fixture).
        if (!string.IsNullOrEmpty(_originalConnectionString) && !string.IsNullOrEmpty(_originalDatabaseProvider) && !string.IsNullOrEmpty(instanceId))
        {
            var fixtureConnectionString = AppendDatabaseSuffix(_originalConnectionString, $"_{instanceId}");
            EnsureDatabaseExists(fixtureConnectionString, _originalDatabaseProvider);

            // ShellSettingsManager gives env vars highest priority, so we must set
            // the env var itself — builder.Configuration overrides get ignored.
            System.Environment.SetEnvironmentVariable("OrchardCore__ConnectionString", fixtureConnectionString);
        }
        else
        {
            // Restore the original connection string for fixtures without an instanceId.
            System.Environment.SetEnvironmentVariable("OrchardCore__ConnectionString", _originalConnectionString);
        }
    }

    private static void ConfigureServices(WebApplicationBuilder builder, string appDataPath, ConcurrentBag<LogEntry> logEntries)
    {
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.WebHost.UseSetting("suppressHostingStartup", "true");

        // PostConfigure overrides whatever ShellOptionsSetup set from the env var,
        // ensuring each instance uses its own app data path.
        var resolvedAppDataPath = Path.IsPathRooted(appDataPath)
            ? appDataPath
            : Path.Combine(builder.Environment.ContentRootPath, appDataPath);

        builder.Services.PostConfigure<ShellOptions>(options =>
        {
            options.ShellsApplicationDataPath = resolvedAppDataPath;
        });

        builder.Logging.AddProvider(new InMemoryLoggerProvider(logEntries));
    }

    private static void EnsureDatabaseExists(string connectionString, string databaseProvider)
    {
        var dbName = ExtractDatabaseName(connectionString);
        if (dbName is null)
        {
            return;
        }

        // Connect to the server's default/admin database.
        var serverConnectionString = ReplaceDatabaseName(connectionString, databaseProvider switch
        {
            "Postgres" => "postgres",
            "MySql" => "mysql",
            "SqlConnection" => "master",
            _ => null,
        });

        if (serverConnectionString is null)
        {
            return;
        }

        using var connection = CreateConnection(serverConnectionString, databaseProvider);
        if (connection is null)
        {
            return;
        }

        connection.Open();
        using var command = connection.CreateCommand();

        command.CommandText = databaseProvider switch
        {
            "Postgres" => $"SELECT 1 FROM pg_database WHERE datname = '{dbName}'",
            "MySql" => $"SELECT 1 FROM information_schema.schemata WHERE schema_name = '{dbName}'",
            "SqlConnection" => $"SELECT 1 FROM sys.databases WHERE name = '{dbName}'",
            _ => null,
        };

        if (command.CommandText is null)
        {
            return;
        }

        var exists = command.ExecuteScalar() is not null;
        if (!exists)
        {
            using var createCommand = connection.CreateCommand();
            createCommand.CommandText = databaseProvider switch
            {
                "Postgres" => $"CREATE DATABASE \"{dbName}\"",
                "MySql" => $"CREATE DATABASE `{dbName}`",
                "SqlConnection" => $"CREATE DATABASE [{dbName}]",
                _ => null,
            };

            createCommand.ExecuteNonQuery();
        }
    }

    private static System.Data.Common.DbConnection CreateConnection(string connectionString, string databaseProvider)
    {
        return databaseProvider switch
        {
            "Postgres" => new Npgsql.NpgsqlConnection(connectionString),
            "MySql" => new MySqlConnector.MySqlConnection(connectionString),
            "SqlConnection" => new global::Microsoft.Data.SqlClient.SqlConnection(connectionString),
            _ => null,
        };
    }

    private static string ExtractDatabaseName(string connectionString)
    {
        var match = System.Text.RegularExpressions.Regex.Match(
            connectionString, @"(?:Database|database)\s*=\s*([^;]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    private static string ReplaceDatabaseName(string connectionString, string newDbName)
    {
        if (newDbName is null)
        {
            return null;
        }

        return System.Text.RegularExpressions.Regex.Replace(
            connectionString, @"((?:Database|database)\s*=\s*)[^;]+", $"${{1}}{newDbName}",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }

    private static string AppendDatabaseSuffix(string connectionString, string suffix)
    {
        var dbName = ExtractDatabaseName(connectionString);
        return dbName is not null ? ReplaceDatabaseName(connectionString, dbName + suffix) : connectionString;
    }

    private static string GetListeningAddress(WebApplication app)
    {
        var server = app.Services.GetRequiredService<IServer>();
        var addresses = server.Features.Get<IServerAddressesFeature>();
        return addresses!.Addresses.First();
    }
}
