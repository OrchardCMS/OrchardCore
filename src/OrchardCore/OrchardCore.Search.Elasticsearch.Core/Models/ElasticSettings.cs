using System;
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

    [Obsolete("This property will be removed in future releases.")]
    public const string StandardAnalyzer = "standardanalyzer";

    [Obsolete($"This property will be removed in future releases. Instead use {nameof(SearchType)} property.")]
    public bool AllowElasticQueryStringQueryInSearch { get; set; } = false;

    public string GetSearchType()
    {
#pragma warning disable CS0618 // Type or member is obsolete
        if (SearchType == null && AllowElasticQueryStringQueryInSearch)
        {
            return QueryStringSearchType;
        }
#pragma warning restore CS0618 // Type or member is obsolete

        return SearchType;
    }
}
