using Microsoft.Extensions.Localization;

namespace OrchardCore.Search.AzureAI.Models;

public class AzureAISearchOptions
{
    private readonly Dictionary<string, AzureAIIndexSourceEntry> _indexSources = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyDictionary<string, AzureAIIndexSourceEntry> IndexSources
        => _indexSources;

    public void AddIndexSource(string source, Action<AzureAIIndexSourceEntry> configure = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(source);

        if (!_indexSources.TryGetValue(source, out var entry))
        {
            entry = new AzureAIIndexSourceEntry(source);
        }

        if (configure != null)
        {
            configure(entry);
        }

        if (string.IsNullOrEmpty(entry.DisplayName))
        {
            entry.DisplayName = new LocalizedString(source, source);
        }

        _indexSources[source] = entry;
    }
}

public sealed class AzureAIIndexSourceEntry
{
    public AzureAIIndexSourceEntry(string source)
    {
        Source = source;
    }

    public string Source { get; }

    public LocalizedString DisplayName { get; set; }

    public LocalizedString Description { get; set; }
}
