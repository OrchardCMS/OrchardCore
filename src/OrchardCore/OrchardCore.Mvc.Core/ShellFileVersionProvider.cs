using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using OrchardCore.Modules;
using OrchardCore.Modules.FileProviders;

namespace OrchardCore.Mvc
{
    /// <summary>
    /// Provides version hash for a specified file.
    /// </summary>
    public class ShellFileVersionProvider : IFileVersionProvider
    {
        private const string VersionKey = "v";
        private static readonly char[] _queryStringAndFragmentTokens = new[] { '?', '#' };
        private static readonly MemoryCache _sharedCache = new(new MemoryCacheOptions());

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
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            var resolvedPath = path;

            var queryStringOrFragmentStartIndex = path.IndexOfAny(_queryStringAndFragmentTokens);
            if (queryStringOrFragmentStartIndex != -1)
            {
                resolvedPath = path[..queryStringOrFragmentStartIndex];
            }

            if (Uri.TryCreate(resolvedPath, UriKind.Absolute, out var uri) && !uri.IsFile)
            {
                // Don't append version if the path is absolute.
                return path;
            }

            // Try to get the hash from the tenant level cache.
            if (_cache.TryGetValue(resolvedPath, out string value))
            {
                if (value.Length > 0)
                {
                    return QueryHelpers.AddQueryString(path, VersionKey, value);
                }

                return path;
            }

            // Try to get the hash from the cache shared across tenants.
            if (resolvedPath.StartsWith(requestPathBase.Value, StringComparison.OrdinalIgnoreCase))
            {
                if (_sharedCache.TryGetValue(resolvedPath[requestPathBase.Value.Length..], out value))
                {
                    return QueryHelpers.AddQueryString(path, VersionKey, value);
                }
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
                    value = GetHashForFile(fileInfo);
                    cacheEntryOptions.SetSize(value.Length * sizeof(char));

                    // Cache module static files to the shared cache.
                    if (fileProvider is IModuleStaticFileProvider)
                    {
                        _sharedCache.Set(resolvedPath, value, cacheEntryOptions);
                    }
                    else
                    {
                        _cache.Set(cacheKey, value, cacheEntryOptions);
                    }

                    return QueryHelpers.AddQueryString(path, VersionKey, value);
                }
            }

            // If the file is not in the current server, set cache so no further checks are done.
            cacheEntryOptions.SetSize(0);
            _cache.Set(cacheKey, String.Empty, cacheEntryOptions);
            return path;
        }

        private static string GetHashForFile(IFileInfo fileInfo)
        {
            using var sha256 = SHA256.Create();
            using var readStream = fileInfo.CreateReadStream();
            var hash = sha256.ComputeHash(readStream);
            return WebEncoders.Base64UrlEncode(hash);
        }
    }
}
