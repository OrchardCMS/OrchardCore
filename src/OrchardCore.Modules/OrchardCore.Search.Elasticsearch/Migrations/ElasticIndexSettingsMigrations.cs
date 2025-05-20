using System.Text.Json.Nodes;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.Documents;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Search.Elasticsearch.Core.Handlers;
using OrchardCore.Search.Elasticsearch.Core.Models;
using OrchardCore.Search.Elasticsearch.Core.Services;
using OrchardCore.Search.Elasticsearch.Models;
using YesSql;
using YesSql.Sql;

namespace OrchardCore.Search.Elasticsearch.Migrations;

/// <summary>
/// In version 3, we introduced Source, Id and the ability to add metadata to index settings.
/// This migration will migrate any index that was created before v3 to use the new structure.
/// This migration will be removed in future releases.
/// </summary>
internal sealed class ElasticIndexSettingsMigrations : DataMigration
{
    private readonly ShellSettings _shellSettings;

    public ElasticIndexSettingsMigrations(ShellSettings shellSettings)
    {
        _shellSettings = shellSettings;
    }

    public int Create()
    {
        if (_shellSettings.IsInitializing())
        {
            // Don't run this migration when initializing the shell.
            return 1;
        }

        ShellScope.AddDeferredTask(async scope =>
        {
            var store = scope.ServiceProvider.GetRequiredService<IStore>();
            var elasticsearchIndexNameService = scope.ServiceProvider.GetRequiredService<ElasticsearchIndexNameService>();
            var dbConnectionAccessor = scope.ServiceProvider.GetRequiredService<IDbConnectionAccessor>();
            var settingsManager = scope.ServiceProvider.GetRequiredService<IDocumentManager<ElasticIndexSettingsDocument>>();

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

            var document = await settingsManager.GetOrCreateMutableAsync();

            foreach (var indexObject in indexesObject)
            {
                var source = indexObject.Value["Source"]?.GetValue<string>();

                if (!string.IsNullOrEmpty(source))
                {
                    // No migration is needed.
                    continue;
                }

                var indexName = indexObject.Key;

                var indexSettings = new ElasticIndexSettings()
                {
                    Id = IdGenerator.GenerateId(),
                    IndexName = indexName,
                    IndexFullName = elasticsearchIndexNameService.GetFullIndexName(indexName),
                    Source = ElasticsearchConstants.ContentsIndexSource,
                };

                indexSettings.AnalyzerName = indexObject.Value[nameof(indexSettings.AnalyzerName)]?.GetValue<string>();
                indexSettings.QueryAnalyzerName = indexObject.Value[nameof(indexSettings.QueryAnalyzerName)]?.GetValue<string>();

                var metadata = indexSettings.As<ContentIndexMetadata>();

                if (string.IsNullOrEmpty(metadata.Culture))
                {
                    metadata.Culture = indexObject.Value[nameof(metadata.Culture)]?.GetValue<string>();
                }

                var indexLatest = indexObject.Value[nameof(metadata.IndexLatest)]?.GetValue<bool>();

                if (indexLatest.HasValue)
                {
                    metadata.IndexLatest = indexLatest.Value;
                }

                var storeSourceData = indexObject.Value[nameof(metadata.StoreSourceData)]?.GetValue<bool>();

                if (storeSourceData.HasValue)
                {
                    metadata.StoreSourceData = storeSourceData.Value;
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

                indexSettings.Put(metadata);

                ContentElasticsearchFieldIndexEvents.AddMappings(indexSettings);

                document.ElasticIndexSettings.Remove(indexName);

                document.ElasticIndexSettings[indexSettings.Id] = indexSettings;
            }

            await settingsManager.UpdateAsync(document);
        });

        return 1;
    }
}
