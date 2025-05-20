namespace OrchardCore.Search.Elasticsearch.Models;

public class ElasticsearchIndexRemoveContext(string indexName, string indexFullName)
{
    public string IndexName { get; } = indexName;

    public string IndexFullName { get; } = indexFullName;
}
