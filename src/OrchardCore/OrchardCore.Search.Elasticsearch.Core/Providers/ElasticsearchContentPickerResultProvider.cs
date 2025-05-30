using System.Text.Json.Nodes;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Json.Path;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.Indexing;
using OrchardCore.Search.Elasticsearch.Core.Models;
using OrchardCore.Search.Elasticsearch.Core.Services;

namespace OrchardCore.Search.Elasticsearch.Core.Providers;

public class ElasticsearchContentPickerResultProvider : IContentPickerResultProvider
{
    private readonly IIndexEntityStore _indexEntityStore;
    private readonly ElasticsearchIndexManager _indexManager;
    private readonly ElasticsearchConnectionOptions _elasticConnectionOptions;

    public ElasticsearchContentPickerResultProvider(
        IIndexEntityStore indexEntityStore,
        IOptions<ElasticsearchConnectionOptions> elasticConnectionOptions,
        ElasticsearchIndexManager indexManager)
    {
        _elasticConnectionOptions = elasticConnectionOptions.Value;
        _indexEntityStore = indexEntityStore;
        _indexManager = indexManager;
    }

    public string Name => "Elasticsearch";

    public async Task<IEnumerable<ContentPickerResult>> Search(ContentPickerSearchContext searchContext)
    {
        if (!_elasticConnectionOptions.ConfigurationExists())
        {
            return [];
        }

        var fieldSettings = searchContext.PartFieldDefinition?.GetSettings<ContentPickerFieldElasticEditorSettings>();

        if (string.IsNullOrWhiteSpace(fieldSettings?.Index))
        {
            return [];
        }

        var index = await _indexEntityStore.FindByNameAndProviderAsync(ElasticsearchConstants.ProviderName, fieldSettings.Index);

        if (index is null || !await _indexManager.ExistsAsync(index.IndexFullName))
        {
            return [];
        }

        var results = new List<ContentPickerResult>();

        await _indexManager.SearchAsync(index, async elasticClient =>
        {
            SearchResponse<JsonObject> searchResponse = null;
            var elasticTopDocs = new ElasticsearchResult();

            var valuesQuery = new TermsQueryField(searchContext.ContentTypes.Select(contentType => FieldValue.String(contentType)).ToArray());

            if (string.IsNullOrWhiteSpace(searchContext.Query))
            {
                searchResponse = await elasticClient.SearchAsync<JsonObject>(s => s
                    .Index(index.IndexFullName)
                    .Query(q => q
                        .Bool(b => b
                            .Filter(f => f
                                .Terms(t => t
                                    .Field("Content.ContentItem.ContentType")
                                    .Terms(valuesQuery)
                                )
                            )
                        )
                    )
                );
            }
            else
            {
                searchResponse = await elasticClient.SearchAsync<JsonObject>(s => s
                    .Index(index.IndexFullName)
                    .Query(q => q
                        .Bool(b => b
                            .Filter(f => f
                                .Terms(t => t
                                    .Field("Content.ContentItem.ContentType")
                                    .Terms(valuesQuery)
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
                elasticTopDocs.TopDocs = searchResponse.Documents.Select(doc => new ElasticsearchRecord(doc)).ToList();
            }

            if (elasticTopDocs.TopDocs != null)
            {
                foreach (var doc in elasticTopDocs.TopDocs)
                {
                    var result = new ContentPickerResult();

                    if (doc.Value.TryGetPropertyValue(nameof(ContentItem.ContentItemId), out var contentItemId))
                    {
                        result.ContentItemId = contentItemId.GetValue<string>();
                    }

                    if (doc.Value.TryGetPropertyValue("Content.ContentItem.DisplayText.keyword", out var keyword))
                    {
                        result.DisplayText = keyword.GetValue<string>();
                    }

                    if (doc.Value.TryGetPropertyValue("Content.ContentItem.Published", out var published) && published.TryGetValue<bool>(out var hasPublished))
                    {
                        result.HasPublished = hasPublished;
                    }
                }
            }
        });

        return results.OrderBy(x => x.DisplayText);
    }
}
