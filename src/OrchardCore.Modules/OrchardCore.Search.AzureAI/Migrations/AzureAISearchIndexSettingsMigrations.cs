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

            var indexManager = scope.ServiceProvider.GetRequiredService<IIndexEntityManager>();
            var indexNamingProvider = scope.ServiceProvider.GetKeyedService<IIndexNameProvider>(AzureAISearchConstants.ProviderName);

            var siteService = scope.ServiceProvider.GetRequiredService<ISiteService>();
            var site = await siteService.LoadSiteSettingsAsync();

            var defaultSearchProvider = site.Properties["SearchSettings"]?["ProviderName"]?.GetValue<string>();

            var azureAISearchSettings = site.Properties["AzureAISearchSettings"] ?? new JsonObject();
            var saveSiteSettings = false;
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

                var index = await indexManager.NewAsync(AzureAISearchConstants.ProviderName, source ?? IndexingConstants.ContentsIndexSource);
                index.IndexName = indexName;
                index.IndexFullName = indexNamingProvider.GetFullIndexName(indexName);
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

                index.Put(metadata);

                var azureMetadata = index.As<AzureAISearchIndexMetadata>();

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

                index.Put(azureMetadata);

                var queryMetadata = index.As<AzureAISearchDefaultQueryMetadata>();

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

                index.Put(queryMetadata);

                await indexManager.CreateAsync(index);

                if (indexName == defaultSearchIndexName && defaultSearchProvider == "Azure AI Search")
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
            defaultSearchFields.Add(AzureAISearchIndexManager.FullTextKey);
        }

        return defaultSearchFields.ToArray();
    }
}
