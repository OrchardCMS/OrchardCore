using System.Text.Json.Nodes;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.Documents;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Search.AzureAI.Models;
using YesSql;
using YesSql.Sql;

namespace OrchardCore.Search.AzureAI.Migrations;

/// <summary>
/// In version 3, we introduced Source, Id and the ability to add metadata to index settings.
/// This migration will migrate any index that was created before v3 to use the new structure.
/// This migration will be removed in future releases.
/// </summary>
internal sealed class AzureAISearchIndexSettingsMigrations : DataMigration
{
#pragma warning disable CA1822 // Mark members as static
    public int Create()
#pragma warning restore CA1822 // Mark members as static
    {
        ShellScope.AddDeferredTask(async scope =>
        {
            var store = scope.ServiceProvider.GetRequiredService<IStore>();
            var dbConnectionAccessor = scope.ServiceProvider.GetRequiredService<IDbConnectionAccessor>();
            var settingsManager = scope.ServiceProvider.GetRequiredService<IDocumentManager<AzureAISearchIndexSettingsDocument>>();

            var documentTableName = store.Configuration.TableNameConvention.GetDocumentTable();
            var table = $"{store.Configuration.TablePrefix}{documentTableName}";
            var dialect = store.Configuration.SqlDialect;
            var quotedTableName = dialect.QuoteForTableName(table, store.Configuration.Schema);
            var quotedContentColumnName = dialect.QuoteForColumnName("Content");
            var quotedTypeColumnName = dialect.QuoteForColumnName("Type");

            var sqlBuilder = new SqlBuilder(store.Configuration.TablePrefix, store.Configuration.SqlDialect);
            sqlBuilder.AddSelector(quotedContentColumnName);
            sqlBuilder.From(quotedTableName);
            sqlBuilder.WhereAnd($" {quotedTypeColumnName} = 'OrchardCore.Search.AzureAI.Models.AzureAISearchIndexSettingsDocument, OrchardCore.Search.AzureAI.Core' ");
            sqlBuilder.Take("1");

            await using var connection = dbConnectionAccessor.CreateConnection();
            await connection.OpenAsync();
            var jsonContent = await connection.QueryFirstOrDefaultAsync<string>(sqlBuilder.ToSqlString());

            if (string.IsNullOrEmpty(jsonContent))
            {
                return;
            }

            var jsonObject = JsonNode.Parse(jsonContent);

            if (jsonObject["IndexSettings"] is not JsonObject indexesObject)
            {
                return;
            }

            var document = await settingsManager.GetOrCreateMutableAsync();

            foreach (var indexObject in indexesObject)
            {
                var source = indexObject.Value["Source"]?.GetValue<string>();

                if (!string.IsNullOrEmpty(source))
                {
                    // No migration is needed.
                    continue;
                }

                indexObject.Value["Source"] = AzureAISearchConstants.ContentsIndexSource;

                var indexName = indexObject.Value["IndexName"]?.GetValue<string>();

                if (!string.IsNullOrEmpty(indexName))
                {
                    // Bad index! this is a scenario that should never happen.
                    continue;
                }

                if (!document.IndexSettings.TryGetValue(indexName, out var indexSettings))
                {
                    // Bad index! this is a scenario that should never happen.
                    continue;
                }

                if (string.IsNullOrEmpty(indexSettings.Id))
                {
                    indexSettings.Id = IdGenerator.GenerateId();
                }

                var metadata = indexSettings.As<ContentIndexMetadata>();

                if (string.IsNullOrEmpty(metadata.Culture))
                {
                    metadata.Culture = indexObject.Value[nameof(ContentIndexMetadata.Culture)]?.GetValue<string>();
                }

                var indexLatest = indexObject.Value[nameof(ContentIndexMetadata.IndexLatest)]?.GetValue<bool>();

                if (indexLatest.HasValue)
                {
                    metadata.IndexLatest = indexLatest.Value;
                }

                var indexContentTypes = indexObject.Value[nameof(ContentIndexMetadata.IndexedContentTypes)]?.GetValue<string[]>();

                if (indexContentTypes is not null)
                {
                    metadata.IndexedContentTypes = indexContentTypes;
                }

                indexSettings.Put(metadata);

                document.IndexSettings.Remove(indexName);
                document.IndexSettings[indexSettings.Id] = indexSettings;
            }

            await settingsManager.UpdateAsync(document);
        });

        return 1;
    }
}
