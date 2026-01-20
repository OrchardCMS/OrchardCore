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
using OrchardCore.Search.Lucene.Models;
using OrchardCore.Settings;
using YesSql;
using YesSql.Sql;

namespace OrchardCore.Search.Lucene.DataMigrations;

internal sealed class IndexingMigrations : DataMigration
{
    private readonly ShellSettings _shellSettings;

    public IndexingMigrations(ShellSettings shellSettings)
    {
        _shellSettings = shellSettings;
    }

    public int Create()
    {
        var stepNumber = 1;

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
            sqlBuilder.WhereAnd($" {quotedTypeColumnName} = 'OrchardCore.Search.Lucene.Model.LuceneIndexSettingsDocument, OrchardCore.Search.Lucene' ");
            sqlBuilder.Take("1");

            var dbConnectionAccessor = scope.ServiceProvider.GetRequiredService<IDbConnectionAccessor>();

            await using var connection = dbConnectionAccessor.CreateConnection();
            await connection.OpenAsync();
            var jsonContent = await connection.QueryFirstOrDefaultAsync<string>(sqlBuilder.ToSqlString());

            if (string.IsNullOrEmpty(jsonContent))
            {
                return;
            }

            var jsonObject = JsonNode.Parse(jsonContent);

            if (jsonObject["LuceneIndexSettings"] is not JsonObject indexesObject)
            {
                return;
            }

            var siteService = scope.ServiceProvider.GetRequiredService<ISiteService>();
            var site = await siteService.LoadSiteSettingsAsync();

            var defaultSearchProvider = site.Properties["SearchSettings"]?["ProviderName"]?.GetValue<string>();
            var elasticSettings = site.Properties["LuceneSettings"] ?? new JsonObject();

            var defaultSearchIndexName = elasticSettings["SearchIndex"]?.GetValue<string>();

            var indexProfileManager = scope.ServiceProvider.GetRequiredService<IIndexProfileManager>();
            var indexNameProvider = scope.ServiceProvider.GetRequiredKeyedService<IIndexNameProvider>(LuceneConstants.ProviderName);
            var indexDocumentManager = scope.ServiceProvider.GetRequiredService<LuceneIndexManager>();

            foreach (var indexObject in indexesObject)
            {
                var indexName = indexObject.Key;

                var indexFullName = indexNameProvider.GetFullIndexName(indexName);

                var indexProfile = await indexProfileManager.NewAsync(LuceneConstants.ProviderName, IndexingConstants.ContentsIndexSource);
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

                await indexProfileManager.CreateAsync(indexProfile);

                if (indexName == defaultSearchIndexName && defaultSearchProvider == "Lucene")
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
        });

        return stepNumber;
    }
}
