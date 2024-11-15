namespace OrchardCore.Search.Elasticsearch;

public class ElasticsearchTopDocs
{
    public List<Dictionary<string, object>> TopDocs { get; set; }
    public List<Dictionary<string, object>> Fields { get; set; }
    public long Count { get; set; }
}
