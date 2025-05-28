using Microsoft.Extensions.Localization;
using OrchardCore.Indexing.Models;

namespace OrchardCore.Indexing.Core;

public sealed class IndexingOptions
{
    private readonly Dictionary<IndexEntityKey, IndexingOptionsEntry> _sources = new(IndexEntityKeyComparer.Instance);

    public IReadOnlyDictionary<IndexEntityKey, IndexingOptionsEntry> Sources
        => _sources;

    public void AddIndexingSource(string providerName, string type, Action<IndexingOptionsEntry> configure = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(providerName);
        ArgumentException.ThrowIfNullOrEmpty(type);

        var key = new IndexEntityKey(providerName, type);

        if (!_sources.TryGetValue(key, out var entry))
        {
            entry = new IndexingOptionsEntry(key);
        }

        if (configure != null)
        {
            configure(entry);
        }

        if (string.IsNullOrEmpty(entry.DisplayName))
        {
            entry.DisplayName = new LocalizedString(providerName, providerName);
        }

        _sources[key] = entry;
    }
}

public sealed class IndexingOptionsEntry
{
    public IndexingOptionsEntry(IndexEntityKey key)
    {
        ProviderName = key.ProviderName;
        Type = key.Type;
    }

    public string ProviderName { get; }

    public string Type { get; }

    public LocalizedString DisplayName { get; set; }

    public LocalizedString Description { get; set; }
}

public sealed class IndexEntityKeyComparer : IEqualityComparer<IndexEntityKey>
{
    public static readonly IndexEntityKeyComparer Instance = new();

    public bool Equals(IndexEntityKey x, IndexEntityKey y)
    {
        return string.Equals(x.ProviderName, y.ProviderName, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(x.Type, y.Type, StringComparison.OrdinalIgnoreCase);
    }

    public int GetHashCode(IndexEntityKey obj)
        => HashCode.Combine(
            StringComparer.OrdinalIgnoreCase.GetHashCode(obj.ProviderName),
            StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Type)
        );
}
