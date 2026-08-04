using System.IO.Pipelines;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Locking;
using OrchardCore.Locking.Distributed;
using OrchardCore.Modules;
using tusdotnet.Interfaces;
using tusdotnet.Models;

namespace OrchardCore.Media.Services;

/// <summary>
/// Distributed TUS store that composes <see cref="ITusTempStore"/> for file data
/// and <see cref="IDistributedCache"/> for metadata coordination across servers.
/// </summary>
public sealed class DistributedMediaTusStore :
    ITusStore,
    ITusPipelineStore,
    ITusCreationStore,
    ITusTerminationStore,
    ITusExpirationStore
{
    private readonly ITusTempStore _tempStore;
    private readonly IDistributedCache _cache;
    private readonly ILock _lock;
    private readonly IClock _clock;
    private readonly ILogger _logger;
    private readonly string _tenantId;
    private readonly IOptions<MediaOptions> _mediaOptions;

    public DistributedMediaTusStore(
        ITusTempStore tempStore,
        IDistributedCache cache,
        IDistributedLock distributedLock,
        ShellSettings shellSettings,
        IClock clock,
        IOptions<MediaOptions> mediaOptions,
        ILogger<DistributedMediaTusStore> logger)
    {
        _tempStore = tempStore;
        _cache = cache;
        _lock = distributedLock;
        _clock = clock;
        _logger = logger;
        _tenantId = shellSettings.TenantId;
        _mediaOptions = mediaOptions;
    }

    // ITusCreationStore

    public async Task<string> CreateFileAsync(long uploadLength, string metadata, CancellationToken cancellationToken)
    {
        var fileId = Guid.NewGuid().ToString("N");

        await _tempStore.CreateFileAsync(fileId, cancellationToken);

        var options = GetCacheOptions();

        // Store metadata.
        if (!string.IsNullOrEmpty(metadata))
        {
            await _cache.SetStringAsync(MetadataKey(fileId), metadata, options, cancellationToken);
        }

        // Store declared length.
        await _cache.SetStringAsync(LengthKey(fileId), uploadLength.ToString(), options, cancellationToken);

        // Initialize offset to 0.
        await _cache.SetStringAsync(OffsetKey(fileId), "0", options, cancellationToken);

        // Register file in the file registry for expiration enumeration.
        await AddToFileRegistryAsync(fileId, cancellationToken);

        return fileId;
    }

    public async Task<string> GetUploadMetadataAsync(string fileId, CancellationToken cancellationToken)
    {
        return await _cache.GetStringAsync(MetadataKey(fileId), cancellationToken);
    }

    // ITusStore

    public async Task<bool> FileExistAsync(string fileId, CancellationToken cancellationToken)
    {
        return await _tempStore.FileExistAsync(fileId, cancellationToken);
    }

    public async Task<long?> GetUploadLengthAsync(string fileId, CancellationToken cancellationToken)
    {
        var text = await _cache.GetStringAsync(LengthKey(fileId), cancellationToken);
        if (text == null)
        {
            return null;
        }

        return long.TryParse(text, out var length) ? length : null;
    }

    public async Task<long> GetUploadOffsetAsync(string fileId, CancellationToken cancellationToken)
    {
        var text = await _cache.GetStringAsync(OffsetKey(fileId), cancellationToken);
        if (text == null)
        {
            return 0L;
        }

        return long.TryParse(text, out var offset) ? offset : 0L;
    }

    public async Task<long> AppendDataAsync(string fileId, Stream stream, CancellationToken cancellationToken)
    {
        var offset = await GetUploadOffsetAsync(fileId, cancellationToken);

        var bytesWritten = await _tempStore.AppendDataAsync(fileId, stream, offset, cancellationToken);

        var newOffset = offset + bytesWritten;
        await _cache.SetStringAsync(OffsetKey(fileId), newOffset.ToString(), GetCacheOptions(), cancellationToken);

        return bytesWritten;
    }

    // ITusPipelineStore

    public async Task<long> AppendDataAsync(string fileId, PipeReader pipeReader, CancellationToken cancellationToken)
    {
        var offset = await GetUploadOffsetAsync(fileId, cancellationToken);

        var bytesWritten = await _tempStore.AppendDataAsync(fileId, pipeReader, offset, cancellationToken);

        var newOffset = offset + bytesWritten;
        await _cache.SetStringAsync(OffsetKey(fileId), newOffset.ToString(), GetCacheOptions(), cancellationToken);

        return bytesWritten;
    }

    // ITusTerminationStore

    public async Task DeleteFileAsync(string fileId, CancellationToken cancellationToken)
    {
        await _tempStore.DeleteFileAsync(fileId, cancellationToken);
        await RemoveCacheEntriesAsync(fileId, cancellationToken);
        await RemoveFromFileRegistryAsync(fileId, cancellationToken);
    }

    // ITusExpirationStore

    public async Task SetExpirationAsync(string fileId, DateTimeOffset expires, CancellationToken cancellationToken)
    {
        await _cache.SetStringAsync(
            ExpirationKey(fileId),
            expires.UtcTicks.ToString(),
            GetCacheOptions(),
            cancellationToken);
    }

    public async Task<DateTimeOffset?> GetExpirationAsync(string fileId, CancellationToken cancellationToken)
    {
        var text = await _cache.GetStringAsync(ExpirationKey(fileId), cancellationToken);
        if (text == null)
        {
            return null;
        }

        return long.TryParse(text, out var ticks)
            ? new DateTimeOffset(ticks, TimeSpan.Zero)
            : null;
    }

    public async Task<IEnumerable<string>> GetExpiredFilesAsync(CancellationToken cancellationToken)
    {
        var fileIds = await GetFileRegistryAsync(cancellationToken);
        if (fileIds == null || fileIds.Count == 0)
        {
            return [];
        }

        var now = _clock.UtcNow;
        var expired = new List<string>();

        foreach (var fileId in fileIds)
        {
            var text = await _cache.GetStringAsync(ExpirationKey(fileId), cancellationToken);
            if (text != null && long.TryParse(text, out var ticks) && new DateTimeOffset(ticks, TimeSpan.Zero) < now)
            {
                if (await _tempStore.FileExistAsync(fileId, cancellationToken))
                {
                    expired.Add(fileId);
                }
            }
        }

        return expired;
    }

    public async Task<int> RemoveExpiredFilesAsync(CancellationToken cancellationToken)
    {
        var expiredFiles = (await GetExpiredFilesAsync(cancellationToken)).ToList();

        foreach (var fileId in expiredFiles)
        {
            await DeleteFileAsync(fileId, cancellationToken);
        }

        if (expiredFiles.Count > 0 && _logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Removed {Count} expired TUS upload(s).", expiredFiles.Count);
        }

        return expiredFiles.Count;
    }

    // Public helpers

    /// <summary>
    /// Opens the completed upload's data file for reading.
    /// Caller is responsible for disposing the stream.
    /// </summary>
    public Stream OpenReadStream(string fileId)
    {
        return _tempStore.OpenReadStream(fileId);
    }

    /// <summary>
    /// Gets the file length from the temp store.
    /// </summary>
    public long GetFileLength(string fileId)
    {
        return _tempStore.GetFileLength(fileId);
    }

    // Cache key helpers

    private string MetadataKey(string fileId) => $"tus:metadata:{_tenantId}:{fileId}";
    private string LengthKey(string fileId) => $"tus:length:{_tenantId}:{fileId}";
    private string OffsetKey(string fileId) => $"tus:offset:{_tenantId}:{fileId}";
    private string ExpirationKey(string fileId) => $"tus:expiration:{_tenantId}:{fileId}";
    private string FileRegistryKey => $"tus:files:{_tenantId}";

    private DistributedCacheEntryOptions GetCacheOptions()
    {
        return new DistributedCacheEntryOptions
        {
            SlidingExpiration = _mediaOptions.Value.TemporaryFileLifetime,
        };
    }

    // File registry — tracks active file IDs for expiration enumeration.

    private async Task<List<string>> GetFileRegistryAsync(CancellationToken cancellationToken)
    {
        var json = await _cache.GetStringAsync(FileRegistryKey, cancellationToken);
        if (string.IsNullOrEmpty(json))
        {
            return [];
        }

        return JsonSerializer.Deserialize<List<string>>(json) ?? [];
    }

    private async Task AddToFileRegistryAsync(string fileId, CancellationToken cancellationToken)
    {
        var (locker, locked) = await _lock.TryAcquireLockAsync(
            $"tus:registry:{_tenantId}",
            TimeSpan.FromSeconds(10),
            TimeSpan.FromMinutes(1));

        if (!locked)
        {
            _logger.LogWarning("Failed to acquire lock for TUS file registry.");
            return;
        }

        await using (locker)
        {
            var fileIds = await GetFileRegistryAsync(cancellationToken);
            if (!fileIds.Contains(fileId))
            {
                fileIds.Add(fileId);
                await _cache.SetStringAsync(FileRegistryKey, JsonSerializer.Serialize(fileIds), cancellationToken);
            }
        }
    }

    private async Task RemoveFromFileRegistryAsync(string fileId, CancellationToken cancellationToken)
    {
        var (locker, locked) = await _lock.TryAcquireLockAsync(
            $"tus:registry:{_tenantId}",
            TimeSpan.FromSeconds(10),
            TimeSpan.FromMinutes(1));

        if (!locked)
        {
            _logger.LogWarning("Failed to acquire lock for TUS file registry.");
            return;
        }

        await using (locker)
        {
            var fileIds = await GetFileRegistryAsync(cancellationToken);
            if (fileIds.Remove(fileId))
            {
                await _cache.SetStringAsync(FileRegistryKey, JsonSerializer.Serialize(fileIds), cancellationToken);
            }
        }
    }

    private async Task RemoveCacheEntriesAsync(string fileId, CancellationToken cancellationToken)
    {
        await _cache.RemoveAsync(MetadataKey(fileId), cancellationToken);
        await _cache.RemoveAsync(LengthKey(fileId), cancellationToken);
        await _cache.RemoveAsync(OffsetKey(fileId), cancellationToken);
        await _cache.RemoveAsync(ExpirationKey(fileId), cancellationToken);
    }
}
