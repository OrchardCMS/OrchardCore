using Elastic.Clients.Elasticsearch;
using OrchardCore.Indexing.Models;

namespace OrchardCore.Search.Elasticsearch.Core.Services;

public class ElasticsearchSearchContext
{
    public IndexProfile IndexProfile { get; }

    public SearchRequest SearchRequest { get; }

    public ElasticsearchSearchContext(IndexProfile indexProfile, SearchRequest searchRequest)
    {
        ArgumentNullException.ThrowIfNull(indexProfile);
        ArgumentNullException.ThrowIfNull(searchRequest);

        IndexProfile = indexProfile;
        SearchRequest = searchRequest;
    }
}
