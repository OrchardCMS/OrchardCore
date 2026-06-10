#nullable enable

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Media.Services;

internal sealed class PhysicalFileSystemResizedImageCache : IResizedImageCache
{
    internal const string CacheFolder = "media-cache";

    private static readonly EnumerationOptions _enumOptions = new() { RecurseSubdirectories = true };

    private readonly string _cacheRoot;
    private readonly ILogger _logger;

    public PhysicalFileSystemResizedImageCache(
        IWebHostEnvironment env,
        ShellSettings shellSettings,
        ILogger<PhysicalFileSystemResizedImageCache> logger)
    {
        _cacheRoot = Path.Combine(env.WebRootPath, shellSettings.Name, CacheFolder);
        _logger = logger;
    }

    public Task<(Stream Content, string ContentType)?> GetAsync(string cacheKey, string fileExtension, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(GetCacheDir(cacheKey), cacheKey + fileExtension);
        if (File.Exists(path))
        {
            try
            {
                Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);

                return Task.FromResult<(Stream, string)?>((stream, MediaResizingConstants.ExtensionToContentType(fileExtension)));
            }
            catch (IOException)
            {
                // File may have been deleted concurrently — treat as cache miss.
            }
        }

        return Task.FromResult<(Stream, string)?>(null);
    }

    public async Task SetAsync(string cacheKey, Stream image, string contentType, TimeSpan maxAge, CancellationToken cancellationToken = default)
    {
        var ext = MediaResizingConstants.ContentTypeToExtension(contentType);
        var dir = GetCacheDir(cacheKey);
        Directory.CreateDirectory(dir);

        var path = Path.Combine(dir, cacheKey + ext);

        // Write to a unique temp file and atomically move it into place. This avoids sharing
        // violations when several requests for the same not-yet-cached image are processed
        // concurrently (cache-stampede), and never serves a partially written file.
        var tempPath = path + '.' + Path.GetRandomFileName() + ".tmp";
        try
        {
            await using (var file = new FileStream(tempPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, useAsync: true))
            {
                await image.CopyToAsync(file, cancellationToken);
            }

            File.Move(tempPath, path, overwrite: true);
        }
        catch (IOException ex)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
            {
                _logger.LogWarning(ex, "Could not write resized image cache entry at '{Path}'.", path);
            }

            TryDeleteTempFile(tempPath);
        }
    }

    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(_cacheRoot))
        {
            return Task.CompletedTask;
        }

        try
        {
            Directory.Delete(_cacheRoot, recursive: true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _logger.LogWarning(ex, "Could not fully clear resized image cache at '{Root}'.", _cacheRoot);
        }

        return Task.CompletedTask;
    }

    public Task ClearStaleAsync(TimeSpan maxAge, CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(_cacheRoot))
        {
            return Task.CompletedTask;
        }

        var cutoff = DateTime.UtcNow - maxAge;

        try
        {
            foreach (var file in Directory.EnumerateFiles(_cacheRoot, "*", _enumOptions))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    var info = new FileInfo(file);
                    if (info.Exists && info.LastWriteTimeUtc < cutoff)
                    {
                        info.Delete();
                    }
                }
                catch (IOException)
                {
                    // File may have been deleted concurrently — continue.
                }
            }
        }
        catch (Exception ex) when (ex is DirectoryNotFoundException or UnauthorizedAccessException)
        {
            _logger.LogWarning(ex, "Could not enumerate resized image cache at '{Root}'.", _cacheRoot);
        }

        return Task.CompletedTask;
    }

    private string GetCacheDir(string cacheKey)
        => Path.Combine(_cacheRoot, cacheKey.Length >= 2 ? cacheKey[..2] : "xx");

    private void TryDeleteTempFile(string tempPath)
    {
        try
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
        catch (IOException)
        {
            // Best-effort cleanup; a stale temp file is collected by ClearStaleAsync.
        }
    }
}
