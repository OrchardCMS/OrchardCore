using Microsoft.Extensions.Localization;
using OrchardCore.Indexing.Models;

namespace OrchardCore.Indexing.Core;

public sealed class IndexingOptions
{
    private readonly Dictionary<IndexProfileKey, IndexingOptionsEntry> _sources = new(IndexProfileKeyComparer.Instance);

    public IReadOnlyDictionary<IndexProfileKey, IndexingOptionsEntry> Sources
        => _sources;

    public void AddIndexingSource(string providerName, string type, Action<IndexingOptionsEntry> configure = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(providerName);
        ArgumentException.ThrowIfNullOrEmpty(type);

        var key = new IndexProfileKey(providerName, type);

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
    public IndexingOptionsEntry(IndexProfileKey key)
    {
        ProviderName = key.ProviderName;
        Type = key.Type;
    }

    public string ProviderName { get; }

    public string Type { get; }

    public LocalizedString DisplayName { get; set; }

    public LocalizedString Description { get; set; }
}

public sealed class IndexProfileKeyComparer : IEqualityComparer<IndexProfileKey>
{
    public static readonly IndexProfileKeyComparer Instance = new();

    public bool Equals(IndexProfileKey x, IndexProfileKey y)
    {
        return string.Equals(x.ProviderName, y.ProviderName, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(x.Type, y.Type, StringComparison.OrdinalIgnoreCase);
    }

    public int GetHashCode(IndexProfileKey obj)
        => HashCode.Combine(
            StringComparer.OrdinalIgnoreCase.GetHashCode(obj.ProviderName),
            StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Type)
        );
}
