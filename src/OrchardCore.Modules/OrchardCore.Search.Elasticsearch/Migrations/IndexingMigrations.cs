using System.Text.Json.Nodes;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Contents.Indexing;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Indexing;
using OrchardCore.Indexing.Core;
using OrchardCore.Indexing.Core.Models;
using OrchardCore.Search.Elasticsearch.Core.Models;
using OrchardCore.Search.Elasticsearch.Core.Services;
using OrchardCore.Search.Elasticsearch.Models;
using OrchardCore.Settings;
using YesSql;
using YesSql.Sql;

namespace OrchardCore.Search.Elasticsearch.Migrations;

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
            var indexDocumentManager = scope.ServiceProvider.GetRequiredService<ElasticsearchDocumentIndexManager>();

            var siteService = scope.ServiceProvider.GetRequiredService<ISiteService>();
            var site = await siteService.LoadSiteSettingsAsync();

            var defaultSearchProvider = site.Properties["SearchSettings"]?["ProviderName"]?.GetValue<string>();
            var elasticSettings = site.Properties["ElasticSettings"] ?? new JsonObject();

            var saveSiteSettings = false;

            var defaultSearchIndexName = elasticSettings["SearchIndex"]?.GetValue<string>();
            var defaultSearchFields = GetDefaultSearchFields(elasticSettings);

            foreach (var indexObject in indexesObject)
            {
                var indexName = indexObject.Key;

                var indexFullName = indexNamingProvider.GetFullIndexName(indexName);

                var index = await indexManager.NewAsync(ElasticsearchConstants.ProviderName, IndexingConstants.ContentsIndexSource);
                index.IndexName = indexName;
                index.IndexFullName = indexFullName;
                index.Name = indexName;

                var counter = 1;

                while (await indexManager.FindByNameAsync(index.Name) is not null)
                {
                    index.Name = $"{indexName}{counter++}";

                    if (counter > 50)
                    {
                        throw new InvalidOperationException($"Unable to create a unique index name for '{indexName}' after 50 attempts.");
                    }
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

                var elasticsearchMetadata = index.As<ElasticsearchIndexMetadata>();

                var storeSourceData = indexObject.Value[nameof(elasticsearchMetadata.StoreSourceData)]?.GetValue<bool>();

                if (storeSourceData.HasValue)
                {
                    elasticsearchMetadata.StoreSourceData = storeSourceData.Value;
                }

                if (string.IsNullOrEmpty(elasticsearchMetadata.AnalyzerName))
                {
                    elasticsearchMetadata.AnalyzerName = indexObject.Value[nameof(elasticsearchMetadata.AnalyzerName)]?.GetValue<string>();
                }

                var mapping = await indexDocumentManager.GetIndexMappingsAsync(indexFullName);

                elasticsearchMetadata.IndexMappings = new ElasticsearchIndexMap
                {
                    KeyFieldName = ContentIndexingConstants.ContentItemIdKey,
                    Mapping = mapping,
                };

                index.Put(elasticsearchMetadata);

                var queryMetadata = index.As<ElasticsearchDefaultQueryMetadata>();
                if (string.IsNullOrEmpty(queryMetadata.QueryAnalyzerName))
                {
                    queryMetadata.QueryAnalyzerName = indexObject.Value[nameof(queryMetadata.QueryAnalyzerName)]?.GetValue<string>();
                }

                if (string.IsNullOrEmpty(queryMetadata.QueryAnalyzerName))
                {
                    queryMetadata.QueryAnalyzerName = elasticsearchMetadata.AnalyzerName;
                }

                queryMetadata.DefaultQuery = indexObject.Value[nameof(queryMetadata.DefaultQuery)]?.GetValue<string>();

                if (string.IsNullOrEmpty(queryMetadata.DefaultQuery))
                {
                    queryMetadata.DefaultQuery = elasticSettings["DefaultQuery"]?.GetValue<string>();
                }

                if (queryMetadata.DefaultSearchFields is null || queryMetadata.DefaultSearchFields.Length == 0)
                {
                    queryMetadata.DefaultSearchFields = defaultSearchFields;
                }

                if (string.IsNullOrEmpty(queryMetadata.SearchType))
                {
                    queryMetadata.SearchType = elasticSettings["SearchType"]?.GetValue<string>();
                }

                index.Put(queryMetadata);

                await indexManager.CreateAsync(index);

                if (indexName == defaultSearchIndexName && defaultSearchProvider == "Elasticsearch")
                {
                    site.Properties["SearchSettings"]["DefaultIndexId"] = index.Id;
                    saveSiteSettings = true;
                }
            }

            if (saveSiteSettings)
            {
                await siteService.UpdateSiteSettingsAsync(site);
            }
        });

        return 1;
    }

    private static string[] GetDefaultSearchFields(JsonNode settings)
    {
        var defaultSearchFields = new List<string>();

        var searchFields = settings["DefaultSearchFields"]?.AsArray();

        if (searchFields is not null && searchFields.Count > 0)
        {
            foreach (var field in searchFields)
            {
                var value = field.GetValue<string>();

                if (!string.IsNullOrEmpty(value))
                {
                    defaultSearchFields.Add(value);
                }
            }
        }

        if (defaultSearchFields.Count == 0)
        {
            defaultSearchFields.Add(ContentIndexingConstants.FullTextKey);
        }

        return defaultSearchFields.ToArray();
    }
}
