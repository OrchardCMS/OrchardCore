using Microsoft.Extensions.Options;
using OpenSearch.Client;
using OrchardCore.ContentManagement;
using OrchardCore.Indexing;
using OrchardCore.Indexing.Core;
using OrchardCore.OpenSearch.Core.Models;
using OrchardCore.OpenSearch.Core.Services;
using OrchardCore.OpenSearch.Models;

namespace OrchardCore.OpenSearch.Core.Providers;

public class OpenSearchContentPickerResultProvider : IContentPickerResultProvider
{
    private readonly IIndexProfileStore _indexProfileStore;
    private readonly OpenSearchIndexManager _indexManager;
    private readonly OpenSearchConnectionOptions _openSearchConnectionOptions;

    public OpenSearchContentPickerResultProvider(
        IIndexProfileStore indexProfileStore,
        IOptions<OpenSearchConnectionOptions> openSearchConnectionOptions,
        OpenSearchIndexManager indexManager)
    {
        _openSearchConnectionOptions = openSearchConnectionOptions.Value;
        _indexProfileStore = indexProfileStore;
        _indexManager = indexManager;
    }

    public string Name { get; } = OpenSearchConstants.ProviderName;

    public async Task<IEnumerable<ContentPickerResult>> Search(ContentPickerSearchContext searchContext)
    {
        if (!_openSearchConnectionOptions.ConfigurationExists())
        {
            return [];
        }

        var fieldSettings = searchContext.PartFieldDefinition?.GetSettings<ContentPickerFieldOpenSearchEditorSettings>();

        if (string.IsNullOrWhiteSpace(fieldSettings?.Index))
        {
            return [];
        }

        var index = await _indexProfileStore.FindByIndexNameAndProviderAsync(fieldSettings.Index, OpenSearchConstants.ProviderName);

        if (index is null || index.Type != IndexingConstants.ContentsIndexSource || !await _indexManager.ExistsAsync(index.IndexFullName))
        {
            return [];
        }

        var results = new List<ContentPickerResult>();

        await _indexManager.SearchAsync(index, async client =>
        {
            ISearchResponse<System.Text.Json.Nodes.JsonObject> searchResponse;

            if (string.IsNullOrWhiteSpace(searchContext.Query))
            {
                searchResponse = await client.SearchAsync<System.Text.Json.Nodes.JsonObject>(s => s
                    .Index(index.IndexFullName)
                    .Query(q => q
                        .Bool(b => b
                            .Filter(f => f
                                .Terms(t => t
                                    .Field("Content.ContentItem.ContentType")
                                    .Terms(searchContext.ContentTypes.ToArray())
                                )
                            )
                        )
                    )
                );
            }
            else
            {
                searchResponse = await client.SearchAsync<System.Text.Json.Nodes.JsonObject>(s => s
                    .Index(index.IndexFullName)
                    .Query(q => q
                        .Bool(b => b
                            .Filter(f => f
                                .Terms(t => t
                                    .Field("Content.ContentItem.ContentType")
                                    .Terms(searchContext.ContentTypes.ToArray())
                                )
                            )
                            .Should(s => s
                                .Wildcard(w => w
                                    .Field("Content.ContentItem.DisplayText.Normalized")
                                    .Value(searchContext.Query.ToLowerInvariant() + "*")
                                )
                            )
                        )
                    )
                );
            }

            if (searchResponse.IsValid && searchResponse.Documents != null)
            {
                foreach (var doc in searchResponse.Documents)
                {
                    var result = new ContentPickerResult();

                    if (doc.TryGetPropertyValue(nameof(ContentItem.ContentItemId), out var contentItemId))
                    {
                        result.ContentItemId = contentItemId.GetValue<string>();
                    }

                    if (doc.TryGetPropertyValue("Content.ContentItem.DisplayText.keyword", out var keyword))
                    {
                        result.DisplayText = keyword.GetValue<string>();
                    }

                    if (doc.TryGetPropertyValue("Content.ContentItem.Published", out var published) && published is System.Text.Json.Nodes.JsonValue publishedValue && publishedValue.TryGetValue<bool>(out var hasPublished))
                    {
                        result.HasPublished = hasPublished;
                    }

                    results.Add(result);
                }
            }
        });

        return results.OrderBy(x => x.DisplayText);
    }
}
