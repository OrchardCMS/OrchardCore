using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Search.AzureAI.Models;

namespace OrchardCore.Search.AzureAI.Services;

public sealed class AzureAISearchIndexNameService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ShellSettings _shellSettings;
    private readonly AzureAISearchDefaultOptions _azureAIOptions;
    private readonly string _prefixCacheKey;

    public AzureAISearchIndexNameService(
        IMemoryCache memoryCache,
        ShellSettings shellSettings,
        IOptions<AzureAISearchDefaultOptions> azureAIOptions)
    {
        _memoryCache = memoryCache;
        _shellSettings = shellSettings;
        _azureAIOptions = azureAIOptions.Value;
        _prefixCacheKey = $"AzureAISearchIndexesPrefix_{shellSettings.Name}";
    }

    public string GetFullIndexName(string indexName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(indexName);

        return GetIndexPrefix() + '-' + indexName;
    }

    private string GetIndexPrefix()
    {
        if (!_memoryCache.TryGetValue<string>(_prefixCacheKey, out var value))
        {
            var prefix = _shellSettings.Name.ToLowerInvariant();

            if (!string.IsNullOrWhiteSpace(_azureAIOptions.IndexesPrefix))
            {
                prefix = $"{_azureAIOptions.IndexesPrefix.ToLowerInvariant()}-{prefix}";
            }

            if (AzureAISearchIndexNamingHelper.TryGetSafePrefix(prefix, out var safePrefix))
            {
                value = safePrefix;
                _memoryCache.Set(_prefixCacheKey, safePrefix);
            }
            else
            {
                throw new InvalidOperationException($"Unable to create a safe index prefix for AI Search. Attempted to created a safe name using '{safePrefix}'.");
            }
        }

        return value ?? string.Empty;
    }
}
