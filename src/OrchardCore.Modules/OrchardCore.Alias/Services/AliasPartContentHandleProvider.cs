using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Alias.Handlers;
using OrchardCore.Alias.Indexes;
using OrchardCore.ContentManagement;
using OrchardCore.Environment.Cache;
using YesSql;

namespace OrchardCore.Alias.Services;

public class AliasPartContentHandleProvider : IContentHandleProvider, ITagRemovedEventHandler
{
    private const string CacheKeyPrefix = "AliasPartContentHandleProvider_";

    private static readonly DistributedCacheEntryOptions _defaultCacheOptions = new()
    {
        SlidingExpiration = TimeSpan.FromHours(1),
    };

    private readonly ISession _session;
    private readonly IDistributedCache _distributedCache;
    private readonly IMemoryCache _memoryCache;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;

    private ITagCache _tagCache;

    public AliasPartContentHandleProvider(
        ISession session,
        IDistributedCache distributedCache,
        IMemoryCache memoryCache,
        IServiceProvider serviceProvider,
        ILogger<AliasPartContentHandleProvider> logger)
    {
        _session = session;
        _distributedCache = distributedCache;
        _memoryCache = memoryCache;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public int Order => 100;

    public async Task<string> GetContentItemIdAsync(string handle)
    {
        if (!handle.StartsWith(AliasConstants.AliasPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var alias = handle[AliasConstants.AliasPrefix.Length..];
        var normalizedAlias = alias.ToLowerInvariant();
        var cacheKey = GetCacheKey(normalizedAlias);

        // Try to get from memory cache first.
        if (_memoryCache.TryGetValue(cacheKey, out string contentItemId))
        {
            return NullIfEmpty(contentItemId);
        }

        // Try to get from distributed cache.
        try
        {
            contentItemId = await _distributedCache.GetStringAsync(cacheKey);

            if (contentItemId is not null)
            {
                // Cache in memory for faster subsequent lookups.
                _memoryCache.Set(cacheKey, contentItemId, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(5),
                });

                return NullIfEmpty(contentItemId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read the alias '{Alias}' from the distributed cache.", alias);
        }

        // Query the database.
        var aliasPartIndex = await AliasPartContentHandleHelper.QueryAliasIndex(_session, alias);
        contentItemId = aliasPartIndex?.ContentItemId;

        // Cache the result (including null/empty results to avoid repeated database queries).
        var tag = GetTag(normalizedAlias);
        await CacheResultAsync(cacheKey, tag, contentItemId);

        return contentItemId;
    }

    /// <summary>
    /// Handles cache eviction when a tag is removed.
    /// This is called by <see cref="ITagCache"/> when <see cref="AliasPartHandler"/> removes the alias tag.
    /// </summary>
    public Task TagRemovedAsync(string tag, IEnumerable<string> keys)
    {
        if (!tag.StartsWith(AliasConstants.AliasPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return Task.CompletedTask;
        }

        // Remove from memory cache for immediate eviction within this instance.
        foreach (var key in keys)
        {
            _memoryCache.Remove(key);
        }

        return Task.CompletedTask;
    }

    private async Task CacheResultAsync(string cacheKey, string tag, string contentItemId)
    {
        // Use empty string as a placeholder for null to distinguish between "not cached" and "cached as null".
        var cacheValue = contentItemId ?? string.Empty;

        // Cache in memory.
        _memoryCache.Set(cacheKey, cacheValue, new MemoryCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(5),
        });

        // Cache in distributed cache and tag for eviction.
        try
        {
            await _distributedCache.SetStringAsync(cacheKey, cacheValue, _defaultCacheOptions);

            // Lazy load ITagCache to prevent circular dependency.
            _tagCache ??= _serviceProvider.GetRequiredService<ITagCache>();
            await _tagCache.TagAsync(cacheKey, tag);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write the alias cache key '{CacheKey}' to the distributed cache.", cacheKey);
        }
    }

    private static string GetCacheKey(string normalizedAlias)
        => CacheKeyPrefix + normalizedAlias;

    private static string GetTag(string normalizedAlias)
        => AliasConstants.AliasPrefix + normalizedAlias;

    private static string NullIfEmpty(string value)
        => string.IsNullOrEmpty(value) ? null : value;
}

internal sealed class AliasPartContentHandleHelper
{
#pragma warning disable CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons
    internal static Task<ContentItem> QueryAliasIndex(ISession session, string alias) =>
        session.Query<ContentItem, AliasPartIndex>(x => x.Alias == alias.ToLowerInvariant()).FirstOrDefaultAsync();
#pragma warning restore CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons
}
