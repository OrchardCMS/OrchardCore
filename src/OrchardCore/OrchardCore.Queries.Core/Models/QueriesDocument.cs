using OrchardCore.Data.Documents;

namespace OrchardCore.Queries.Core.Models;

public sealed class QueriesDocument : Document
{
    public Dictionary<string, Query> Queries { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
