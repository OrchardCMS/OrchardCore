namespace OrchardCore.Search.Abstractions;

public class SearchResult
{
    public IList<string> ContentItemIds { get; set; }

    public Dictionary<string, IReadOnlyDictionary<string, IReadOnlyCollection<string>>> Highlights { get; set; }

    public bool Latest { get; set; }

    public bool Success { get; set; }
}
