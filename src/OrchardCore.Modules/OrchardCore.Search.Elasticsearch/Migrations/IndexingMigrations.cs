using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Contents.Indexing;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Indexing;
using OrchardCore.Indexing.Core;
using OrchardCore.Indexing.Core.Models;
using OrchardCore.Indexing.Models;
using OrchardCore.Search.Elasticsearch.Core.Models;
using OrchardCore.Search.Elasticsearch.Core.Services;
using OrchardCore.Search.Elasticsearch.Models;
using OrchardCore.Settings;
using YesSql;
using YesSql.Sql;

namespace OrchardCore.Search.Elasticsearch.Migrations;

internal sealed class IndexingMigrations : DataMigration
{
    private const string SearchSettings = nameof(SearchSettings);
    private const string DefaultIndexProfileName = nameof(DefaultIndexProfileName);

    private readonly ShellSettings _shellSettings;

    public IndexingMigrations(ShellSettings shellSettings)
    {
        _shellSettings = shellSettings;
    }

    public int Create()
    {
        const int stepNumber = 2;

        if (_shellSettings.IsInitializing())
        {
            return stepNumber;
        }

        ShellScope.AddDeferredTask(async scope =>
        {
            // This logic must be deferred to ensure that other migrations create the necessary database tables first.

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
            sqlBuilder.WhereAnd($" {quotedTypeColumnName} = 'OrchardCore.Search.Elasticsearch.Core.Models.ElasticIndexSettingsDocument, OrchardCore.Search.Elasticsearch.Core' ");
            sqlBuilder.Take("1");

            var dbConnectionAccessor = scope.ServiceProvider.GetRequiredService<IDbConnectionAccessor>();
            await using var connection = dbConnectionAccessor.CreateConnection();
            await connection.OpenAsync();
            var jsonContent = await connection.QueryFirstOrDefaultAsync<string>(sqlBuilder.ToSqlString());

            if (string.IsNullOrEmpty(jsonContent) ||
                JsonNode.Parse(jsonContent)?["ElasticIndexSettings"] is not JsonObject indexesObject)
            {
                return;
            }

            var siteService = scope.ServiceProvider.GetRequiredService<ISiteService>();
            var site = await siteService.LoadSiteSettingsAsync();

            var defaultSearchProvider = site.Properties["SearchSettings"]?["ProviderName"]?.GetValue<string>();
            var elasticSettings = site.Properties["ElasticSettings"] ?? new JsonObject();

            var defaultSearchIndexName = elasticSettings["SearchIndex"]?.GetValue<string>();
            var defaultSearchFields = GetDefaultSearchFields(elasticSettings);

            var indexProfileManager = scope.ServiceProvider.GetRequiredService<IIndexProfileManager>();
            var indexNameProvider = scope.ServiceProvider.GetRequiredKeyedService<IIndexNameProvider>(ElasticsearchConstants.ProviderName);
            var indexDocumentManager = scope.ServiceProvider.GetRequiredService<ElasticsearchDocumentIndexManager>();

            foreach (var indexObject in indexesObject)
            {
                var indexName = indexObject.Key;

                var indexFullName = indexNameProvider.GetFullIndexName(indexName);

                var indexProfile = await indexProfileManager.NewAsync(ElasticsearchConstants.ProviderName, IndexingConstants.ContentsIndexSource);
                indexProfile.IndexName = indexName;
                indexProfile.IndexFullName = indexFullName;
                indexProfile.Name = indexName;

                var counter = 1;

                while (await indexProfileManager.FindByNameAsync(indexProfile.Name) is not null)
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

                var mapping = await indexDocumentManager.GetIndexMappingsAsync(indexFullName);

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

                await indexProfileManager.CreateAsync(indexProfile);

                if (indexName == defaultSearchIndexName && defaultSearchProvider == "Elasticsearch")
                {
                    site.Properties["SearchSettings"]["DefaultIndexProfileName"] = indexProfile.Name;

                    try
                    {
                        await siteService.UpdateSiteSettingsAsync(site);
                    }
                    catch (Exception ex)
                    {
                        // Log the error but do not throw, as this is not critical to the migration.
                        var logger = scope.ServiceProvider.GetRequiredService<ILogger<IndexingMigrations>>();

                        logger.LogError(ex, "An error occurred while updating the default search index profile name in site settings.");
                    }
                }
            }

            await UpdateLegacyAnalyzerNameAsync(indexProfileManager);

            await EnsureDefaultSearchSiteSettingAsync(indexProfileManager, siteService, site);
        });

        return stepNumber;
    }

    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    public int UpdateFrom1()
    {
        ShellScope.AddDeferredTask(scope =>
            UpdateLegacyAnalyzerNameAsync(scope.ServiceProvider.GetRequiredService<IIndexProfileManager>()));

        return 2;
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

    private static async Task UpdateLegacyAnalyzerNameAsync(IIndexProfileManager indexProfileManager)
    {
        // The name "standardanalyzer" is a legacy used prior OC 1.6 release. It can be removed in future releases.
        static bool IsLegacyAnalyzerName(string analyzerName) =>
            analyzerName == "standardanalyzer" || string.IsNullOrEmpty(analyzerName);

        foreach (var indexProfile in await GetElasticsearchIndexesAsync(indexProfileManager))
        {
            ElasticsearchDefaultQueryMetadata queryMetadata = null;
            if (indexProfile.TryGet<ElasticsearchDefaultQueryMetadata>(out var storedQueryMetadata))
            {
                queryMetadata = storedQueryMetadata;
            }

            ElasticsearchIndexMetadata indexMetadata = null;
            if (indexProfile.TryGet<ElasticsearchIndexMetadata>(out var storedIndexMetadata))
            {
                indexMetadata = storedIndexMetadata;
            }

            if (IsLegacyAnalyzerName(queryMetadata?.QueryAnalyzerName) ||
                IsLegacyAnalyzerName(indexMetadata?.AnalyzerName))
            {
                indexProfile.Alter<ElasticsearchDefaultQueryMetadata>(metadata =>
                    metadata.QueryAnalyzerName = ElasticsearchConstants.DefaultAnalyzer);
                indexProfile.Alter<ElasticsearchIndexMetadata>(metadata =>
                    metadata.AnalyzerName = ElasticsearchConstants.DefaultAnalyzer);

                await indexProfileManager.ResetAsync(indexProfile);
                await indexProfileManager.UpdateAsync(indexProfile);
            }
        }
    }

    /// <summary>
    /// In previous versions, if there was one Elasticsearch index it was already treated as the default search index
    /// even if the site setting was not explicitly saved. Since this is no longer the default behavior, the setting has
    /// to be applied in a migration if there are applicable indexes.
    /// </summary>
    private static async Task EnsureDefaultSearchSiteSettingAsync(
        IIndexProfileManager indexProfileManager,
        ISiteService siteService,
        ISite site)
    {
        // Ensure that the site.Properties.SearchSettings object exists so we can safely set its property.
        if (site.Properties[SearchSettings] is not JsonObject searchSettings)
        {
            searchSettings = [];
            site.Properties[SearchSettings] = searchSettings;
        }

        // If the site.Properties.SearchSettings.DefaultIndexProfileName setting is missing or empty, and there is at
        // least one Elasticsearch index in store, set it to the first index.
        if (string.IsNullOrWhiteSpace((searchSettings[DefaultIndexProfileName] as JsonValue)?.GetValue<string>()) &&
            (await GetElasticsearchIndexesAsync(indexProfileManager))?.OrderBy(index => index.CreatedUtc).FirstOrDefault() is { } indexProfile)
        {
            searchSettings[DefaultIndexProfileName] = indexProfile.Name;
            await siteService.UpdateSiteSettingsAsync(site);
        }
    }

    private static ValueTask<IEnumerable<IndexProfile>> GetElasticsearchIndexesAsync(IIndexProfileManager indexProfileManager) =>
        indexProfileManager.GetByProviderAsync(ElasticsearchConstants.ProviderName);
}
