using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using OrchardCore.Environment.Shell;
using OrchardCore.Logging;

namespace OrchardCore.Tests.Functional.Helpers;

public sealed class OrchardTestServer : IAsyncDisposable
{
    // Capture the original connection string before any fixture modifies the env var.
    private static readonly string _originalConnectionString =
        System.Environment.GetEnvironmentVariable("OrchardCore__ConnectionString");

    private static readonly string _originalDatabaseProvider =
        System.Environment.GetEnvironmentVariable("OrchardCore__DatabaseProvider");

    private readonly WebApplication _app;
    private readonly FakeLogCollector _logCollector;

    public string ServerAddress { get; }

    private OrchardTestServer(WebApplication app, string serverAddress, FakeLogCollector logCollector)
    {
        _app = app;
        ServerAddress = serverAddress;
        _logCollector = logCollector;
    }

    public static async Task<OrchardTestServer> StartCmsAsync(string contentRoot, string appDataPath, string instanceId = null)
    {
        var loggerProvider = new FakeLoggerProvider();

        SetupAppData(appDataPath, instanceId);

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ApplicationName = "OrchardCore.Cms.Web",
            ContentRootPath = contentRoot,
        });

        builder.Host.UseNLogHost();

        builder.Services
            .AddOrchardCms()
            .AddSetupFeatures("OrchardCore.AutoSetup");

        ConfigureServices(builder, appDataPath, loggerProvider);

        var app = builder.Build();
        app.UseStaticFiles();
        app.UseOrchardCore();

        await app.StartAsync();

        return new OrchardTestServer(app, GetListeningAddress(app), loggerProvider.Collector);
    }

    public static async Task<OrchardTestServer> StartMvcAsync(string contentRoot, string appDataPath)
    {
        var loggerProvider = new FakeLoggerProvider();

        SetupAppData(appDataPath, null);

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ApplicationName = "OrchardCore.Mvc.Web",
            ContentRootPath = contentRoot,
        });

        builder.Services
            .AddOrchardCore()
            .AddMvc();

        ConfigureServices(builder, appDataPath, loggerProvider);

        var app = builder.Build();
        app.UseStaticFiles();
        app.UseOrchardCore();

        await app.StartAsync();

        return new OrchardTestServer(app, GetListeningAddress(app), loggerProvider.Collector);
    }

    public void AssertNoLoggedIssues()
    {
        var issues = _logCollector.GetSnapshot()
            .Where(e => e.Level >= LogLevel.Warning)
            .Where(e => !IsIgnoredWarning(e))
            .ToList();

        if (issues.Count > 0)
        {
            var messages = string.Join(
                System.Environment.NewLine,
                issues.Select(e => $"[{e.Level}] {e.Category}: {e.Message}{(e.Exception is not null ? $" -> {e.Exception}" : string.Empty)}"));

            throw new Xunit.Sdk.XunitException(
                $"Expected no logged warnings or errors, but found {issues.Count}:{System.Environment.NewLine}{messages}");
        }
    }

    private static bool IsIgnoredWarning(FakeLogRecord record) =>
        record.Category == "Microsoft.AspNetCore.DataProtection.KeyManagement.XmlKeyManager"
        && record.Message.Contains("No XML encryptor configured");

    public async ValueTask DisposeAsync()
    {
        if (_app is not null)
        {
            await _app.StopAsync();
            await _app.DisposeAsync();
        }
    }

    /// <summary>
    /// Prepares the app data directory. When a shared database server is configured via
    /// environment variables, creates a fixture-specific database and REMOVES the env vars
    /// so ShellSettingsManager can't override our per-fixture configuration.
    /// The connection string is written to tenants.json instead.
    /// Safe because tests run serially (maxParallelThreads: 1).
    /// </summary>
    private static void SetupAppData(string appDataPath, string instanceId)
    {
        System.Environment.SetEnvironmentVariable("ORCHARD_APP_DATA", appDataPath);

        if (string.IsNullOrEmpty(_originalConnectionString) || string.IsNullOrEmpty(_originalDatabaseProvider))
        {
            return;
        }

        // Remove the env vars so ShellSettingsManager can't re-add them with highest priority.
        System.Environment.SetEnvironmentVariable("OrchardCore__ConnectionString", null);
        System.Environment.SetEnvironmentVariable("OrchardCore__DatabaseProvider", null);

        var connectionString = _originalConnectionString;

        if (!string.IsNullOrEmpty(instanceId))
        {
            var dbName = ExtractDatabaseName(_originalConnectionString);
            if (dbName is not null)
            {
                connectionString = ReplaceDatabaseName(_originalConnectionString, $"{dbName}_{instanceId}");
                EnsureDatabaseExists(connectionString, _originalDatabaseProvider);
            }
        }

        // Write tenants.json with the fixture-specific connection string.
        // With env vars removed, this is the sole source of database configuration.
        Directory.CreateDirectory(appDataPath);
        File.WriteAllText(
            Path.Combine(appDataPath, "tenants.json"),
            JsonSerializer.Serialize(new Dictionary<string, object>
            {
                ["Default"] = new Dictionary<string, string>
                {
                    ["DatabaseProvider"] = _originalDatabaseProvider,
                    ["ConnectionString"] = connectionString,
                },
            }));
    }

    private static void ConfigureServices(WebApplicationBuilder builder, string appDataPath, FakeLoggerProvider loggerProvider)
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

        // Disable YesSql concurrency checks during setup. Each recipe step runs in a new
        // scope with a new session, but the document cache can serve stale versions, causing
        // ConcurrencyException on SiteSettings with external databases.
        builder.Configuration["OrchardCore:OrchardCore_Documents:CheckConcurrency"] = "false";

        builder.Logging.AddFilter<FakeLoggerProvider>(level => level >= LogLevel.Warning);
        builder.Logging.AddProvider(loggerProvider);
    }

    /// <summary>
    /// Creates a fixture-specific database if it doesn't already exist. The CI workflow
    /// creates a single shared service database (e.g., "app"), but each test fixture uses
    /// its own database (e.g., "app_cmssetupfixture") to avoid cross-fixture interference.
    /// </summary>
    private static void EnsureDatabaseExists(string connectionString, string databaseProvider)
    {
        var dbName = ExtractDatabaseName(connectionString);
        if (dbName is null)
        {
            return;
        }

        ValidateDatabaseName(dbName);

        var adminDb = databaseProvider switch
        {
            "Postgres" => "postgres",
            "MySql" => "mysql",
            "SqlConnection" => "master",
            _ => null,
        };

        var serverConnectionString = ReplaceDatabaseName(connectionString, adminDb);
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

        using var checkCommand = connection.CreateCommand();
        var param = checkCommand.CreateParameter();
        param.ParameterName = "@dbName";
        param.Value = dbName;
        checkCommand.Parameters.Add(param);

        checkCommand.CommandText = databaseProvider switch
        {
            "Postgres" => "SELECT 1 FROM pg_database WHERE datname = @dbName",
            "MySql" => "SELECT 1 FROM information_schema.schemata WHERE schema_name = @dbName",
            "SqlConnection" => "SELECT 1 FROM sys.databases WHERE name = @dbName",
            _ => null,
        };

        if (checkCommand.CommandText is null || checkCommand.ExecuteScalar() is not null)
        {
            return;
        }

        // CREATE DATABASE is DDL and cannot be parameterized; quote per provider.
        using var createCommand = connection.CreateCommand();
        createCommand.CommandText = databaseProvider switch
        {
            "Postgres" => $"CREATE DATABASE \"{dbName.Replace("\"", "\"\"")}\"",
            "MySql" => $"CREATE DATABASE `{dbName.Replace("`", "``")}`",
            "SqlConnection" => $"CREATE DATABASE [{dbName.Replace("]", "]]")}]",
            _ => null,
        };

        createCommand.ExecuteNonQuery();
    }

    private static void ValidateDatabaseName(string dbName)
    {
        if (!System.Text.RegularExpressions.Regex.IsMatch(dbName, @"^[a-zA-Z0-9_\-]+$"))
        {
            throw new ArgumentException(
                $"Database name '{dbName}' contains invalid characters. Only alphanumeric, underscore, and hyphen are allowed.");
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
            connectionString, @"(?:Database|database)\s*=\s*([^;]+)",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
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

    private static string GetListeningAddress(WebApplication app)
    {
        var server = app.Services.GetRequiredService<IServer>();
        var addresses = server.Features.Get<IServerAddressesFeature>();
        return addresses!.Addresses.First();
    }
}
