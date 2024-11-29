using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Clients.Elasticsearch.QueryDsl;

namespace OrchardCore.Search.Elasticsearch.Core.Services;

public class ElasticsearchSearchContext
{
    public string IndexName { get; }

    public Query Query { get; }

    public Highlight Highlight { get; set; }

    public ICollection<SortOptions> Sorts { get; set; }

    public int? From { get; set; }

    public int? Size { get; set; }

    public SourceConfig Source { get; set; }

    public ICollection<FieldAndFormat> Fields { get; set; }

    public ElasticsearchSearchContext(string indexName, Query query)
    {
        ArgumentException.ThrowIfNullOrEmpty(indexName);
        ArgumentNullException.ThrowIfNull(query);

        IndexName = indexName;
        Query = query;
    }
}
