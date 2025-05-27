namespace OrchardCore.Search.Elasticsearch.Core.Models;

public sealed class ElasticsearchDefaultQueryMetadata
{
    public string QueryAnalyzerName { get; set; }

    public string DefaultQuery { get; set; }

    public string[] DefaultSearchFields { get; set; }

    public string SearchType { get; set; }

    public string GetSearchType()
    {
        if (SearchType == null)
        {
            return ElasticsearchConstants.QueryStringSearchType;
        }

        return SearchType;
    }
}
