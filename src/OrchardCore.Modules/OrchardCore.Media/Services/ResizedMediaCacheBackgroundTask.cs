using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.BackgroundTasks;
using OrchardCore.Modules;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Middleware;

namespace OrchardCore.Media.Services;

[BackgroundTask(Schedule = "* * * * *", Description = "'Resized image cache cleanup.")]
public class ResizedMediaCacheBackgroundTask : IBackgroundTask
{
    private static readonly EnumerationOptions _enumerationOptions = new() { RecurseSubdirectories = true };

    private readonly ILogger _logger;

    private readonly string _cachePath;
    private readonly string _cacheFolder;
    private readonly TimeSpan _cacheMaxAge;
    private readonly TimeSpan? _cacheMaxStale;
    private readonly bool _cacheCleanup;

    public ResizedMediaCacheBackgroundTask(
        IWebHostEnvironment webHostEnvironment,
        IOptions<MediaOptions> mediaOptions,
        IOptions<ImageSharpMiddlewareOptions> middlewareOptions,
        IOptions<PhysicalFileSystemCacheOptions> cacheOptions,
        ILogger<ResizedMediaCacheBackgroundTask> logger)
    {
        _cachePath = Path.Combine(webHostEnvironment.WebRootPath, cacheOptions.Value.CacheFolder);
        _cacheFolder = Path.GetFileName(cacheOptions.Value.CacheFolder);

        if (mediaOptions.Value.CacheMaxStale.HasValue)
        {
            _cacheMaxStale = middlewareOptions.Value.CacheMaxAge + mediaOptions.Value.CacheMaxStale.Value;
        }

        _cacheMaxAge = middlewareOptions.Value.CacheMaxAge;
        _cacheMaxStale = mediaOptions.Value.CacheMaxStale;
        _cacheCleanup = mediaOptions.Value.CacheCleanup;
        _logger = logger;
    }

    public Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        // Ensure that the cache folder exists and should be cleaned.
        if (!_cacheCleanup || !_cacheMaxStale.HasValue || !Directory.Exists(_cachePath))
        {
            return Task.CompletedTask;
        }

        var minAge = DateTimeOffset.UtcNow - _cacheMaxAge - _cacheMaxStale.Value;
        try
        {
            // Lookup for all '*.meta' files.
            var files = Directory.GetFiles(_cachePath, "*.meta", _enumerationOptions);
            foreach (var file in files)
            {
                // Check if the file is too recent.
                var fileInfo = new FileInfo(file);
                if (fileInfo.LastWriteTimeUtc > minAge)
                {
                    continue;
                }

                // Delete the folder including the resized image.
                Directory.Delete(fileInfo.DirectoryName, true);

                // Delete all new empty parent directories.
                var parent = fileInfo.Directory.Parent;
                while (parent is not null && parent.Name != _cacheFolder)
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
                    "Sharing violation while cleaning the resized image cache at '{CachePath}'.",
                    _cachePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to clean the resized image cache at '{CachePath}'.",
                _cachePath);
        }

        return Task.CompletedTask;
    }
}
