using System.Text.Json.Nodes;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Indexing;
using OrchardCore.Indexing.Core;
using OrchardCore.Indexing.Core.Models;
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
    private readonly ShellSettings _shellSettings;

    public AzureAISearchIndexSettingsMigrations(ShellSettings shellSettings)
    {
        _shellSettings = shellSettings;
    }

#pragma warning disable CA1822 // Mark members as static
    public int Create()
#pragma warning restore CA1822 // Mark members as static
    {
        return 2;
    }

    public int UpdateFrom1()
    {
        if (_shellSettings.IsInitializing())
        {
            return 2;
        }

        ShellScope.AddDeferredTask(async scope =>
        {
            var store = scope.ServiceProvider.GetRequiredService<IStore>();
            var dbConnectionAccessor = scope.ServiceProvider.GetRequiredService<IDbConnectionAccessor>();
            var indexManager = scope.ServiceProvider.GetRequiredService<IIndexEntityManager>();

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

            foreach (var indexObject in indexesObject)
            {
                var indexName = indexObject.Value["IndexName"]?.GetValue<string>();

                if (string.IsNullOrEmpty(indexName))
                {
                    indexName = indexObject.Key;
                }

                var source = indexObject.Value["Source"]?.GetValue<string>();

                var index = await indexManager.NewAsync(AzureAISearchConstants.ProviderName, source ?? IndexingConstants.ContentsIndexSource);
                index.IndexName = indexName;
                index.IndexFullName = _shellSettings.Name + "_" + indexName;
                var id = indexObject.Value["Id"]?.GetValue<string>();

                if (!string.IsNullOrEmpty(id))
                {
                    index.Id = id;
                }

                var metadata = index.As<ContentIndexMetadata>();

                if (string.IsNullOrEmpty(metadata.Culture))
                {
                    metadata.Culture = indexObject.Value[nameof(metadata.Culture)]?.GetValue<string>();
                }

                var indexLatest = indexObject.Value[nameof(metadata.IndexLatest)]?.GetValue<bool>();

                if (indexLatest.HasValue)
                {
                    metadata.IndexLatest = indexLatest.Value;
                }

                var indexContentTypes = indexObject.Value[nameof(metadata.IndexedContentTypes)]?.AsArray();

                if (indexContentTypes is not null)
                {
                    var items = new HashSet<string>();

                    foreach (var indexContentType in indexContentTypes)
                    {
                        var value = indexContentType.GetValue<string>();

                        if (!string.IsNullOrEmpty(value))
                        {
                            items.Add(value);
                        }
                    }

                    metadata.IndexedContentTypes = items.ToArray();
                }

                index.Put(metadata);

                var azureMetadata = index.As<AzureAISearchIndexMetadata>();

                if (string.IsNullOrEmpty(azureMetadata.AnalyzerName))
                {
                    azureMetadata.AnalyzerName = indexObject.Value[nameof(azureMetadata.AnalyzerName)]?.GetValue<string>();
                }

                if (string.IsNullOrEmpty(azureMetadata.QueryAnalyzerName))
                {
                    azureMetadata.QueryAnalyzerName = indexObject.Value[nameof(azureMetadata.QueryAnalyzerName)]?.GetValue<string>();
                }

                if (string.IsNullOrEmpty(azureMetadata.QueryAnalyzerName))
                {
                    azureMetadata.QueryAnalyzerName = azureMetadata.AnalyzerName;
                }

                var indexMappings = indexObject.Value[nameof(azureMetadata.IndexMappings)]?.AsArray();

                if (indexMappings is not null)
                {
                    foreach (var indexMapping in indexMappings)
                    {
                        azureMetadata.IndexMappings.Add(indexMapping.GetValue<AzureAISearchIndexMap>());
                    }
                }

                await indexManager.CreateAsync(index);
            }
        });

        return 2;
    }
}
