using System.Text.Json.Nodes;

namespace OrchardCore.Queries;

public sealed class UpdatingQueryContext : DataQueryContextBase
{
    public UpdatingQueryContext(Query query, JsonNode data = null)
        : base(query, data)
    {
    }
}
