using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace OrchardCore.Media.Services;

internal sealed class MediaCacheManagementService
{
    private readonly IMediaFileStoreCache _remote;
    private readonly IResizedImageCache _resized;
    private readonly ILogger _logger;

    public MediaCacheManagementService(IServiceProvider services)
    {
        _remote = services.GetService<IMediaFileStoreCache>();
        _resized = services.GetService<IResizedImageCache>();
        _logger = services.GetService<ILogger<MediaCacheManagementService>>() ?? NullLogger<MediaCacheManagementService>.Instance;
    }

    public bool RemoteConfigured => _remote is not null;
    public bool ResizedConfigured => _resized is not null;

    public async Task<MediaCachePurgeStatus> PurgeAsync(string cache, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            if (cache == "remote")
            {
                if (_remote is null) { return MediaCachePurgeStatus.Unavailable; }
                return await _remote.PurgeAsync() ? MediaCachePurgeStatus.Failed : MediaCachePurgeStatus.Purged;
            }
            if (cache == "resized")
            {
                if (_resized is null) { return MediaCachePurgeStatus.Unavailable; }
                await _resized.ClearAsync(cancellationToken);
                return MediaCachePurgeStatus.Purged;
            }
            return MediaCachePurgeStatus.Invalid;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            _logger.LogWarning(exception, "Could not fully purge tenant media cache '{Cache}'.", cache);
            return MediaCachePurgeStatus.Failed;
        }
    }
}

internal enum MediaCachePurgeStatus
{
    Purged,
    Unavailable,
    Invalid,
    Failed,
}
