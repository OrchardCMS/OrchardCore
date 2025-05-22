using OrchardCore.Data.Documents;

namespace OrchardCore.Indexing.Core;

public sealed class ModelDocument<T> : Document
{
    public Dictionary<string, T> Records { get; init; } = [];
}
