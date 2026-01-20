using System.Collections.Generic;

namespace OrchardCore.Search.Abstractions;

public class SearchContext
{
    public string Index { get; set; }

    public string Terms { get; set; }

    public int TotalHits { get; set; }

    public IEnumerable<string> ContentItemIds { get; set; }

    public ISearchService SearchService { get; set; }
}
