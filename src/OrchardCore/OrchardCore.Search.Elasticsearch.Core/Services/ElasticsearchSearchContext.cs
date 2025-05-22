using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Clients.Elasticsearch.QueryDsl;
using OrchardCore.Search.Elasticsearch.Core.Models;

namespace OrchardCore.Search.Elasticsearch.Core.Services;

public class ElasticsearchSearchContext
{
    public ElasticIndexSettings Settings { get; }

    public Query Query { get; }

    public Highlight Highlight { get; set; }

    public ICollection<SortOptions> Sorts { get; set; }

    public int? From { get; set; }

    public int? Size { get; set; }

    public SourceConfig Source { get; set; }

    public ICollection<FieldAndFormat> Fields { get; set; }

    public ElasticsearchSearchContext(ElasticIndexSettings settings, Query query)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(query);

        Settings = settings;
        Query = query;
    }
}
