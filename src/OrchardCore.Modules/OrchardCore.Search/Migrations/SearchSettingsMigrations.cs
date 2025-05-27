using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Data.Migration;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Indexing;
using OrchardCore.Indexing.Models;
using OrchardCore.Search.Models;
using OrchardCore.Settings;

namespace OrchardCore.Search.Migrations;

internal sealed class SearchSettingsMigrations : DataMigration
{
    private readonly ShellSettings _shellSettings;

    public SearchSettingsMigrations(ShellSettings shellSettings)
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
            var siteService = scope.ServiceProvider.GetRequiredService<ISiteService>();

            var site = await siteService.LoadSiteSettingsAsync();

            var searchSettings = site.As<SearchSettings>();

            var indexStore = scope.ServiceProvider.GetRequiredService<IIndexEntityStore>();

            var azureAISearchIndex = await MigrateAzureAISearchSettingsAsync(site, indexStore);

            var elasticsearchIndex = await MigrateElasticsearchSettingsAsync(site, indexStore);

            var luceneIndex = await MigrateLuceneSettingsAsync(site, indexStore);

            var defaultSearchProvider = site.Properties["SearchSettings"]?["ProviderName"]?.GetValue<string>();

            if (defaultSearchProvider == "Azure AI Search")
            {
                searchSettings.DefaultIndexId = azureAISearchIndex?.Id;
            }
            else if (defaultSearchProvider == "Elasticsearch")
            {
                searchSettings.DefaultIndexId = elasticsearchIndex?.Id;
            }
            else if (defaultSearchProvider == "Lucene")
            {
                searchSettings.DefaultIndexId = luceneIndex?.Id;
            }

            site.Put(searchSettings);

            await siteService.UpdateSiteSettingsAsync(site);
        });

        return 1;
    }

    private static async Task<IndexEntity> MigrateAzureAISearchSettingsAsync(ISite site, IIndexEntityStore indexStore)
    {
        var azureAISearchSettings = site.Properties["AzureAISearchSettings"];

        if (azureAISearchSettings is null)
        {
            return null;
        }

        var defaultIndexName = azureAISearchSettings["SearchIndex"]?.GetValue<string>();

        if (string.IsNullOrEmpty(defaultIndexName))
        {
            return null;
        }

        var index = await indexStore.FindByNameAndProviderAsync(defaultIndexName, "AzureAISearch");

        if (index is null)
        {
            // Nothing to do, the index does not exist.
            return null;
        }

        var defaultSearchFields = azureAISearchSettings["DefaultSearchFields"]?.AsArray();

        if (defaultSearchFields?.Count > 0)
        {
            // Migrate the default query settings to the index properties.
            index.Properties["AzureAISearchDefaultQueryMetadata"] = new JsonObject
            {
                ["DefaultSearchFields"] = defaultSearchFields,
            };

            await indexStore.UpdateAsync(index);
        }

        return index;
    }

    private static async Task<IndexEntity> MigrateElasticsearchSettingsAsync(ISite site, IIndexEntityStore indexStore)
    {
        var elasticSettings = site.Properties["ElasticSettings"];

        if (elasticSettings is null)
        {
            return null;
        }

        var defaultIndexName = elasticSettings["SearchIndex"]?.GetValue<string>();

        if (string.IsNullOrEmpty(defaultIndexName))
        {
            return null;
        }

        var index = await indexStore.FindByNameAndProviderAsync(defaultIndexName, "AzureAISearch");

        if (index is null)
        {
            // Nothing to do, the index does not exist.
            return null;
        }

        var defaultQuery = elasticSettings["DefaultQuery"]?.GetValue<string>();

        var defaultSearchFields = elasticSettings["FullTextField"]?.AsArray();

        if (defaultSearchFields?.Count > 0)
        {
            elasticSettings["SearchIndex"] = null;

            // Migrate the default query settings to the index properties.
            index.Properties["ElasticsearchDefaultQueryMetadata"] = elasticSettings;

            await indexStore.UpdateAsync(index);
        }

        return index;
    }

    private static Task<IndexEntity> MigrateLuceneSettingsAsync(ISite site, IIndexEntityStore indexStore)
    {
        // TODO, migrate the Lucene settings.

        return Task.FromResult<IndexEntity>(null);
    }

}
