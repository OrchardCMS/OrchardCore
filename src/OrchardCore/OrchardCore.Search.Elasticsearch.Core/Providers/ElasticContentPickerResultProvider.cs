using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nest;
using OrchardCore.ContentManagement;
using OrchardCore.Environment.Shell;
using OrchardCore.Search.Elasticsearch.Core.Models;
using OrchardCore.Search.Elasticsearch.Core.Services;

namespace OrchardCore.Search.Elasticsearch.Core.Providers
{
    public class ElasticContentPickerResultProvider : IContentPickerResultProvider
    {
        private readonly ElasticIndexManager _elasticIndexManager;
        private readonly string _indexPrefix;

        public ElasticContentPickerResultProvider(
            ShellSettings shellSettings,
            ElasticIndexManager elasticIndexManager)
        {
            _indexPrefix = shellSettings.Name.ToLowerInvariant() + "_";
            _elasticIndexManager = elasticIndexManager;
        }

        public string Name => "Elasticsearch";

        public async Task<IEnumerable<ContentPickerResult>> Search(ContentPickerSearchContext searchContext)
        {
            string indexName = null;

            var fieldSettings = searchContext.PartFieldDefinition?.GetSettings<ContentPickerFieldElasticEditorSettings>();

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
                var elasticTopDocs = new ElasticTopDocs();

                if (String.IsNullOrWhiteSpace(searchContext.Query))
                {
                    searchResponse = await elasticClient.SearchAsync<Dictionary<string, object>>(s => s
                        .Index(_indexPrefix + indexName)
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
                    searchResponse = await elasticClient.SearchAsync<Dictionary<string, object>>(s => s
                        .Index(_indexPrefix + indexName)
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

                if (elasticTopDocs.TopDocs != null)
                {
                    foreach (var doc in elasticTopDocs.TopDocs)
                    {
                        results.Add(new ContentPickerResult
                        {
                            ContentItemId = doc["ContentItemId"].ToString(),
                            DisplayText = doc["Content.ContentItem.DisplayText.keyword"].ToString(),
                            HasPublished = doc["Content.ContentItem.Published"].ToString().ToLower() == "true"
                        });
                    }
                }
            });

            return results.OrderBy(x => x.DisplayText);
        }
    }
}
