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
public class ShellDbTablesRemovingHandler : ShellRemovingHostHandler
{
    private readonly IShellContextFactory _shellContextFactory;
    private readonly ILogger _logger;

    public ShellDbTablesRemovingHandler(
        IShellContextFactory shellContextFactory,
        ILogger<ShellDbTablesRemovingHandler> logger)
    {
        _shellContextFactory = shellContextFactory;
        _logger = logger;
    }

    /// <summary>
    /// Removes the database tables retrieved from the migrations of the provided tenant.
    /// </summary>
    public override async Task RemovingAsync(ShellRemovingContext context)
    {
        var shellDbTablesInfo = await GetTablesToRemoveAsync(context);
        if (!context.Success)
        {
            return;
        }

        try
        {
            // Create a minimum shell context without any features.
            using var shellContext = await _shellContextFactory.CreateMinimumContextAsync(context.ShellSettings);
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
            _logger.LogError(ex, "Failed to remove the tables of tenant '{TenantName}'.", context.ShellSettings.Name);
            context.ErrorMessage = $"Failed to remove the tables.";
            context.Error = ex;
        }
    }

    /// <summary>
    /// Gets the database tables retrieved from the migrations of the provided tenant.
    /// </summary>
    private async Task<ShellDbTablesInfo> GetTablesToRemoveAsync(ShellRemovingContext context)
    {
        var shellDbTablesInfo = new ShellDbTablesInfo().Configure(context.ShellSettings);
        if (shellDbTablesInfo.DatabaseProvider == "Sqlite")
        {
            // The whole database file will be removed.
            return shellDbTablesInfo;
        }

        // Create a shell context composed of all features that have been installed.
        using var shellContext = await _shellContextFactory.CreateMaximumContextAsync(context.ShellSettings);
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
                        context.ShellSettings.Name);

                    context.ErrorMessage = $"Failed to replay the migration '{type}' from version '{version}'.";
                    context.Error = ex;

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
