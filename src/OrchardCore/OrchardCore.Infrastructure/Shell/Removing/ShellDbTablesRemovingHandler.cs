using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Data.Documents;
using OrchardCore.Data.Migration;
using OrchardCore.Environment.Shell.Builders;
using YesSql;

namespace OrchardCore.Environment.Shell.Removing;

/// <summary>
/// Allows to remove the database tables retrieved from the migrations of a given tenant.
/// </summary>
public class ShellDbTablesRemovingHandler : IShellRemovingHandler
{
    private readonly IShellHost _shellHost;
    private readonly IShellContextFactory _shellContextFactory;
    private readonly ILogger _logger;

    public ShellDbTablesRemovingHandler(
        IShellHost shellHost,
        IShellContextFactory shellContextFactory,
        ILogger<ShellDbTablesRemovingHandler> logger)
    {
        _shellHost = shellHost;
        _shellContextFactory = shellContextFactory;
        _logger = logger;
    }

    /// <summary>
    /// Removes the database tables retrieved from the migrations of the provided tenant.
    /// </summary>
    public async Task RemovingAsync(ShellRemovingContext context)
    {
        if (!_shellHost.TryGetSettings(context.TenantName, out var shellSettings))
        {
            context.ErrorMessage = $"The tenant '{context.TenantName}' doesn't exist.";
        }

        var shellDbTablesInfo = await GetTablesAsync(context.TenantName);
        if (!shellDbTablesInfo.Success)
        {
            context.ErrorMessage = shellDbTablesInfo.ErrorMessage;
            return;
        }

        try
        {
            // Create a minimum shell context without any features.
            using var shellContext = await _shellContextFactory.CreateMinimumContextAsync(shellSettings);
            var store = shellContext.ServiceProvider.GetRequiredService<IStore>();

            using var connection = store.Configuration.ConnectionFactory.CreateConnection();
            if (shellDbTablesInfo.DatabaseProvider == "Sqlite" && connection is SqliteConnection sqliteConnection)
            {
                // Clear the pool to unlock the file and remove it.
                SqliteConnection.ClearPool(sqliteConnection);
                File.Delete(connection.DataSource);
            }
            else
            {
                await connection.OpenAsync();
                using var transaction = connection.BeginTransaction(store.Configuration.IsolationLevel);

                // Remove all tables of this tenant.
                shellDbTablesInfo.Configure(transaction);
                shellDbTablesInfo.RemoveAllTables();

                transaction.Commit();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove the tables of tenant '{TenantName}'.", context.TenantName);
            shellDbTablesInfo.ErrorMessage = $"Failed to remove the tables of tenant '{context.TenantName}'.";
        }

        if (!shellDbTablesInfo.Success)
        {
            context.ErrorMessage = shellDbTablesInfo.ErrorMessage;
        }
    }

    /// <summary>
    /// Gets the database tables retrieved from the migrations of the provided tenant.
    /// </summary>
    private async Task<ShellDbTablesInfo> GetTablesAsync(string tenant)
    {
        var shellDbTablesInfo = new ShellDbTablesInfo().Configure(tenant);
        if (!_shellHost.TryGetSettings(tenant, out var shellSettings))
        {
            shellDbTablesInfo.ErrorMessage = $"The tenant '{tenant}' doesn't exist.";
            return shellDbTablesInfo;
        }

        shellDbTablesInfo.Configure(shellSettings);

        // Create a shell context composed of all features that have been installed.
        using var shellContext = await _shellContextFactory.CreateMaximumContextAsync(shellSettings);
        await shellContext.CreateScope().UsingServiceScopeAsync(async scope =>
        {
            var store = scope.ServiceProvider.GetRequiredService<IStore>();
            shellDbTablesInfo.Configure(store.Configuration);

            // Replay tenant migrations but by using a 'ShellDbTablesInfo' that
            // only tracks table definitions without executing any Sql commands.
            var migrations = scope.ServiceProvider.GetServices<IDataMigration>();
            foreach (var migration in migrations)
            {
                migration.SchemaBuilder = shellDbTablesInfo;

                var version = 0;
                try
                {
                    version = await migration.ExecuteCreateMethodAsync();
                    version = await migration.ExecuteUpdateMethodsAsync(version);
                }
                catch (Exception ex)
                {
                    var type = migration.GetType().FullName;

                    _logger.LogError(
                        ex,
                        "Failed to replay the migration '{MigrationType}' from version '{Version}' on tenant '{TenantName}'.",
                        type,
                        version,
                        tenant);

                    shellDbTablesInfo.ErrorMessage = $"Failed to replay the migration '{type}' from version '{version}'";

                    break;
                }
            }

            // The 'ShellDbTablesInfo' doesn't execute any Sql command and type definitions
            // are intended to be idempotent, but anyway here nothing needs to be persisted.
            var documentStore = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
            await documentStore.CancelAsync();
        });

        return shellDbTablesInfo;
    }
}
