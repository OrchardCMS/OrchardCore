using System.Text.Json.Nodes;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.AdminMenu.Services;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.Environment.Shell.Scope;
using YesSql;
using YesSql.Sql;

namespace OrchardCore.AdminMenu;

public sealed class Migrations : DataMigration
{
    public static int Create()
    {
        // Then migrate pre-existing admin menus (created with pre-3.0 libraries) to the 3.0 format.
        ShellScope.AddDeferredTask(async scope =>
            await AdminMenuItemMigrator.MigrateItemTo(scope, async (menu, menuItem, scope) => menuItem.MenuName = menu.Name));

        ShellScope.AddDeferredTask(MigrateAdminContentTypesAdminNode);

        return 1;
    }

    private static async Task MigrateAdminContentTypesAdminNode(ShellScope scope)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Migrations>>();

        try
        {
            // This logic must be deferred to ensure that other migrations create the necessary database tables first.
            var contentDefinitionManager = scope.ServiceProvider.GetRequiredService<IContentDefinitionManager>();
            var store = scope.ServiceProvider.GetRequiredService<IStore>();
            var documentTableName = store.Configuration.TableNameConvention.GetDocumentTable();
            var table = $"{store.Configuration.TablePrefix}{documentTableName}";
            var dialect = store.Configuration.SqlDialect;
            var quotedTableName = dialect.QuoteForTableName(table, store.Configuration.Schema);
            var quotedContentColumnName = dialect.QuoteForColumnName("Content");
            var quotedTypeColumnName = dialect.QuoteForColumnName("Type");

            var sqlBuilder = new SqlBuilder(store.Configuration.TablePrefix, store.Configuration.SqlDialect);
            sqlBuilder.AddSelector(quotedContentColumnName);
            sqlBuilder.From(quotedTableName);
            sqlBuilder.WhereAnd($" {quotedTypeColumnName} = 'OrchardCore.AdminMenu.Models.AdminMenuList, OrchardCore.AdminMenu' ");
            sqlBuilder.Take("1");

            var dbConnectionAccessor = scope.ServiceProvider.GetRequiredService<IDbConnectionAccessor>();

            await using var connection = dbConnectionAccessor.CreateConnection();
            await connection.OpenAsync();
            var jsonContent = await connection.QueryFirstOrDefaultAsync<string>(sqlBuilder.ToSqlString());

            if (string.IsNullOrEmpty(jsonContent))
            {
                logger.LogInformation("Convert to 3.x format: no admin menu record.");
                return;
            }

            var jsonObject = JsonNode.Parse(jsonContent);

            // Retrieve all admin menus
            if (!(jsonObject["AdminMenu"] is JsonArray adminMenuItems))
            {
                logger.LogWarning("Convert to 3.x format: no admin menu item, strange AdminMenuList format.");
                return;
            }

            foreach (var adminMenu in adminMenuItems.OfType<JsonObject>())
            {
                // Work on menu items
                if (adminMenu["MenuItems"] is JsonArray menuItems)
                {
                    await MigrateContentTypesNodes(menuItems, contentDefinitionManager, logger);
                }
            }

            // Save the modified AdminMenuList back into the 'Content' column.
            var updateCmd = $"UPDATE {quotedTableName} SET {quotedContentColumnName} = @Content WHERE {quotedTypeColumnName} = 'OrchardCore.AdminMenu.Models.AdminMenuList, OrchardCore.AdminMenu'";

            // Reuse the already-open connection.
            await using var updTransaction = await connection.BeginTransactionAsync(store.Configuration.IsolationLevel);

            var affected = await connection.ExecuteAsync(updateCmd, new { Content = jsonObject.ToJsonString() }, updTransaction);

            await updTransaction.CommitAsync();
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Convert to 3.x format: record modified {Affected}.", affected);
            }
        }
        catch (Exception e)
        {
            // Log explicitly so the failure is visible while testing.
            logger.LogError(e, "Convert to 3.x format: Failed to convert 'ContentTypesAdminNode' in the AdminMenuList document type.");
            throw;
        }
    }

    private static async Task MigrateContentTypesNodes(JsonArray nodes, IContentDefinitionManager contentDefinitionManager, ILogger<Migrations> logger)
    {
        foreach (var node in nodes.OfType<JsonObject>())
        {
            // Only 'ContentTypesAdminNode' carries a 'ContentTypes' array.
            if (node["ContentTypes"] is JsonArray contentTypes)
            {
                foreach (var entry in contentTypes.OfType<JsonObject>())
                {
                    var contentTypeId = entry["ContentTypeId"]?.GetValue<string>();
                    if (logger.IsEnabled(LogLevel.Information))
                    {
                        logger.LogInformation("Convert to 3.x format: Menu item {ContentTypeId}", contentTypeId);
                    }

                    if (!string.IsNullOrEmpty(contentTypeId) && string.IsNullOrEmpty(entry["ContentTypeName"]?.GetValue<string>()))
                    {
                        entry["ContentTypeName"] = contentTypeId;

                        if (string.IsNullOrEmpty(entry["ContentTypeDisplayName"]?.GetValue<string>()))
                        {
                            var typedef = await contentDefinitionManager.GetTypeDefinitionAsync(contentTypeId);
                            entry["ContentTypeDisplayName"] = typedef?.DisplayName ?? contentTypeId;
                        }

                        //remove ContentTypeId
                        entry.Remove("ContentTypeId");
                    }
                }
            }

            // Recourse into child nodes.
            if (node["Items"] is JsonArray items)
            {
                await MigrateContentTypesNodes(items, contentDefinitionManager, logger);
            }
        }
    }
}
