using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Search.Elasticsearch;

public class ElasticsearchOptions
{
    public string IndexPrefix { get; set; }

    public Dictionary<string, JsonObject> Analyzers { get; } = [];

    public Dictionary<string, JsonObject> TokenFilters { get; } = [];

    private readonly Dictionary<string, ElasticsearchIndexSourceEntry> _indexSources = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyDictionary<string, ElasticsearchIndexSourceEntry> IndexSources
        => _indexSources;

    public void AddIndexSource(string source, Action<ElasticsearchIndexSourceEntry> configure = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(source);

        if (!_indexSources.TryGetValue(source, out var entry))
        {
            entry = new ElasticsearchIndexSourceEntry(source);
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

public sealed class ElasticsearchIndexSourceEntry
{
    public ElasticsearchIndexSourceEntry(string source)
    {
        Source = source;
    }

    public string Source { get; }

    public LocalizedString DisplayName { get; set; }

    public LocalizedString Description { get; set; }
}
