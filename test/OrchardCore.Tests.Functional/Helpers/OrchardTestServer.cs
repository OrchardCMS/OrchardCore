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
using OrchardCore.Recipes.Services;

namespace OrchardCore.Tests.Functional.Helpers;

public sealed class OrchardTestServer : IAsyncDisposable
{
    // Capture the original values once at process start, then clear the env vars so
    // ShellSettingsManager can't re-apply them with highest priority — each host gets
    // its per-fixture connection string via tenants.json and builder.Configuration instead.
    private static readonly string _originalConnectionString = CaptureAndClear("OrchardCore__ConnectionString");
    private static readonly string _originalDatabaseProvider = CaptureAndClear("OrchardCore__DatabaseProvider");

    private static string CaptureAndClear(string variable)
    {
        var value = System.Environment.GetEnvironmentVariable(variable);
        if (!string.IsNullOrEmpty(value))
        {
            System.Environment.SetEnvironmentVariable(variable, null);
        }

        return value;
    }

    /// <summary>
    /// The per-fixture connection string used by the most recently started server.
    /// Used by test helpers (e.g., CreateTenantAsync) to fill in child tenant connection strings.
    /// </summary>
    [ThreadStatic]
    private static string _currentConnectionString;

    public static string CurrentConnectionString => _currentConnectionString;

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

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ApplicationName = "OrchardCore.Cms.Web",
            ContentRootPath = contentRoot,
        });

        builder.Host.UseNLogHost();

        builder.Services
            .AddOrchardCms()
            .AddSetupFeatures("OrchardCore.AutoSetup");

        // Serve test recipes from embedded resources instead of copying files.
        builder.Services.AddScoped<IRecipeHarvester, EmbeddedRecipeHarvester>();

        ConfigureServices(builder, appDataPath, instanceId, loggerProvider);

        var app = builder.Build();
        app.UseStaticFiles();
        app.UseOrchardCore();

        await app.StartAsync();

        return new OrchardTestServer(app, GetListeningAddress(app), loggerProvider.Collector);
    }

    public static async Task<OrchardTestServer> StartMvcAsync(string contentRoot, string appDataPath)
    {
        var loggerProvider = new FakeLoggerProvider();

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ApplicationName = "OrchardCore.Mvc.Web",
            ContentRootPath = contentRoot,
        });

        builder.Services
            .AddOrchardCore()
            .AddMvc();

        ConfigureServices(builder, appDataPath, instanceId: null, loggerProvider);

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
        (record.Category == "Microsoft.AspNetCore.DataProtection.KeyManagement.XmlKeyManager"
            && record.Message.Contains("No XML encryptor configured"))
        || record.Category == "OrchardCore.Media.Controllers.MediaApiController"
        || record.Category == "OrchardCore.Media.Services.DiskTusTempStore"
        || (record.Category == "Microsoft.AspNetCore.Server.Kestrel"
            && record.Message.Contains("Connection processing ended abnormally"))
        || (record.Category == "Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddleware"
            && record.Exception?.ToString().Contains("Cannot create directory '_Users") == true);

    public async ValueTask DisposeAsync()
    {
        if (_app is not null)
        {
            await _app.StopAsync();
            await _app.DisposeAsync();
        }
    }

    private static void ConfigureServices(
        WebApplicationBuilder builder,
        string appDataPath,
        string instanceId,
        FakeLoggerProvider loggerProvider)
    {
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.WebHost.UseSetting("suppressHostingStartup", "true");

        var resolvedAppDataPath = Path.IsPathRooted(appDataPath)
            ? appDataPath
            : Path.Combine(builder.Environment.ContentRootPath, appDataPath);

        // Override app data path per fixture via PostConfigure (no env var needed).
        builder.Services.PostConfigure<ShellOptions>(options =>
        {
            options.ShellsApplicationDataPath = resolvedAppDataPath;
        });

        // Override database config via IConfiguration (higher priority than env vars)
        // so we never need to mutate global environment variables.
        if (!string.IsNullOrEmpty(_originalConnectionString) && !string.IsNullOrEmpty(_originalDatabaseProvider))
        {
            var connectionString = _originalConnectionString;

            if (!string.IsNullOrEmpty(instanceId))
            {
                var dbName = ExtractDatabaseName(_originalConnectionString);
                if (dbName is not null)
                {
                    connectionString = ReplaceDatabaseName(_originalConnectionString, $"{dbName}_{instanceId}");
                    EnsureCleanDatabase(connectionString, _originalDatabaseProvider);
                }
            }

            // Store for test helpers that need the fixture-specific connection string.
            _currentConnectionString = connectionString;

            // Write tenants.json so OrchardCore picks up the per-fixture database.
            Directory.CreateDirectory(resolvedAppDataPath);
            File.WriteAllText(
                Path.Combine(resolvedAppDataPath, "tenants.json"),
                JsonSerializer.Serialize(new Dictionary<string, object>
                {
                    ["Default"] = new Dictionary<string, string>
                    {
                        ["DatabaseProvider"] = _originalDatabaseProvider,
                        ["ConnectionString"] = connectionString,
                    },
                }));

            // Override the env-var-sourced configuration keys so ShellSettingsManager
            // doesn't re-apply the original values with highest priority.
            builder.Configuration["OrchardCore:ConnectionString"] = connectionString;
            builder.Configuration["OrchardCore:DatabaseProvider"] = _originalDatabaseProvider;
        }

        // Disable YesSql concurrency checks during setup. Each recipe step runs in a new
        // scope with a new session, but the document cache can serve stale versions, causing
        // ConcurrencyException on SiteSettings with external databases.
        builder.Configuration["OrchardCore:OrchardCore_Documents:CheckConcurrency"] = "false";

        builder.Logging.AddFilter<FakeLoggerProvider>(level => level >= LogLevel.Warning);
        builder.Logging.AddProvider(loggerProvider);
    }

    /// <summary>
    /// Drops and recreates the fixture-specific database to ensure a clean state.
    /// Each test fixture uses its own database (e.g., "app_SaasFixture") to avoid
    /// cross-fixture interference. Dropping ensures no leftover tables from previous runs.
    /// </summary>
    private static void EnsureCleanDatabase(string connectionString, string databaseProvider)
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

        // Check if the database exists.
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

        if (checkCommand.CommandText is null)
        {
            return;
        }

        // Drop if it already exists so we start clean (no leftover tables from previous runs).
        if (checkCommand.ExecuteScalar() is not null)
        {
            // Terminate active connections before dropping (required by Postgres < 13 which lacks DROP ... WITH (FORCE)).
            if (databaseProvider == "Postgres")
            {
                using var terminateCommand = connection.CreateCommand();
                var terminateParam = terminateCommand.CreateParameter();
                terminateParam.ParameterName = "@dbName";
                terminateParam.Value = dbName;
                terminateCommand.Parameters.Add(terminateParam);
                terminateCommand.CommandText = "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = @dbName AND pid <> pg_backend_pid()";
                terminateCommand.ExecuteNonQuery();
            }

            using var dropCommand = connection.CreateCommand();
            dropCommand.CommandText = databaseProvider switch
            {
                "Postgres" => $"DROP DATABASE \"{dbName.Replace("\"", "\"\"")}\"",
                "MySql" => $"DROP DATABASE `{dbName.Replace("`", "``")}`",
                "SqlConnection" => $"ALTER DATABASE [{dbName.Replace("]", "]]")}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{dbName.Replace("]", "]]")}]",
                _ => null,
            };

            dropCommand.ExecuteNonQuery();
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
