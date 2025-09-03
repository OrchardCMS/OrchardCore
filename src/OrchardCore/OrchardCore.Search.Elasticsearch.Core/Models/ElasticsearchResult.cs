using System.Text.Json.Nodes;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Aggregations;

namespace OrchardCore.Search.Elasticsearch;

public class ElasticsearchResult
{
    public IList<ElasticsearchRecord> TopDocs { get; set; }

    public IList<ElasticsearchRecord> Fields { get; set; }

    public long Count { get; set; }

    public long TotalCount { get; set; }

    public SearchResponse<JsonObject> Response { get; set; }
}

public class ElasticsearchRecord
{
    public JsonObject Value { get; }

    public IReadOnlyDictionary<string, IReadOnlyCollection<string>> Highlights { get; set; }

    public double? Score { get; set; }

    public ElasticsearchRecord(JsonObject value)
    {
        ArgumentNullException.ThrowIfNull(value);

        Value = value;
    }
}
