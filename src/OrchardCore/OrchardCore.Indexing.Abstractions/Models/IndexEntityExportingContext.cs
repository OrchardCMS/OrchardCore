using System.Text.Json.Nodes;

namespace OrchardCore.Indexing.Models;

public sealed class IndexEntityExportingContext
{
    public IndexEntity Index { get; }

    public JsonObject Data { get; }


    public IndexEntityExportingContext(IndexEntity index, JsonObject data)
    {
        ArgumentNullException.ThrowIfNull(index);
        ArgumentNullException.ThrowIfNull(data);

        Index = index;
        Data = data;
    }
}

