#nullable enable

using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Media.Services;

internal sealed class PhysicalFileSystemResizedImageCache : IResizedImageCache
{
    internal const string CacheFolder = "media-cache";

    private static readonly (string Extension, string ContentType)[] _formats =
    [
        (".jpg",  "image/jpeg"),
        (".png",  "image/png"),
        (".webp", "image/webp"),
        (".gif",  "image/gif"),
        (".bmp",  "image/bmp"),
    ];

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

    public Task<(Stream Content, string ContentType)?> GetAsync(string cacheKey, CancellationToken cancellationToken = default)
    {
        var dir = GetCacheDir(cacheKey);
        foreach (var (ext, contentType) in _formats)
        {
            var path = Path.Combine(dir, cacheKey + ext);
            if (!File.Exists(path))
                continue;

            try
            {
                Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
                return Task.FromResult<(Stream, string)?>(( stream, contentType ));
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
        var ext = ContentTypeToExtension(contentType);
        var dir = GetCacheDir(cacheKey);
        Directory.CreateDirectory(dir);

        var path = Path.Combine(dir, cacheKey + ext);
        try
        {
            await using var file = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true);
            await image.CopyToAsync(file, cancellationToken);
        }
        catch (IOException ex)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning(ex, "Could not write resized image cache entry at '{Path}'.", path);
        }
    }

    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(_cacheRoot))
            return Task.CompletedTask;

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
            return Task.CompletedTask;

        var cutoff = DateTime.UtcNow - maxAge;

        try
        {
            foreach (var file in Directory.EnumerateFiles(_cacheRoot, "*", _enumOptions))
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                try
                {
                    var info = new FileInfo(file);
                    if (info.Exists && info.LastWriteTimeUtc < cutoff)
                        info.Delete();
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

    private static string ContentTypeToExtension(string contentType) => contentType switch
    {
        "image/png"  => ".png",
        "image/webp" => ".webp",
        "image/gif"  => ".gif",
        "image/bmp"  => ".bmp",
        _            => ".jpg",
    };

    internal static string ComputeCacheKey(string tenantName, string path, IEnumerable<KeyValuePair<string, string>> commands)
    {
        var builder = new StringBuilder();
        builder.Append(tenantName).Append('|').Append(path).Append('|');

        foreach (var (key, value) in commands.OrderBy(p => p.Key, StringComparer.OrdinalIgnoreCase))
        {
            builder.Append(key).Append('=').Append(value).Append('&');
        }

        var raw = Encoding.UTF8.GetBytes(builder.ToString());
        var hash = SHA256.HashData(raw);
        return Convert.ToHexStringLower(hash);
    }
}
