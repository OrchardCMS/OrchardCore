using OrchardCore.Data.Documents;

namespace OrchardCore.Queries.Core.Models;

public sealed class QueriesDocument : Document
{
    public IDictionary<string, Query> Queries { get; init; } = new Dictionary<string, Query>(StringComparer.OrdinalIgnoreCase);
}
