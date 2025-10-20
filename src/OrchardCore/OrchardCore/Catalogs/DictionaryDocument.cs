using OrchardCore.Data.Documents;

namespace OrchardCore.Catalogs;

public sealed class DictionaryDocument<T> : Document
{
    public Dictionary<string, T> Records { get; init; } = [];
}
