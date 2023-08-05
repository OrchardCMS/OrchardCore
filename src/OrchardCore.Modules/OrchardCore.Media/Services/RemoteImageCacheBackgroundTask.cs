using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using OrchardCore.BackgroundTasks;
using OrchardCore.Environment.Shell;
using OrchardCore.Media.Core;
using OrchardCore.Modules;

namespace OrchardCore.Media.Services;

[BackgroundTask(Schedule = "* * * * *", Description = "'Remote image cache cleanup.")]
public class RemoteImageCacheBackgroundTask : IBackgroundTask
{
    private static readonly EnumerationOptions _enumerationOptions = new() { RecurseSubdirectories = true };

    private readonly IMediaFileStore _mediaFileStore;
    private readonly ILogger _logger;

    private readonly string _cachePath;

    public RemoteImageCacheBackgroundTask(
        ShellSettings shellSettings,
        IMediaFileStore mediaFileStore,
        IWebHostEnvironment webHostEnvironment,
        ILogger<RemoteImageCacheBackgroundTask> logger)
    {
        _mediaFileStore = mediaFileStore;

        _cachePath = Path.Combine(
            webHostEnvironment.WebRootPath,
            shellSettings.Name,
            DefaultMediaFileStoreCacheFileProvider.AssetsCachePath);

        _logger = logger;
    }

    public async Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        try
        {
            // Check if the cache exists.
            if (!Directory.Exists(_cachePath))
            {
                return;
            }

            var directories = Directory.GetDirectories(_cachePath, "*", _enumerationOptions);
            foreach (var directory in directories)
            {
                var directoryInfo = new DirectoryInfo(directory);
                var path = Path.GetRelativePath(_cachePath, directoryInfo.FullName);

                // Check if the remote directory still exists.
                var entry = await _mediaFileStore.GetDirectoryInfoAsync(path);
                if (entry is null)
                {
                    Directory.Delete(directoryInfo.FullName, true);
                }
            }

            var files = Directory.GetFiles(_cachePath, "*", _enumerationOptions);
            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                var path = Path.GetRelativePath(_cachePath, fileInfo.FullName);

                // Check if the remote image still exists or was updated.
                var entry = await _mediaFileStore.GetFileInfoAsync(path);
                if (entry is null ||
                    entry.LastModifiedUtc > fileInfo.LastWriteTimeUtc)
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
