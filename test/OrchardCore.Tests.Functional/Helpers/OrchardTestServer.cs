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
    private readonly WebApplication _app;
    private readonly ConcurrentBag<LogEntry> _logEntries;

    public string ServerAddress { get; }

    private OrchardTestServer(WebApplication app, string serverAddress, ConcurrentBag<LogEntry> logEntries)
    {
        _app = app;
        ServerAddress = serverAddress;
        _logEntries = logEntries;
    }

    public static async Task<OrchardTestServer> StartCmsAsync(string contentRoot, string appDataPath, string tablePrefix = null)
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

        ConfigureCommon(builder, appDataPath, tablePrefix, logEntries);

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

        ConfigureCommon(builder, appDataPath, null, logEntries);

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

    public async ValueTask DisposeAsync()
    {
        if (_app is not null)
        {
            await _app.StopAsync();
            await _app.DisposeAsync();
        }
    }

    private static void ConfigureCommon(WebApplicationBuilder builder, string appDataPath, string tablePrefix, ConcurrentBag<LogEntry> logEntries)
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
        var databaseProvider = System.Environment.GetEnvironmentVariable("OrchardCore__DatabaseProvider");

        if (!string.IsNullOrEmpty(connectionString) && !string.IsNullOrEmpty(databaseProvider))
        {
            // Give each fixture its own database to enable parallel execution.
            if (!string.IsNullOrEmpty(tablePrefix))
            {
                connectionString = AppendDatabaseSuffix(connectionString, $"_{tablePrefix}");
                EnsureDatabaseExists(connectionString, databaseProvider);
            }

            builder.Configuration["OrchardCore:ConnectionString"] = connectionString;
            builder.Configuration["OrchardCore:DatabaseProvider"] = databaseProvider;
        }

        builder.Logging.AddProvider(new InMemoryLoggerProvider(logEntries));
    }

    private static void EnsureDatabaseExists(string connectionString, string databaseProvider)
    {
        var pattern = @"((?:Database|database)\s*=\s*)([^;]+)";
        var match = System.Text.RegularExpressions.Regex.Match(connectionString, pattern);

        if (!match.Success)
        {
            return;
        }

        var dbName = match.Groups[2].Value.Trim();

        // Build a connection string pointing to the server's default database.
        var serverConnectionString = databaseProvider switch
        {
            "Postgres" => System.Text.RegularExpressions.Regex.Replace(connectionString, pattern, "${1}postgres"),
            "MySql" => System.Text.RegularExpressions.Regex.Replace(connectionString, pattern, "${1}mysql"),
            "SqlConnection" => System.Text.RegularExpressions.Regex.Replace(connectionString, pattern, "${1}master"),
            _ => null,
        };

        if (serverConnectionString is null)
        {
            return;
        }

        var createSql = databaseProvider switch
        {
            "SqlConnection" => $"IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = '{dbName}') CREATE DATABASE [{dbName}]",
            _ => $"CREATE DATABASE IF NOT EXISTS \"{dbName}\"",
        };

        // MySQL uses backticks, not double quotes.
        if (databaseProvider == "MySql")
        {
            createSql = $"CREATE DATABASE IF NOT EXISTS `{dbName}`";
        }

        using var connection = databaseProvider switch
        {
            "Postgres" => (System.Data.Common.DbConnection)new Npgsql.NpgsqlConnection(serverConnectionString),
            "MySql" => new MySqlConnector.MySqlConnection(serverConnectionString),
            "SqlConnection" => new global::Microsoft.Data.SqlClient.SqlConnection(serverConnectionString),
            _ => null,
        };

        if (connection is null)
        {
            return;
        }

        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = createSql;
        command.ExecuteNonQuery();
    }

    private static string AppendDatabaseSuffix(string connectionString, string suffix)
    {
        // Matches "Database=xxx" or "database=xxx" in connection strings (Postgres, MySQL, SQL Server).
        var pattern = @"((?:Database|database)\s*=\s*)([^;]+)";
        var match = System.Text.RegularExpressions.Regex.Match(connectionString, pattern);

        if (match.Success)
        {
            var dbName = match.Groups[2].Value.Trim() + suffix;
            return connectionString[..match.Groups[2].Index] + dbName + connectionString[(match.Groups[2].Index + match.Groups[2].Length)..];
        }

        return connectionString;
    }

    private static string GetListeningAddress(WebApplication app)
    {
        var server = app.Services.GetRequiredService<IServer>();
        var addresses = server.Features.Get<IServerAddressesFeature>();
        return addresses!.Addresses.First();
    }
}
