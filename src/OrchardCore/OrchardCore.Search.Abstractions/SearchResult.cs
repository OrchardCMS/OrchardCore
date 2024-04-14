using System.Collections.Generic;

namespace OrchardCore.Search.Abstractions;

public class SearchResult
{
    public IList<string> ContentItemIds { get; set; }

    public bool Latest { get; set; }

    public bool Success { get; set; }
}
