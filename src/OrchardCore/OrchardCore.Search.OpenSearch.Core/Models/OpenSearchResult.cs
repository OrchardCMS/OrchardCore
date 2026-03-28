using System.Text.Json.Nodes;

namespace OrchardCore.Search.OpenSearch;

public class OpenSearchResult
{
    public IList<OpenSearchRecord> TopDocs { get; set; }

    public IList<OpenSearchRecord> Fields { get; set; }

    public long Count { get; set; }

    public long TotalCount { get; set; }
}

public class OpenSearchRecord
{
    public JsonObject Value { get; }

    public IReadOnlyDictionary<string, IReadOnlyCollection<string>> Highlights { get; set; }

    public double? Score { get; set; }

    public OpenSearchRecord(JsonObject value)
    {
        ArgumentNullException.ThrowIfNull(value);

        Value = value;
    }
}
