using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.BackgroundTasks;
using OrchardCore.Environment.Shell;
using OrchardCore.Media.Core;
using OrchardCore.Modules;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Middleware;

namespace OrchardCore.Media.Services;

[BackgroundTask(Schedule = "* * * * *", Description = "'ImageSharp' cache cleanup.")]
public class ImageSharpCacheBackgroundTask : IBackgroundTask
{
    private static readonly EnumerationOptions _enumerationOptions = new() { RecurseSubdirectories = true };

    private readonly IMediaFileStore _mediaFileStore;
    private readonly ImageSharpMiddlewareOptions _middlewareOptions;
    private readonly ILogger _logger;

    private readonly string _imageSharpCachePath;
    private readonly string _imageSharpCacheFolder;
    private readonly string _remoteImageCachePath;

    public ImageSharpCacheBackgroundTask(
        ShellSettings shellSettings,
        IMediaFileStore mediaFileStore,
        IWebHostEnvironment webHostEnvironment,
        IOptions<ImageSharpMiddlewareOptions> middlewareOptions,
        IOptions<PhysicalFileSystemCacheOptions> cacheOptions,
        ILogger<ImageSharpCacheBackgroundTask> logger)
    {
        _mediaFileStore = mediaFileStore;
        _middlewareOptions = middlewareOptions.Value;

        _imageSharpCachePath = Path.Combine(
            webHostEnvironment.WebRootPath,
            cacheOptions.Value.CacheFolder);

        _imageSharpCacheFolder = Path.GetFileName(cacheOptions.Value.CacheFolder);

        _remoteImageCachePath = Path.Combine(
            webHostEnvironment.WebRootPath,
            shellSettings.Name,
            DefaultMediaFileStoreCacheFileProvider.AssetsCachePath);

        _logger = logger;
    }

    public async Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        try
        {
            var directories = Directory.GetDirectories(_remoteImageCachePath, "*", _enumerationOptions);
            foreach (var directory in directories)
            {
                var directoryInfo = new DirectoryInfo(directory);
                var path = Path.GetRelativePath(_remoteImageCachePath, directoryInfo.FullName);

                var entry = await _mediaFileStore.GetDirectoryInfoAsync(path);
                if (entry is null)
                {
                    Directory.Delete(directoryInfo.FullName, true);
                }
            }

            var files = Directory.GetFiles(_remoteImageCachePath, "*", _enumerationOptions);
            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                var path = Path.GetRelativePath(_remoteImageCachePath, fileInfo.FullName);

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
                    "Sharing violation while cleaning the image cache folder at '{CachePath}'.",
                    _remoteImageCachePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to clean the remote image cache folder at '{CachePath}'.",
                _remoteImageCachePath);
        }


        try
        {
            var files = Directory.GetFiles(_imageSharpCachePath, "*.meta", _enumerationOptions);
            foreach ( var file in files)
            {
                var fileInfo = new FileInfo(file);

                if (fileInfo.LastWriteTimeUtc > (DateTimeOffset.UtcNow - _middlewareOptions.CacheMaxAge))
                {
                    continue;
                }

                Directory.Delete(fileInfo.DirectoryName, true);

                var parent = fileInfo.Directory.Parent;
                while (parent is not null && parent.Name != _imageSharpCacheFolder)
                {
                    Directory.Delete(parent.FullName);

                    parent = parent.Parent;
                    if (parent.EnumerateFileSystemInfos().Any())
                    {
                        break;
                    }
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
                    "Sharing violation while cleaning the image cache folder at '{CachePath}'.",
                    _imageSharpCachePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to clean the image cache folder at '{CachePath}'.",
                _imageSharpCachePath);
        }
    }
}
