using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Indexing;

namespace OrchardCore.Search.Elasticsearch.Core.Services;

public sealed class ElasticsearchIndexNameProvider : IIndexNameProvider
{
    private const string _separator = "_";

    private static readonly List<char> _charsToRemove =
        ['\\', '/', '*', '\"', '|', '<', '>', '`', '\'', ' ', '#', ':', '.'];

    private readonly IMemoryCache _memoryCache;
    private readonly ShellSettings _shellSettings;
    private readonly ElasticsearchOptions _elasticsearchOptions;
    private readonly string _prefixCacheKey;

    public ElasticsearchIndexNameProvider(
        IMemoryCache memoryCache,
        ShellSettings shellSettings,
        IOptions<ElasticsearchOptions> azureAIOptions)
    {
        _memoryCache = memoryCache;
        _shellSettings = shellSettings;
        _elasticsearchOptions = azureAIOptions.Value;
        _prefixCacheKey = $"ElasticsearchIndexesPrefix_{shellSettings.Name}";
    }

    public string GetFullIndexName(string indexName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(indexName);

        return $"{GetIndexPrefix()}{_separator}{ToSafeIndexName(indexName)}";
    }

    private string GetIndexPrefix()
    {
        if (!_memoryCache.TryGetValue<string>(_prefixCacheKey, out var value))
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(_elasticsearchOptions.IndexPrefix))
            {
                parts.Add(_elasticsearchOptions.IndexPrefix.ToLowerInvariant());
            }

            parts.Add(_shellSettings.Name.ToLowerInvariant());

            value = string.Join(_separator, parts);
            _memoryCache.Set(_prefixCacheKey, value);
        }

        return value ?? string.Empty;
    }

    /// <summary>
    /// Makes sure that the index names are compliant with Elasticsearch specifications.
    /// <see href="https://www.elastic.co/guide/en/elasticsearch/reference/current/indices-create-index.html#indices-create-api-path-params"/>.
    /// </summary>
    public static string ToSafeIndexName(string indexName)
    {
        ArgumentException.ThrowIfNullOrEmpty(indexName);

        indexName = indexName.ToLowerInvariant();

        if (indexName[0] == '-' || indexName[0] == '_' || indexName[0] == '+' || indexName[0] == '.')
        {
            indexName = indexName.Remove(0, 1);
        }

        _charsToRemove.ForEach(c => indexName = indexName.Replace(c.ToString(), string.Empty));

        return indexName;
    }
}
