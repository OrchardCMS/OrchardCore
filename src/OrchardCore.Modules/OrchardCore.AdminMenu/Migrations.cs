using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.AdminMenu.Services;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.Environment.Shell.Scope;
using YesSql;

namespace OrchardCore.AdminMenu;

public sealed class Migrations : DataMigration
{
    // The 'AdminMenuList' document type used to live in the 'OrchardCore.AdminMenu' assembly and was
    // moved to 'OrchardCore.AdminMenu.Abstractions'. YesSql stores the assembly-qualified type name in
    // the 'Type' column of the 'Document' table and filters documents by it, so records created before
    // the move can no longer be found. This is the type name stored by pre-move (pre-3.0) sites...
    private const string OldAdminMenuListType = "OrchardCore.AdminMenu.Models.AdminMenuList, OrchardCore.AdminMenu";

    // ...and the type name after the move.
    private const string NewAdminMenuListType = "OrchardCore.AdminMenu.Models.AdminMenuList, OrchardCore.AdminMenu.Abstractions";

    public static int Create()
    {
        // Data manipulation must be deferred so that it runs in a fresh scope after the migration's
        // schema transaction has been committed. Each deferred task runs in its own scope, which also
        // avoids concurrency issues when several features migrate the same AdminMenuList document.

        // First rename the stored document type. This must run (and commit) before the format migration
        // below, which reads the AdminMenuList through YesSql and would otherwise find nothing.
        ShellScope.AddDeferredTask(RenameAdminMenuListDocumentTypeAsync);

        // Then migrate pre-existing admin menus (created with pre-3.0 libraries) to the 3.0 format.
        ShellScope.AddDeferredTask(async scope =>
            await AdminMenuItemMigrator.MigrateItemTo(scope, async (menu, menuItem, scope) => menuItem.MenuName = menu.Name));

        return 1;
    }

    private static async Task RenameAdminMenuListDocumentTypeAsync(ShellScope scope)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Migrations>>();

        try
        {
            var store = scope.ServiceProvider.GetRequiredService<IStore>();
            var dbConnectionAccessor = scope.ServiceProvider.GetRequiredService<IDbConnectionAccessor>();

            var configuration = store.Configuration;
            var dialect = configuration.SqlDialect;

            var documentTable = $"{configuration.TablePrefix}{configuration.TableNameConvention.GetDocumentTable()}";
            var quotedTableName = dialect.QuoteForTableName(documentTable, configuration.Schema);
            var quotedTypeColumn = dialect.QuoteForColumnName("Type");

            // The type names are fixed, known-safe constants (no quotes), so they can be inlined.
            var updateCmd = $"UPDATE {quotedTableName} SET {quotedTypeColumn} = '{NewAdminMenuListType}' WHERE {quotedTypeColumn} = '{OldAdminMenuListType}'";

            await using var connection = dbConnectionAccessor.CreateConnection();
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync(configuration.IsolationLevel);

            await using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = updateCmd;

            var affected = await command.ExecuteNonQueryAsync();

            await transaction.CommitAsync();

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Renamed {Count} AdminMenuList document(s) to the '{Type}' type.", affected, NewAdminMenuListType);
            }
        }
        catch (Exception e)
        {
            // Log explicitly so the failure is visible while testing.
            logger.LogError(e, "Failed to rename the AdminMenuList document type.");
            throw;
        }
    }
}
