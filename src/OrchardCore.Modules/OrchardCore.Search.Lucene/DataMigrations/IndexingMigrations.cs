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
using OrchardCore.Search.Lucene.Models;
using OrchardCore.Settings;
using YesSql;
using YesSql.Sql;

namespace OrchardCore.Search.Lucene.DataMigrations;

internal sealed class IndexingMigrations : DataMigration
{
    private readonly ShellSettings _shellSettings;
    private readonly IStore _store;
    private readonly IDbConnectionAccessor _dbConnectionAccessor;
    private readonly IIndexProfileManager _indexProfileManager;
    private readonly ISiteService _siteService;
    private readonly IIndexNameProvider _indexNameProvider;

    public IndexingMigrations(
        ShellSettings shellSettings,
        IStore store,
        IDbConnectionAccessor dbConnectionAccessor,
        IIndexProfileManager indexProfileManager,
        ISiteService siteService,
        [FromKeyedServices(LuceneConstants.ProviderName)] IIndexNameProvider indexNameProvider)
    {
        _shellSettings = shellSettings;
        _store = store;
        _dbConnectionAccessor = dbConnectionAccessor;
        _indexProfileManager = indexProfileManager;
        _siteService = siteService;
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
        sqlBuilder.WhereAnd($" {quotedTypeColumnName} = 'OrchardCore.Search.Lucene.Model.LuceneIndexSettingsDocument, OrchardCore.Search.Lucene' ");
        sqlBuilder.Take("1");

        await using var connection = _dbConnectionAccessor.CreateConnection();
        await connection.OpenAsync();
        var jsonContent = await connection.QueryFirstOrDefaultAsync<string>(sqlBuilder.ToSqlString());

        if (string.IsNullOrEmpty(jsonContent))
        {
            return stepNumber;
        }

        var jsonObject = JsonNode.Parse(jsonContent);

        if (jsonObject["LuceneIndexSettings"] is not JsonObject indexesObject)
        {
            return stepNumber;
        }

        var site = await _siteService.GetSiteSettingsAsync();

        var defaultSearchProvider = site.Properties["SearchSettings"]?["ProviderName"]?.GetValue<string>();
        var elasticSettings = site.Properties["LuceneSettings"] ?? new JsonObject();

        var defaultSearchIndexName = elasticSettings["SearchIndex"]?.GetValue<string>();

        foreach (var indexObject in indexesObject)
        {
            var indexName = indexObject.Key;

            var indexFullName = _indexNameProvider.GetFullIndexName(indexName);

            var indexProfile = await _indexProfileManager.NewAsync(LuceneConstants.ProviderName, IndexingConstants.ContentsIndexSource);
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

            var azureMetadata = indexProfile.As<LuceneIndexMetadata>();

            if (string.IsNullOrEmpty(azureMetadata.AnalyzerName))
            {
                azureMetadata.AnalyzerName = indexObject.Value[nameof(azureMetadata.AnalyzerName)]?.GetValue<string>();
            }

            var storeSourceData = indexObject.Value[nameof(azureMetadata.StoreSourceData)]?.GetValue<bool>();

            if (storeSourceData.HasValue)
            {
                azureMetadata.StoreSourceData = storeSourceData.Value;
            }

            indexProfile.Put(azureMetadata);

            var queryMetadata = indexProfile.As<LuceneIndexDefaultQueryMetadata>();
            if (string.IsNullOrEmpty(queryMetadata.QueryAnalyzerName))
            {
                queryMetadata.QueryAnalyzerName = indexObject.Value[nameof(queryMetadata.QueryAnalyzerName)]?.GetValue<string>();
            }

            if (string.IsNullOrEmpty(queryMetadata.QueryAnalyzerName))
            {
                queryMetadata.QueryAnalyzerName = azureMetadata.AnalyzerName;
            }

            if (queryMetadata.DefaultSearchFields is null || queryMetadata.DefaultSearchFields.Length == 0)
            {
                queryMetadata.DefaultSearchFields = [ContentIndexingConstants.FullTextKey];
            }

            indexProfile.Put(queryMetadata);

            await _indexProfileManager.CreateAsync(indexProfile);

            if (indexName == defaultSearchIndexName && defaultSearchProvider == "Lucene")
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
}
