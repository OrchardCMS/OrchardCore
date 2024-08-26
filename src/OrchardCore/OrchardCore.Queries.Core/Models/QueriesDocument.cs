using OrchardCore.Data.Documents;
using OrchardCore.Modules;

namespace OrchardCore.Queries.Core.Models;

public sealed class QueriesDocument : Document
{
    private readonly Dictionary<string, Query> _queries = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, Query> Queries
    {
        get => _queries;
        set => _queries.SetItems(value);
    }
}
