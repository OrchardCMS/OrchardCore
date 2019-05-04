using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Antiforgery.Internal;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;


namespace OrchardCore.ResourceManagement
{
    /// <summary>
    /// Provides version hash for a specified file.
    /// </summary>
    public class FileVersionProvider : IFileVersionProvider
    {
        private const string VersionKey = "v";
        private static readonly char[] QueryStringAndFragmentTokens = new[] { '?', '#' };

        private readonly IEnumerable<IFileProvider> _registeredFileProviders;
        private readonly IFileProvider _webRootfileProvider;

        private IList<IFileProvider> _combinedFileProviders;
        private readonly IMemoryCache _cache;
        public FileVersionProvider(
            IEnumerable<IFileProvider> registeredFileProviders,
            IHostingEnvironment environment,
            IMemoryCache cache
            )
        {
            _registeredFileProviders = registeredFileProviders;
            _webRootfileProvider = environment.WebRootFileProvider;
            _cache = cache;
        }



        public string AddFileVersionToPath(PathString requestPathBase, string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            //build _combinedFileProviders if has not been built
            if (_combinedFileProviders == null)
            {
                _combinedFileProviders = _registeredFileProviders.ToList();
                if (_webRootfileProvider != null)
                {
                    _combinedFileProviders.Add(_webRootfileProvider);
                }
            }

            var resolvedPath = path;

            var queryStringOrFragmentStartIndex = path.IndexOfAny(QueryStringAndFragmentTokens);
            if (queryStringOrFragmentStartIndex != -1)
            {
                resolvedPath = path.Substring(0, queryStringOrFragmentStartIndex);
            }
            resolvedPath = resolvedPath.TrimStart('~');

            if (Uri.TryCreate(resolvedPath, UriKind.Absolute, out var uri) && !uri.IsFile)
            {
                // Don't append version if the path is absolute.
                return path;
            }

            if (_cache.TryGetValue(path, out string value))
            {
                return value;
            }

            var cacheEntryOptions = new MemoryCacheEntryOptions();
            foreach (var fileProvider in _combinedFileProviders)
            {
                cacheEntryOptions.AddExpirationToken(fileProvider.Watch(resolvedPath));
                var fileInfo = fileProvider.GetFileInfo(resolvedPath);

                if (!fileInfo.Exists &&
                    requestPathBase.HasValue &&
                    resolvedPath.StartsWith(requestPathBase.Value, StringComparison.OrdinalIgnoreCase))
                {
                    var requestPathBaseRelativePath = resolvedPath.Substring(requestPathBase.Value.Length);
                    cacheEntryOptions.AddExpirationToken(fileProvider.Watch(requestPathBaseRelativePath));
                    fileInfo = fileProvider.GetFileInfo(requestPathBaseRelativePath);
                }
                if (fileInfo.Exists)
                {
                    value = QueryHelpers.AddQueryString(path, VersionKey, GetHashForFile(fileInfo));
                    cacheEntryOptions.SetSize(value.Length * sizeof(char));
                    value = _cache.Set(path, value, cacheEntryOptions);
                    return value;
                }
            }

            // if the file is not in the current server, set cache so no further checks are done.
            value = path;
            cacheEntryOptions.SetSize(value.Length * sizeof(char));
            value = _cache.Set(path, value, cacheEntryOptions);

            return value;
        }

        private static string GetHashForFile(IFileInfo fileInfo)
        {
            using (var sha256 = CryptographyAlgorithms.CreateSHA256())
            {
                using (var readStream = fileInfo.CreateReadStream())
                {
                    var hash = sha256.ComputeHash(readStream);
                    return WebEncoders.Base64UrlEncode(hash);
                }
            }
        }
    }
}