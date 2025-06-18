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
    private readonly IStore _store;
    private readonly IDbConnectionAccessor _dbConnectionAccessor;
    private readonly IIndexProfileManager _indexProfileManager;
    private readonly ISiteService _siteService;
    private readonly ElasticsearchDocumentIndexManager _indexDocumentManager;
    private readonly IIndexNameProvider _indexNameProvider;

    public IndexingMigrations(
        ShellSettings shellSettings,
        IStore store,
        IDbConnectionAccessor dbConnectionAccessor,
        IIndexProfileManager indexProfileManager,
        ISiteService siteService,
        ElasticsearchDocumentIndexManager indexDocumentManager,
        [FromKeyedServices(ElasticsearchConstants.ProviderName)] IIndexNameProvider indexNameProvider)
    {
        _shellSettings = shellSettings;
        _store = store;
        _dbConnectionAccessor = dbConnectionAccessor;
        _indexProfileManager = indexProfileManager;
        _siteService = siteService;
        _indexDocumentManager = indexDocumentManager;
        _indexNameProvider = indexNameProvider;
    }

    public async Task<int> CreateAsync()
    {
        var stepNumber = 1;

        if (_shellSettings.IsInitializing())
        {
            return stepNumber;
        }

        var documentTableName = _store.Configuration.TableNameConvention.GetDocumentTable();
        var table = $"{_store.Configuration.TablePrefix}{documentTableName}";
        var dialect = _store.Configuration.SqlDialect;
        var quotedTableName = dialect.QuoteForTableName(table, _store.Configuration.Schema);
        var quotedContentColumnName = dialect.QuoteForColumnName("Content");
        var quotedTypeColumnName = dialect.QuoteForColumnName("Type");

        var sqlBuilder = new SqlBuilder(_store.Configuration.TablePrefix, _store.Configuration.SqlDialect);
        sqlBuilder.AddSelector(quotedContentColumnName);
        sqlBuilder.From(quotedTableName);
        sqlBuilder.WhereAnd($" {quotedTypeColumnName} = 'OrchardCore.Search.Elasticsearch.Core.Models.ElasticIndexSettingsDocument, OrchardCore.Search.Elasticsearch.Core' ");
        sqlBuilder.Take("1");

        await using var connection = _dbConnectionAccessor.CreateConnection();
        await connection.OpenAsync();
        var jsonContent = await connection.QueryFirstOrDefaultAsync<string>(sqlBuilder.ToSqlString());

        if (string.IsNullOrEmpty(jsonContent))
        {
            return stepNumber;
        }

        var jsonObject = JsonNode.Parse(jsonContent);

        if (jsonObject["ElasticIndexSettings"] is not JsonObject indexesObject)
        {
            return stepNumber;
        }

        var site = await _siteService.LoadSiteSettingsAsync();

        var defaultSearchProvider = site.Properties["SearchSettings"]?["ProviderName"]?.GetValue<string>();
        var elasticSettings = site.Properties["ElasticSettings"] ?? new JsonObject();

        var defaultSearchIndexName = elasticSettings["SearchIndex"]?.GetValue<string>();
        var defaultSearchFields = GetDefaultSearchFields(elasticSettings);

        foreach (var indexObject in indexesObject)
        {
            var indexName = indexObject.Key;

            var indexFullName = _indexNameProvider.GetFullIndexName(indexName);

            var indexProfile = await _indexProfileManager.NewAsync(ElasticsearchConstants.ProviderName, IndexingConstants.ContentsIndexSource);
            indexProfile.IndexName = indexName;
            indexProfile.IndexFullName = indexFullName;
            indexProfile.Name = indexName;

            var counter = 1;

            while (await _indexProfileManager.FindByNameAsync(indexProfile.Name) is not null)
            {
                indexProfile.Name = $"{indexName}{counter++}";

                if (counter > 50)
                {
                    throw new InvalidOperationException($"Unable to create a unique index name for '{indexName}' after 50 attempts.");
                }
            }

            var metadata = indexProfile.As<ContentIndexMetadata>();

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

            indexProfile.Put(metadata);

            var elasticsearchMetadata = indexProfile.As<ElasticsearchIndexMetadata>();

            var storeSourceData = indexObject.Value[nameof(elasticsearchMetadata.StoreSourceData)]?.GetValue<bool>();

            if (storeSourceData.HasValue)
            {
                elasticsearchMetadata.StoreSourceData = storeSourceData.Value;
            }

            if (string.IsNullOrEmpty(elasticsearchMetadata.AnalyzerName))
            {
                elasticsearchMetadata.AnalyzerName = indexObject.Value[nameof(elasticsearchMetadata.AnalyzerName)]?.GetValue<string>();
            }

            var mapping = await _indexDocumentManager.GetIndexMappingsAsync(indexFullName);

            elasticsearchMetadata.IndexMappings = new ElasticsearchIndexMap
            {
                KeyFieldName = ContentIndexingConstants.ContentItemIdKey,
                Mapping = mapping,
            };

            indexProfile.Put(elasticsearchMetadata);

            var queryMetadata = indexProfile.As<ElasticsearchDefaultQueryMetadata>();
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

            indexProfile.Put(queryMetadata);

            await _indexProfileManager.CreateAsync(indexProfile);

            if (indexName == defaultSearchIndexName && defaultSearchProvider == "Elasticsearch")
            {
                ShellScope.AddDeferredTask(async scope =>
                {
                    var siteService = scope.ServiceProvider.GetRequiredService<ISiteService>();
                    var site = await siteService.LoadSiteSettingsAsync();

                    site.Properties["SearchSettings"]["DefaultIndexProfileName"] = indexProfile.Name;

                    await siteService.UpdateSiteSettingsAsync(site);
                });
            }
        }

        return stepNumber;
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
