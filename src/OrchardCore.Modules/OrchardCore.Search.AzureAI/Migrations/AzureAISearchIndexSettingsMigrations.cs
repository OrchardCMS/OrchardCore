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
using OrchardCore.Modules;
using OrchardCore.Search.AzureAI.Models;
using OrchardCore.Search.AzureAI.Services;
using OrchardCore.Settings;
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
    private readonly IStore _store;
    private readonly IDbConnectionAccessor _dbConnectionAccessor;
    private readonly IIndexProfileManager _indexProfileManager;
    private readonly ISiteService _siteService;
    private readonly IIndexNameProvider _indexNameProvider;

    public AzureAISearchIndexSettingsMigrations(
        ShellSettings shellSettings,
        IStore store,
        IDbConnectionAccessor dbConnectionAccessor,
        IIndexProfileManager indexProfileManager,
        ISiteService siteService,
        [FromKeyedServices(AzureAISearchConstants.ProviderName)] IIndexNameProvider indexNameProvider)
    {
        _shellSettings = shellSettings;
        _store = store;
        _dbConnectionAccessor = dbConnectionAccessor;
        _indexProfileManager = indexProfileManager;
        _siteService = siteService;
        _indexNameProvider = indexNameProvider;
    }

#pragma warning disable CA1822 // Mark members as static
    public int Create()
#pragma warning restore CA1822 // Mark members as static
    {
        return 2;
    }

    public async Task<int> UpdateFrom1Async()
    {
        var stepNumber = 2;

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
        sqlBuilder.WhereAnd($" {quotedTypeColumnName} = 'OrchardCore.Search.AzureAI.Models.AzureAISearchIndexSettingsDocument, OrchardCore.Search.AzureAI.Core' ");
        sqlBuilder.Take("1");

        await using var connection = _dbConnectionAccessor.CreateConnection();
        await connection.OpenAsync();
        var jsonContent = await connection.QueryFirstOrDefaultAsync<string>(sqlBuilder.ToSqlString());

        if (string.IsNullOrEmpty(jsonContent))
        {
            return stepNumber;
        }

        var jsonObject = JsonNode.Parse(jsonContent);

        if (jsonObject["IndexSettings"] is not JsonObject indexesObject)
        {
            return stepNumber;
        }

        var site = await _siteService.GetSiteSettingsAsync();

        var defaultSearchProvider = site.Properties["SearchSettings"]?["ProviderName"]?.GetValue<string>();

        var azureAISearchSettings = site.Properties["AzureAISearchSettings"] ?? new JsonObject();

        var defaultSearchIndexName = azureAISearchSettings["SearchIndex"]?.GetValue<string>();

        var defaultSearchFields = GetDefaultSearchFields(azureAISearchSettings);

        foreach (var indexObject in indexesObject)
        {
            var indexName = indexObject.Value["IndexName"]?.GetValue<string>();

            if (string.IsNullOrEmpty(indexName))
            {
                indexName = indexObject.Key;
            }

            var source = indexObject.Value["Source"]?.GetValue<string>();

            if (source == "Contents")
            {
                source = IndexingConstants.ContentsIndexSource;
            }

            var indexProfile = await _indexProfileManager.NewAsync(AzureAISearchConstants.ProviderName, source ?? IndexingConstants.ContentsIndexSource);
            indexProfile.IndexName = indexName;
            indexProfile.IndexFullName = _indexNameProvider.GetFullIndexName(indexName);
            indexProfile.Name = indexName;

            var counter = 1;

            var properties = indexObject.Value[nameof(indexProfile.Properties)]?.AsObject();

            if (properties is not null && properties.Count > 0)
            {
                indexProfile.Properties = properties.Clone();
            }

            while (await _indexProfileManager.FindByNameAsync(indexProfile.Name) is not null)
            {
                indexProfile.Name = $"{indexName}{counter++}";

                if (counter > 50)
                {
                    throw new InvalidOperationException($"Unable to create a unique index name for '{indexName}' after 50 attempts.");
                }
            }

            var id = indexObject.Value["Id"]?.GetValue<string>();

            if (!string.IsNullOrEmpty(id))
            {
                indexProfile.Id = id;
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

            if (indexContentTypes is null || indexContentTypes.Count == 0)
            {
                indexContentTypes = indexObject.Value["Properties"]?["ContentIndexMetadata"]?["IndexedContentTypes"]?.AsArray();
            }

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

            var azureMetadata = indexProfile.As<AzureAISearchIndexMetadata>();

            if (string.IsNullOrEmpty(azureMetadata.AnalyzerName))
            {
                azureMetadata.AnalyzerName = indexObject.Value[nameof(azureMetadata.AnalyzerName)]?.GetValue<string>();
            }

            var indexMappings = indexObject.Value[nameof(azureMetadata.IndexMappings)]?.AsArray();

            if (indexMappings is not null && indexMappings.Count > 0)
            {
                foreach (var indexMapping in indexMappings)
                {
                    var map = indexMapping.ToObject<AzureAISearchIndexMap>();

                    if (azureMetadata.IndexMappings.Any(map => map.AzureFieldKey.EqualsOrdinalIgnoreCase(map.AzureFieldKey)))
                    {
                        continue;
                    }

                    azureMetadata.IndexMappings.Add(map);
                }
            }

            indexProfile.Put(azureMetadata);

            var queryMetadata = indexProfile.As<AzureAISearchDefaultQueryMetadata>();

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
                queryMetadata.DefaultSearchFields = defaultSearchFields;
            }

            indexProfile.Put(queryMetadata);

            await _indexProfileManager.CreateAsync(indexProfile);

            if (indexName == defaultSearchIndexName && defaultSearchProvider == "Azure AI Search")
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
            defaultSearchFields.Add(AzureAISearchIndexManager.FullTextKey);
        }

        return defaultSearchFields.ToArray();
    }
}
