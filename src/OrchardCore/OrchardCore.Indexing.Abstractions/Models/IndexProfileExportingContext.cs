using System.Text.Json.Nodes;

namespace OrchardCore.Indexing.Models;

public sealed class IndexProfileExportingContext
{
    public IndexProfile IndexProfile { get; }

    public JsonObject Data { get; }


    public IndexProfileExportingContext(IndexProfile indexProfile, JsonObject data)
    {
        ArgumentNullException.ThrowIfNull(indexProfile);
        ArgumentNullException.ThrowIfNull(data);

        IndexProfile = indexProfile;
        Data = data;
    }
}

