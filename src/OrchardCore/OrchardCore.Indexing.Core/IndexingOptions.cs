using Microsoft.Extensions.Localization;
using OrchardCore.Indexing.Models;

namespace OrchardCore.Indexing.Core;

public sealed class IndexingOptions
{
    private readonly Dictionary<IndexProfileKey, IndexingOptionsEntry> _sources = new(IndexProfileKeyComparer.Instance);
    private readonly Dictionary<string, IndexingProviderOptionsEntry> _providers = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyDictionary<IndexProfileKey, IndexingOptionsEntry> Sources
        => _sources;

    public IReadOnlyDictionary<string, IndexingProviderOptionsEntry> Providers
        => _providers;

    public void AddIndexingSource(
        string providerName,
        string type,
        Action<IndexingOptionsEntry> configure = null,
        Action<IndexingProviderOptionsEntry> configureProvider = null)
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

        AddIndexingProvider(providerName, configureProvider);
    }

    /// <summary>
    /// Returns whether each server keeps its own copy of the indexes of a provider, see <see cref="IndexingProviderOptionsEntry.IsLocal"/>.
    /// </summary>
    /// <param name="providerName">The name of the provider.</param>
    public bool IsLocalProvider(string providerName)
        => !string.IsNullOrEmpty(providerName) && _providers.TryGetValue(providerName, out var provider) && provider.IsLocal;

    public void AddIndexingProvider(string providerName, Action<IndexingProviderOptionsEntry> configure = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(providerName);

        if (!_providers.TryGetValue(providerName, out var entry))
        {
            entry = new IndexingProviderOptionsEntry(providerName);
        }

        configure?.Invoke(entry);

        if (string.IsNullOrEmpty(entry.DisplayName))
        {
            entry.DisplayName = new LocalizedString(providerName, providerName);
        }

        _providers[providerName] = entry;
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

public sealed class IndexingProviderOptionsEntry
{
    public IndexingProviderOptionsEntry(string providerName)
    {
        ProviderName = providerName;
    }

    public string ProviderName { get; }

    public LocalizedString DisplayName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether each server keeps its own copy of the indexes of this provider, such
    /// as on its disk, instead of all the servers sharing one index in a remote service. Every server then keeps its
    /// copy up to date, while a shared index is updated by a single server at a time. It is <see langword="false"/>
    /// by default.
    /// </summary>
    public bool IsLocal { get; set; }
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
