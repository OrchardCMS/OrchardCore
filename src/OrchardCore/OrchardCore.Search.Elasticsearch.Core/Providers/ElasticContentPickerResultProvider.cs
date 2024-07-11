using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.Search.Elasticsearch.Core.Models;
using OrchardCore.Search.Elasticsearch.Core.Services;

namespace OrchardCore.Search.Elasticsearch.Core.Providers
{
    public class ElasticContentPickerResultProvider : IContentPickerResultProvider
    {
        private readonly IElasticIndexManager _elasticIndexManager;
        private readonly ElasticConnectionOptions _elasticConnectionOptions;

        public ElasticContentPickerResultProvider(
            IOptions<ElasticConnectionOptions> elasticConnectionOptions,
            IElasticIndexManager elasticIndexManager)
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

            var elasticTopDocs = await _elasticIndexManager.SearchAsync(indexName, descriptor =>
                descriptor.Query(q => q
                            .Bool(b =>
                            {
                                b = b.Filter(f => f
                                    .Terms(t => t
                                        .Field("Content.ContentItem.ContentType")
                                        .Terms(searchContext.ContentTypes.ToArray())
                                    )
                                );
                                return string.IsNullOrWhiteSpace(searchContext.Query) ? b : b.Should(s => s
                                    .Wildcard(w => w
                                        .Field("Content.ContentItem.DisplayText.Normalized")
                                        .Wildcard(searchContext.Query.ToLowerInvariant() + "*")
                                    )
                                );
                            })
                        ));

            if (elasticTopDocs.TopDocs != null)
            {
                foreach (var doc in elasticTopDocs.TopDocs)
                {
                    results.Add(new ContentPickerResult
                    {
                        ContentItemId = doc["ContentItemId"].ToString(),
                        DisplayText = doc["Content.ContentItem.DisplayText.keyword"].ToString(),
                        HasPublished = doc["Content.ContentItem.Published"].ToString().ToLowerInvariant().Equals("true", StringComparison.Ordinal)
                    });
                }
            }

            return results.OrderBy(x => x.DisplayText);
        }
    }
}
