namespace OrchardCore.Search.OpenSearch.Core.Models;

public sealed class OpenSearchDefaultQueryMetadata
{
    public string QueryAnalyzerName { get; set; }

    public string DefaultQuery { get; set; }

    public string[] DefaultSearchFields { get; set; }

    public string SearchType { get; set; }

    public string GetSearchType()
    {
        if (SearchType == null)
        {
            return OpenSearchConstants.QueryStringSearchType;
        }

        return SearchType;
    }
}
