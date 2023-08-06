using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.BackgroundTasks;
using OrchardCore.Environment.Shell;
using OrchardCore.Media.Core;
using OrchardCore.Modules;

namespace OrchardCore.Media.Services;

[BackgroundTask(Schedule = "* * * * *", Description = "'Remote image cache cleanup.")]
public class RemoteMediaCacheBackgroundTask : IBackgroundTask
{
    private static readonly EnumerationOptions _enumerationOptions = new() { RecurseSubdirectories = true };

    private readonly IMediaFileStore _mediaFileStore;
    private readonly ILogger _logger;

    private readonly string _cachePath;
    private readonly TimeSpan? _cacheMaxStale;
    private readonly bool _cacheCleanup;

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

        _cacheMaxStale = mediaOptions.Value.CacheMaxStale.Value;
        _cacheCleanup = mediaOptions.Value.CacheCleanup;
        _logger = logger;
    }

    public async Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        // Ensure that the cache folder exists and should be cleaned.
        if (!_cacheCleanup || !_cacheMaxStale.HasValue || !Directory.Exists(_cachePath))
        {
            return;
        }

        // Ensure that a remote media cache has been registered.
        if (serviceProvider.GetService<IMediaFileStoreCache>() is null)
        {
            return;
        }

        var maxStale = _cacheMaxStale.Value;
        var minAge = DateTimeOffset.UtcNow - maxStale;
        try
        {
            var directories = Directory.GetDirectories(_cachePath, "*", _enumerationOptions);
            foreach (var directory in directories)
            {
                // Check if the directory is too recent.
                var directoryInfo = new DirectoryInfo(directory);
                if (directoryInfo.LastWriteTimeUtc > minAge)
                {
                    continue;
                }

                var path = Path.GetRelativePath(_cachePath, directoryInfo.FullName);

                // Check if the remote directory no longer exists.
                var entry = await _mediaFileStore.GetDirectoryInfoAsync(path);
                if (entry is null)
                {
                    Directory.Delete(directoryInfo.FullName, true);
                }
            }

            var files = Directory.GetFiles(_cachePath, "*", _enumerationOptions);
            foreach (var file in files)
            {
                // Check if the file is too recent.
                var fileInfo = new FileInfo(file);
                if (fileInfo.LastWriteTimeUtc > minAge)
                {
                    continue;
                }

                var path = Path.GetRelativePath(_cachePath, fileInfo.FullName);

                // Check if the remote media no longer exists or was updated.
                var entry = await _mediaFileStore.GetFileInfoAsync(path);
                if (entry is null || entry.LastModifiedUtc > (fileInfo.LastWriteTimeUtc + maxStale))
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
                    "Sharing violation while cleaning the remote image cache at '{CachePath}'.",
                    _cachePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to clean the remote image cache at '{CachePath}'.",
                _cachePath);
        }

    }
}
