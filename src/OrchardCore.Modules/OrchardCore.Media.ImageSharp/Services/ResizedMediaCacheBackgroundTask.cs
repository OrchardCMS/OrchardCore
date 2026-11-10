using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.BackgroundTasks;
using OrchardCore.Modules;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Middleware;

namespace OrchardCore.Media.Services;

[BackgroundTask(Schedule = "0 0 * * *", Description = "Resized media cache cleanup.")]
public sealed class ResizedMediaCacheBackgroundTask : IBackgroundTask
{
    private static readonly EnumerationOptions _enumerationOptions = new() { RecurseSubdirectories = true };

    private readonly ILogger _logger;

    private readonly string _cachePath;
    private readonly string _cacheFolder;
    private readonly TimeSpan _cacheMaxAge;
    private readonly TimeSpan? _cacheMaxStale;

    public ResizedMediaCacheBackgroundTask(
        IWebHostEnvironment webHostEnvironment,
        IOptions<MediaOptions> mediaOptions,
        IOptions<ImageSharpMiddlewareOptions> middlewareOptions,
        IOptions<PhysicalFileSystemCacheOptions> cacheOptions,
        ILogger<ResizedMediaCacheBackgroundTask> logger)
    {
        _cachePath = Path.Combine(webHostEnvironment.WebRootPath, cacheOptions.Value.CacheFolder);
        _cacheFolder = Path.GetFileName(cacheOptions.Value.CacheFolder);
        _cacheMaxAge = middlewareOptions.Value.CacheMaxAge;
        _cacheMaxStale = mediaOptions.Value.ResizedCacheMaxStale;
        _logger = logger;
    }

    public Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        // Ensure that the cache folder exists and should be cleaned.
        if (!_cacheMaxStale.HasValue || !Directory.Exists(_cachePath))
        {
            return Task.CompletedTask;
        }

        // The min write time for an item to be retained in the cache.
        var minWriteTimeUtc = DateTime.UtcNow.Subtract(_cacheMaxAge + _cacheMaxStale.Value);
        try
        {
            // Lookup for all meta files.
            var files = Directory.GetFiles(_cachePath, "*.meta", _enumerationOptions);
            foreach (var file in files)
            {
                // Check if the file is retained.
                var fileInfo = new FileInfo(file);
                if (!fileInfo.Exists || fileInfo.LastWriteTimeUtc > minWriteTimeUtc)
                {
                    continue;
                }

                // Delete the folder including the media item.
                Directory.Delete(fileInfo.DirectoryName, true);

                // Delete new empty parent directories.
                var parent = fileInfo.Directory.Parent;
                while (parent is not null && parent.Name != _cacheFolder)
                {
                    Directory.Delete(parent.FullName);

                    parent = parent.Parent;
                    if (!parent.Exists || parent.EnumerateFileSystemInfos().Any())
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
                    "Sharing violation while cleaning the resized media cache at '{CachePath}'.",
                    _cachePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to clean the resized media cache at '{CachePath}'.",
                _cachePath);
        }

        return Task.CompletedTask;
    }
}
