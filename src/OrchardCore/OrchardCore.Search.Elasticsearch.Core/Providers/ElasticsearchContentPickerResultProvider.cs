using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.Search.Elasticsearch.Core.Models;
using OrchardCore.Search.Elasticsearch.Core.Services;

namespace OrchardCore.Search.Elasticsearch.Core.Providers;

public class ElasticsearchContentPickerResultProvider : IContentPickerResultProvider
{
    private readonly ElasticsearchIndexManager _elasticIndexManager;
    private readonly ElasticsearchConnectionOptions _elasticConnectionOptions;

    public ElasticsearchContentPickerResultProvider(
        IOptions<ElasticsearchConnectionOptions> elasticConnectionOptions,
        ElasticsearchIndexManager elasticIndexManager)
    {
        _elasticConnectionOptions = elasticConnectionOptions.Value;
        _elasticIndexManager = elasticIndexManager;
    }

    public string Name => "Elasticsearch";

    public async Task<IEnumerable<ContentPickerResult>> Search(ContentPickerSearchContext searchContext)
    {
        if (!_elasticConnectionOptions.FileConfigurationExists())
        {
            return [];
        }

        string indexName = null;

        var fieldSettings = searchContext.PartFieldDefinition?.GetSettings<ContentPickerFieldElasticEditorSettings>();

        if (!string.IsNullOrWhiteSpace(fieldSettings?.Index))
        {
            indexName = fieldSettings.Index;
        }

        if (indexName != null && !await _elasticIndexManager.ExistsAsync(indexName))
        {
            return [];
        }

        var results = new List<ContentPickerResult>();

        await _elasticIndexManager.SearchAsync(indexName, async elasticClient =>
        {
            SearchResponse<Dictionary<string, object>> searchResponse = null;
            var elasticTopDocs = new ElasticsearchTopDocs();

            if (string.IsNullOrWhiteSpace(searchContext.Query))
            {
                searchResponse = await elasticClient.SearchAsync<Dictionary<string, object>>(s => s
                    .Index(_elasticIndexManager.GetFullIndexName(indexName))
                    .Query(q => q
                        .Bool(b => b
                            .Filter(f => f
                                .Terms(t => t
                                    .Field("Content.ContentItem.ContentType")
                                    .Term(new TermsQueryField(searchContext.ContentTypes.Select(contentType => FieldValue.String(contentType)).ToArray()))
                                )
                            )
                        )
                    )
                );
            }
            else
            {
                searchResponse = await elasticClient.SearchAsync<Dictionary<string, object>>(s => s
                    .Index(_elasticIndexManager.GetFullIndexName(indexName))
                    .Query(q => q
                        .Bool(b => b
                            .Filter(f => f
                                .Terms(t => t
                                    .Field("Content.ContentItem.ContentType")
                                    .Term(new TermsQueryField(searchContext.ContentTypes.Select(contentType => FieldValue.String(contentType)).ToArray()))
                                )
                            )
                            .Should(s => s
                                .Wildcard(w => w
                                    .Field("Content.ContentItem.DisplayText.Normalized")
                                    .Wildcard(searchContext.Query.ToLowerInvariant() + "*")
                                )
                            )
                        )
                    )
                );
            }

            if (searchResponse.IsValidResponse)
            {
                elasticTopDocs.TopDocs = searchResponse.Documents.ToList();
            }

            if (elasticTopDocs.TopDocs != null)
            {
                foreach (var doc in elasticTopDocs.TopDocs)
                {
                    results.Add(new ContentPickerResult
                    {
                        ContentItemId = doc["ContentItemId"].ToString(),
                        DisplayText = doc["Content.ContentItem.DisplayText.keyword"].ToString(),
                        HasPublished = doc["Content.ContentItem.Published"].ToString().Equals("true", StringComparison.OrdinalIgnoreCase)
                    });
                }
            }
        });

        return results.OrderBy(x => x.DisplayText);
    }
}
