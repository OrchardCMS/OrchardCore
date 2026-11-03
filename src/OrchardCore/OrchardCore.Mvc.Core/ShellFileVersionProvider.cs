using System.Security.Cryptography;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using OrchardCore.Modules;
using OrchardCore.Modules.FileProviders;

namespace OrchardCore.Mvc;

/// <summary>
/// Provides version hash for a specified file.
/// </summary>
public class ShellFileVersionProvider : IFileVersionProvider
{
    private const string VersionKey = "v";
    private static readonly char[] s_queryStringAndFragmentTokens = ['?', '#'];
    private static readonly MemoryCache s_sharedCache = new(new MemoryCacheOptions());

    private readonly IFileProvider[] _fileProviders;
    private readonly IMemoryCache _cache;

    public ShellFileVersionProvider(
        IEnumerable<IStaticFileProvider> staticFileProviders,
        IWebHostEnvironment environment,
        IMemoryCache cache)
    {
        _fileProviders = staticFileProviders
            .Concat(new[] { environment.WebRootFileProvider })
            .ToArray();

        _cache = cache;
    }

    public string AddFileVersionToPath(PathString requestPathBase, string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        var resolvedPath = path;

        var queryStringOrFragmentStartIndex = path.IndexOfAny(s_queryStringAndFragmentTokens);
        if (queryStringOrFragmentStartIndex != -1)
        {
            resolvedPath = path[..queryStringOrFragmentStartIndex];
        }

        // The versioned path is cached for the common case of a path without any query string or fragment.
        var isResolvedPath = queryStringOrFragmentStartIndex == -1;

        if (Uri.TryCreate(resolvedPath, UriKind.Absolute, out var uri) && !uri.IsFile)
        {
            // Don't append version if the path is absolute.
            return path;
        }

        // Try to get the version from the tenant level cache.
        if (_cache.TryGetValue(resolvedPath, out FileVersion fileVersion))
        {
            if (fileVersion.Hash.Length > 0)
            {
                return isResolvedPath
                    ? fileVersion.VersionedPath
                    : QueryHelpers.AddQueryString(path, VersionKey, fileVersion.Hash);
            }

            return path;
        }

        // Try to get the version from the cache shared across tenants.
        if (resolvedPath.StartsWith(requestPathBase.Value, StringComparison.OrdinalIgnoreCase))
        {
#pragma warning disable CA1831 // Use AsSpan instead of Range-based indexers for string
            if (s_sharedCache.TryGetValue(resolvedPath[requestPathBase.Value.Length..], out fileVersion))
            {
                if (!isResolvedPath)
                {
                    return QueryHelpers.AddQueryString(path, VersionKey, fileVersion.Hash);
                }

                // The shared versioned path is relative to the request path base.
                return requestPathBase.HasValue
                    ? string.Concat(requestPathBase.Value, fileVersion.VersionedPath)
                    : fileVersion.VersionedPath;
            }
#pragma warning restore CA1831
        }

        var cacheKey = resolvedPath;

        var cacheEntryOptions = new MemoryCacheEntryOptions();
        foreach (var fileProvider in _fileProviders)
        {
            cacheEntryOptions.AddExpirationToken(fileProvider.Watch(resolvedPath));
            var fileInfo = fileProvider.GetFileInfo(resolvedPath);

            // Perform check against requestPathBase.
            if (!fileInfo.Exists &&
                requestPathBase.HasValue &&
                resolvedPath.StartsWith(requestPathBase.Value, StringComparison.OrdinalIgnoreCase))
            {
                resolvedPath = resolvedPath[requestPathBase.Value.Length..];
                cacheEntryOptions.AddExpirationToken(fileProvider.Watch(resolvedPath));
                fileInfo = fileProvider.GetFileInfo(resolvedPath);
            }

            // Perform check against VirtualPathBase.
            if (!fileInfo.Exists &&
                fileProvider is IVirtualPathBaseProvider virtualPathBaseProvider &&
                virtualPathBaseProvider.VirtualPathBase.HasValue &&
                resolvedPath.StartsWith(virtualPathBaseProvider.VirtualPathBase.Value, StringComparison.OrdinalIgnoreCase))
            {
                resolvedPath = resolvedPath[virtualPathBaseProvider.VirtualPathBase.Value.Length..];
                cacheEntryOptions.AddExpirationToken(fileProvider.Watch(resolvedPath));
                fileInfo = fileProvider.GetFileInfo(resolvedPath);
            }

            if (fileInfo.Exists)
            {
                var hash = GetHashForFile(fileInfo);

                // Cache module static files to the shared cache.
                if (fileProvider is IModuleStaticFileProvider)
                {
                    // The shared versioned path is computed for the path relative to the request path base.
                    fileVersion = new FileVersion(hash, QueryHelpers.AddQueryString(resolvedPath, VersionKey, hash));
                    cacheEntryOptions.SetSize(fileVersion.Size);
                    s_sharedCache.Set(resolvedPath, fileVersion, cacheEntryOptions);
                }
                else
                {
                    fileVersion = new FileVersion(hash, QueryHelpers.AddQueryString(cacheKey, VersionKey, hash));
                    cacheEntryOptions.SetSize(fileVersion.Size);
                    _cache.Set(cacheKey, fileVersion, cacheEntryOptions);
                }

                return QueryHelpers.AddQueryString(path, VersionKey, hash);
            }
        }

        // If the file is not in the current server, set cache so no further checks are done.
        cacheEntryOptions.SetSize(0);
        _cache.Set(cacheKey, FileVersion.NotFound, cacheEntryOptions);
        return path;
    }

    /// <summary>
    /// The cached version of a file: its hash and the path with the version appended.
    /// </summary>
    private sealed class FileVersion
    {
        public static readonly FileVersion NotFound = new(string.Empty, null);

        public FileVersion(string hash, string versionedPath)
        {
            Hash = hash;
            VersionedPath = versionedPath;
        }

        public string Hash { get; }

        public string VersionedPath { get; }

        public long Size => (Hash.Length + (VersionedPath?.Length ?? 0)) * sizeof(char);
    }

    private static string GetHashForFile(IFileInfo fileInfo)
    {
        using var sha256 = SHA256.Create();
        using var readStream = fileInfo.CreateReadStream();
        var hash = sha256.ComputeHash(readStream);
        return WebEncoders.Base64UrlEncode(hash);
    }
}
