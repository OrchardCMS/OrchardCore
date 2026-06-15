using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Data;
using OrchardCore.Data.Documents;
using OrchardCore.Data.Migration;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Scope;
using YesSql;

namespace OrchardCore.Environment.Shell.Removing;

/// <summary>
/// Allows to remove the database tables retrieved from the migrations of a given tenant.
/// </summary>
public class ShellDbTablesRemovingHandler : IShellRemovingHandler
{
    private readonly IShellContextFactory _shellContextFactory;
    protected readonly IStringLocalizer S;
    private readonly ILogger _logger;

    public ShellDbTablesRemovingHandler(
        IShellContextFactory shellContextFactory,
        IStringLocalizer<ShellDbTablesRemovingHandler> localizer,
        ILogger<ShellDbTablesRemovingHandler> logger)
    {
        _shellContextFactory = shellContextFactory;
        S = localizer;
        _logger = logger;
    }

    /// <summary>
    /// Removes the database tables retrieved from the migrations of the provided tenant.
    /// </summary>
    public async Task RemovingAsync(ShellRemovingContext context)
    {
        if (context.LocalResourcesOnly)
        {
            return;
        }

        if (context.ShellSettings.IsUninitialized())
        {
            var dbConnectionValidator = ShellScope.Services?.GetService<IDbConnectionValidator>();
            if (dbConnectionValidator is null)
            {
                return;
            }

            // Check for a valid connection and if at least the 'Document' table exists.
            var validationContext = new DbConnectionValidatorContext(context.ShellSettings);
            var result = await dbConnectionValidator.ValidateAsync(validationContext);

            if (result != DbConnectionValidatorResult.DocumentTableFound)
            {
                return;
            }
        }

        // Can't resolve 'IStore' if 'Uninitialized', so force to 'Disabled'.
        var shellSettings = new ShellSettings(context.ShellSettings).AsDisabled();

        // If no database provider is configured, there are no tables to remove.
        if (shellSettings["DatabaseProvider"] is null)
        {
            return;
        }

        ShellDbTablesInfo shellDbTablesInfo;
        try
        {
            shellDbTablesInfo = await GetTablesToRemoveAsync(shellSettings);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to determine the tables to remove for tenant '{TenantName}'. " +
                "The database may be missing or inaccessible. Skipping table removal.",
                shellSettings.Name);

            // If we can't even determine which tables to remove (e.g., the database is
            // unreachable or ISession is null), we skip table removal gracefully.
            return;
        }

        if (!context.Success)
        {
            return;
        }

        try
        {
            // Create a minimum shell context without any features.
            await using var shellContext = await _shellContextFactory.CreateMinimumContextAsync(shellSettings);
            var store = shellContext.ServiceProvider.GetRequiredService<IStore>();

            await using var connection = store.Configuration.ConnectionFactory.CreateConnection();
            if (shellSettings["DatabaseProvider"] == DatabaseProviderValue.Sqlite && connection is SqliteConnection sqliteConnection)
            {
                // Clear the pool to unlock the file and remove it.
                SqliteConnection.ClearPool(sqliteConnection);

                if (File.Exists(connection.DataSource))
                {
                    File.Delete(connection.DataSource);
                }
            }
            else
            {
                await connection.OpenAsync();
                using var transaction = await connection.BeginTransactionAsync(store.Configuration.IsolationLevel);
                try
                {
                    // Remove all tables of this tenant.
                    shellDbTablesInfo.Configure(transaction, _logger, throwOnError: false);
                    shellDbTablesInfo.RemoveAllTables();

                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }
        catch (Exception ex) when (IsDatabaseNotFoundException(ex, shellSettings))
        {
            // The database file or server is not reachable. Since there's nothing to clean up,
            // log a warning and allow the removal to continue.
            _logger.LogWarning(
                ex,
                "The database for tenant '{TenantName}' is not accessible. " +
                "Skipping table removal as the database may have already been removed.",
                shellSettings.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove the tables of tenant '{TenantName}'.", shellSettings.Name);
            context.ErrorMessage = S["Failed to remove the tables."];
            context.Error = ex;
        }
    }

    /// <summary>
    /// Gets the database tables retrieved from the migrations of the provided tenant.
    /// </summary>
    private async Task<ShellDbTablesInfo> GetTablesToRemoveAsync(ShellSettings shellSettings)
    {
        var shellDbTablesInfo = new ShellDbTablesInfo();
        if (shellSettings["DatabaseProvider"] is null || shellSettings["DatabaseProvider"] == DatabaseProviderValue.Sqlite)
        {
            // No database provider configured or the whole SQLite database file will be removed.
            return shellDbTablesInfo;
        }

        // Create an isolated shell context composed of all features that have been installed.
        await using var shellContext = await _shellContextFactory.CreateMaximumContextAsync(shellSettings);
        await (await shellContext.CreateScopeAsync()).UsingServiceScopeAsync(async scope =>
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
                        shellSettings.Name);

                    // Replaying a migration may fail for the same reason that a setup or any migration failed.
                    // So the tenant removal is not interrupted, the already enlisted tables may be sufficient.

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

    /// <summary>
    /// Determines whether the exception indicates the database file or server does not exist,
    /// as opposed to being locked or otherwise inaccessible.
    /// </summary>
    private static bool IsDatabaseNotFoundException(Exception ex, ShellSettings shellSettings)
    {
        if (shellSettings["DatabaseProvider"] == DatabaseProviderValue.Sqlite)
        {
            // SQLite Error 14 (SQLITE_CANTOPEN) with "unable to open database file"
            // indicates the file doesn't exist. A locked file produces a different error code.
            if (ex is SqliteException sqliteEx && sqliteEx.SqliteErrorCode == 14)
            {
                return true;
            }

            // Also handle the case where an inner SqliteException is wrapped.
            if (ex.InnerException is SqliteException innerSqliteEx && innerSqliteEx.SqliteErrorCode == 14)
            {
                return true;
            }
        }

        return false;
    }
}
