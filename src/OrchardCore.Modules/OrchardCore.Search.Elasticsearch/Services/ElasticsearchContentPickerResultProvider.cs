using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nest;
using OrchardCore.ContentManagement;
using OrchardCore.Search.Elasticsearch.Settings;

namespace OrchardCore.Search.Elasticsearch
{
    public class ElasticsearchContentPickerResultProvider : IContentPickerResultProvider
    {
        private readonly ElasticsearchIndexManager _elasticIndexManager;

        public ElasticsearchContentPickerResultProvider(ElasticsearchIndexManager elasticIndexManager)
        {
            _elasticIndexManager = elasticIndexManager;
        }

        public string Name => "Elasticsearch";

        public async Task<IEnumerable<ContentPickerResult>> Search(ContentPickerSearchContext searchContext)
        {
            string indexName = null;

            var fieldSettings = searchContext.PartFieldDefinition?.GetSettings<ContentPickerFieldElasticsearchEditorSettings>();

            if (!String.IsNullOrWhiteSpace(fieldSettings?.Index))
            {
                indexName = fieldSettings.Index;
            }

            if (indexName != null && !await _elasticIndexManager.Exists(indexName))
            {
                return new List<ContentPickerResult>();
            }

            var results = new List<ContentPickerResult>();

            await _elasticIndexManager.SearchAsync(indexName, async elasticClient =>
            {
                ISearchResponse<Dictionary<string, object>> searchResponse = null;
                var elasticTopDocs = new ElasticsearchTopDocs();

                if (String.IsNullOrWhiteSpace(searchContext.Query))
                {
                    searchResponse = await elasticClient.SearchAsync<Dictionary<string, object>>(s => s
                        .Index(indexName)
                        .Query(q => q
                            .Bool(b => b
                                .Filter(f => f
                                    .Terms(t => t
                                        .Field("Content.ContentItem.ContentType.keyword")
                                        .Terms(searchContext.ContentTypes.ToArray())
                                    )
                                )
                            )
                        )
                    );
                }
                else
                {
                    searchResponse = await elasticClient.SearchAsync<Dictionary<string, object>>(s => s
                        .Index(indexName)
                        .Query(q => q
                            .Bool(b => b
                                .Filter(f => f
                                    .Terms(t => t
                                        .Field("Content.ContentItem.ContentType.keyword")
                                        .Terms(searchContext.ContentTypes.ToArray())
                                    )
                                )
                                .Should(s => s
                                    .Wildcard(w => w
                                        .Field("Content.ContentItem.DisplayText_Normalized")
                                        .Wildcard(searchContext.Query.ToLowerInvariant() + "*")
                                    )
                                )
                            )
                        )
                    );
                }

                if (searchResponse.IsValid)
                {
                    elasticTopDocs.TopDocs = searchResponse.Documents.ToList();
                }

                foreach (var doc in elasticTopDocs.TopDocs)
                {
                    results.Add(new ContentPickerResult
                    {
                        ContentItemId = doc["ContentItemId"].ToString(),
                        DisplayText = doc["Content.ContentItem.DisplayText"].ToString(),
                        HasPublished = doc["Content.ContentItem.Published"].ToString().ToLower() == "true"
                    });
                }
            });

            return results.OrderBy(x => x.DisplayText);
        }
    }
}
