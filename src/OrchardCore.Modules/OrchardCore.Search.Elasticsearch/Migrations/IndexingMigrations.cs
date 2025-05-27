using System.Text.Json.Nodes;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Contents.Indexing;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Indexing;
using OrchardCore.Indexing.Core;
using OrchardCore.Indexing.Core.Models;
using OrchardCore.Json;
using OrchardCore.Search.Elasticsearch;
using OrchardCore.Search.Elasticsearch.Core.Models;
using OrchardCore.Search.Elasticsearch.Core.Services;
using OrchardCore.Search.Elasticsearch.Models;
using YesSql;
using YesSql.Sql;

namespace OrchardCore.Queries.Sql.Migrations;

internal sealed class IndexingMigrations : DataMigration
{
    private readonly ShellSettings _shellSettings;

    public IndexingMigrations(ShellSettings shellSettings)
    {
        _shellSettings = shellSettings;
    }

    public int Create()
    {
        if (_shellSettings.IsInitializing())
        {
            return 1;
        }

        ShellScope.AddDeferredTask(async scope =>
        {
            var store = scope.ServiceProvider.GetRequiredService<IStore>();
            var dbConnectionAccessor = scope.ServiceProvider.GetRequiredService<IDbConnectionAccessor>();

            var documentTableName = store.Configuration.TableNameConvention.GetDocumentTable();
            var table = $"{store.Configuration.TablePrefix}{documentTableName}";
            var dialect = store.Configuration.SqlDialect;
            var quotedTableName = dialect.QuoteForTableName(table, store.Configuration.Schema);
            var quotedContentColumnName = dialect.QuoteForColumnName("Content");
            var quotedTypeColumnName = dialect.QuoteForColumnName("Type");

            var sqlBuilder = new SqlBuilder(store.Configuration.TablePrefix, store.Configuration.SqlDialect);
            sqlBuilder.AddSelector(quotedContentColumnName);
            sqlBuilder.From(quotedTableName);
            sqlBuilder.WhereAnd($" {quotedTypeColumnName} = 'OrchardCore.Search.Elasticsearch.Core.Models.ElasticIndexSettingsDocument, OrchardCore.Search.Elasticsearch.Core' ");
            sqlBuilder.Take("1");

            await using var connection = dbConnectionAccessor.CreateConnection();
            await connection.OpenAsync();
            var jsonContent = await connection.QueryFirstOrDefaultAsync<string>(sqlBuilder.ToSqlString());

            if (string.IsNullOrEmpty(jsonContent))
            {
                return;
            }

            var jsonObject = JsonNode.Parse(jsonContent);

            if (jsonObject["ElasticIndexSettings"] is not JsonObject indexesObject)
            {
                return;
            }

            var indexManager = scope.ServiceProvider.GetRequiredService<IIndexEntityManager>();
            var indexNamingProvider = scope.ServiceProvider.GetKeyedService<IIndexNameProvider>(ElasticsearchConstants.ProviderName);
            var indexDocumentManager = scope.ServiceProvider.GetRequiredService<ElasticsearchIndexDocumentManager>();

            var _serializerOptions = scope.ServiceProvider.GetRequiredService<IOptions<DocumentJsonSerializerOptions>>();
            foreach (var indexObject in indexesObject)
            {
                var indexName = indexObject.Key;

                var indexFullName = indexNamingProvider.GetFullIndexName(indexName);

                var index = await indexManager.NewAsync(ElasticsearchConstants.ProviderName, IndexingConstants.ContentsIndexSource);
                index.IndexName = indexName;
                index.IndexFullName = indexFullName;
                index.DisplayText = indexName;

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

                var azureMetadata = index.As<ElasticsearchIndexMetadata>();

                if (string.IsNullOrEmpty(azureMetadata.AnalyzerName))
                {
                    azureMetadata.AnalyzerName = indexObject.Value[nameof(azureMetadata.AnalyzerName)]?.GetValue<string>();
                }

                index.Put(azureMetadata);

                var contentMetadata = index.As<ElasticsearchContentIndexMetadata>();

                var storeSourceData = indexObject.Value[nameof(contentMetadata.StoreSourceData)]?.GetValue<bool>();

                if (storeSourceData.HasValue)
                {
                    contentMetadata.StoreSourceData = storeSourceData.Value;
                }

                index.Put(contentMetadata);

                var queryMetadata = index.As<ElasticsearchDefaultQueryMetadata>();
                if (string.IsNullOrEmpty(queryMetadata.QueryAnalyzerName))
                {
                    queryMetadata.QueryAnalyzerName = indexObject.Value[nameof(queryMetadata.QueryAnalyzerName)]?.GetValue<string>();
                }

                if (string.IsNullOrEmpty(queryMetadata.QueryAnalyzerName))
                {
                    queryMetadata.QueryAnalyzerName = azureMetadata.AnalyzerName;
                }
                var mapping = await indexDocumentManager.GetIndexMappingsAsync(indexFullName);

                azureMetadata.IndexMappings = new ElasticsearchIndexMap
                {
                    KeyFieldName = ContentIndexingConstants.ContentItemIdKey,
                    SourceField = mapping.Source,
                };

                foreach (var property in mapping.Properties)
                {
                    azureMetadata.IndexMappings.Properties.Add(property.Key.Name, property.Value);
                }

                foreach (var dynamicTemplate in mapping.DynamicTemplates)
                {
                    azureMetadata.IndexMappings.DynamicTemplates.Add(dynamicTemplate);
                }

                index.Put(queryMetadata);

                await indexManager.CreateAsync(index);
            }
        });

        return 1;
    }
}
