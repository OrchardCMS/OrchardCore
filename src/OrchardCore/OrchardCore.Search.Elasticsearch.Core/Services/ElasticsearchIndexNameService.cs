using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Search.Elasticsearch.Core.Services;

public sealed class ElasticsearchIndexNameService
{
    private const string _separator = "_";

    private readonly IMemoryCache _memoryCache;
    private readonly ShellSettings _shellSettings;
    private readonly ElasticsearchOptions _elasticsearchOptions;
    private readonly string _prefixCacheKey;

    public ElasticsearchIndexNameService(
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

        return $"{GetIndexPrefix()}{_separator}{indexName}";
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
}
