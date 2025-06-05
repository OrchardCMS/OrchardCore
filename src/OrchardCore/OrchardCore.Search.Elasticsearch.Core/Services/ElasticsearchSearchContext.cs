using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Clients.Elasticsearch.QueryDsl;
using OrchardCore.Indexing.Models;

namespace OrchardCore.Search.Elasticsearch.Core.Services;

public class ElasticsearchSearchContext
{
    public IndexProfile Index { get; }

    public Query Query { get; }

    public Highlight Highlight { get; set; }

    public ICollection<SortOptions> Sorts { get; set; }

    public int? From { get; set; }

    public int? Size { get; set; }

    public SourceConfig Source { get; set; }

    public ICollection<FieldAndFormat> Fields { get; set; }

    public ElasticsearchSearchContext(IndexProfile index, Query query)
    {
        ArgumentNullException.ThrowIfNull(index);
        ArgumentNullException.ThrowIfNull(query);

        Index = index;
        Query = query;
    }
}
