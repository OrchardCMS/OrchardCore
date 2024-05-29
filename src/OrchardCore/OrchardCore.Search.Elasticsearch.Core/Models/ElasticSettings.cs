using OrchardCore.Contents.Indexing;

namespace OrchardCore.Search.Elasticsearch.Core.Models;

public class ElasticSettings
{
    public const string CustomSearchType = "custom";

    public const string QueryStringSearchType = "query_string";

    public static readonly string[] FullTextField = [IndexingConstants.FullTextKey];

    public string SearchIndex { get; set; }

    public string DefaultQuery { get; set; }

    public string[] DefaultSearchFields { get; set; } = FullTextField;

    public string SearchType { get; set; }

    public string GetSearchType()
    {
        if (SearchType == null)
        {
            return QueryStringSearchType;
        }

        return SearchType;
    }
}
