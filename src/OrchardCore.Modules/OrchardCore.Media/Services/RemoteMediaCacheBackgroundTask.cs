using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.BackgroundTasks;
using OrchardCore.Environment.Shell;
using OrchardCore.Media.Core;
using OrchardCore.Modules;

namespace OrchardCore.Media.Services;

[BackgroundTask(Schedule = "30 0 * * *", Description = "Remote media cache cleanup.")]
public sealed class RemoteMediaCacheBackgroundTask : IBackgroundTask
{
    private static readonly EnumerationOptions _enumerationOptions = new() { RecurseSubdirectories = true };

    private readonly IMediaFileStore _mediaFileStore;
    private readonly ILogger _logger;

    private readonly string _cachePath;
    private readonly TimeSpan? _cacheMaxStale;

    public RemoteMediaCacheBackgroundTask(
        ShellSettings shellSettings,
        IMediaFileStore mediaFileStore,
        IWebHostEnvironment webHostEnvironment,
        IOptions<MediaOptions> mediaOptions,
        ILogger<RemoteMediaCacheBackgroundTask> logger)
    {
        _mediaFileStore = mediaFileStore;

        _cachePath = Path.Combine(
            webHostEnvironment.WebRootPath,
            shellSettings.Name,
            DefaultMediaFileStoreCacheFileProvider.AssetsCachePath);

        _cacheMaxStale = mediaOptions.Value.RemoteCacheMaxStale;
        _logger = logger;
    }

    public async Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        // Ensure that the cache folder exists and should be cleaned.
        if (!_cacheMaxStale.HasValue || !Directory.Exists(_cachePath))
        {
            return;
        }

        // Ensure that a remote media cache has been registered.
        if (serviceProvider.GetService<IMediaFileStoreCache>() is null)
        {
            return;
        }

        // The min write time for an item to be retained in the cache,
        // without having to get the item info from the remote store.
        var minWriteTimeUtc = DateTimeOffset.UtcNow - _cacheMaxStale.Value;
        try
        {
            // Lookup for all cache directories.
            var directories = Directory.GetDirectories(_cachePath, "*", _enumerationOptions);
            foreach (var directory in directories)
            {
                // Check if the directory is retained.
                var directoryInfo = new DirectoryInfo(directory);
                if (!directoryInfo.Exists || directoryInfo.LastWriteTimeUtc > minWriteTimeUtc)
                {
                    continue;
                }

                var path = Path.GetRelativePath(_cachePath, directoryInfo.FullName);

                // Check if the remote directory doesn't exist.
                var entry = await _mediaFileStore.GetDirectoryInfoAsync(path);
                if (entry is null)
                {
                    Directory.Delete(directoryInfo.FullName, true);
                }
            }

            // Lookup for all cache files.
            var files = Directory.GetFiles(_cachePath, "*", _enumerationOptions);
            foreach (var file in files)
            {
                // Check if the file is retained.
                var fileInfo = new FileInfo(file);
                if (!fileInfo.Exists || fileInfo.LastWriteTimeUtc > minWriteTimeUtc)
                {
                    continue;
                }

                var path = Path.GetRelativePath(_cachePath, fileInfo.FullName);

                // Check if the remote media doesn't exist or was updated.
                var entry = await _mediaFileStore.GetFileInfoAsync(path);
                if (entry is null ||
                    (entry.LastModifiedUtc > fileInfo.LastWriteTimeUtc &&
                    entry.LastModifiedUtc < minWriteTimeUtc))
                {
                    File.Delete(fileInfo.FullName);
                }
            }
        }
        catch (Exception ex) when (ex is DirectoryNotFoundException)
        {
        }
        catch (Exception ex) when (ex.IsFileSharingViolation())
        {
            if (_logger.IsEnabled(LogLevel.Warning))
            {
                _logger.LogWarning(
                    ex,
                    "Sharing violation while cleaning the remote media cache at '{CachePath}'.",
                    _cachePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to clean the remote media cache at '{CachePath}'.",
                _cachePath);
        }

    }
}
