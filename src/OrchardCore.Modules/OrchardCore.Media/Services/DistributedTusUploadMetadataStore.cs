using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Media.Services;

/// <summary>
/// Distributed TUS upload metadata store that uses
/// <see cref="IDistributedCache"/> to share upload metadata across servers.
/// Falls back to in-memory when no external cache (e.g. Redis) is configured.
/// </summary>
public sealed class DistributedTusUploadMetadataStore
{
    private readonly IDistributedCache _cache;
    private readonly string _keyPrefix;
    private readonly IOptions<MediaOptions> _mediaOptions;

    public DistributedTusUploadMetadataStore(
        IDistributedCache cache,
        ShellSettings shellSettings,
        IOptions<MediaOptions> mediaOptions)
    {
        _cache = cache;
        _mediaOptions = mediaOptions;
        _keyPrefix = $"tus:uploadmeta:{shellSettings.TenantId}:";
    }

    public async Task SetAsync(string uploadId, TusUploadEntry entry, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(entry);
        var options = new DistributedCacheEntryOptions
        {
            SlidingExpiration = _mediaOptions.Value.TemporaryFileLifetime,
        };

        await _cache.SetStringAsync(_keyPrefix + uploadId, json, options, cancellationToken);
    }

    public async Task<TusUploadEntry> GetAsync(string uploadId, CancellationToken cancellationToken = default)
    {
        var json = await _cache.GetStringAsync(_keyPrefix + uploadId, cancellationToken);
        return json != null ? JsonSerializer.Deserialize<TusUploadEntry>(json) : null;
    }

    public async Task<TusUploadEntry> RemoveAsync(string uploadId, CancellationToken cancellationToken = default)
    {
        var entry = await GetAsync(uploadId, cancellationToken);
        if (entry != null)
        {
            await _cache.RemoveAsync(_keyPrefix + uploadId, cancellationToken);
        }

        return entry;
    }
}
