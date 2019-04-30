using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Mvc.Razor.Infrastructure;
using System.Security.Cryptography;
using System.Threading.Tasks;
using OrchardCore.Abstractions.Modules;

namespace OrchardCore.Media.Services
{
    /// <summary>
    /// Provides version hash for a specified file.
    /// </summary>
    public class MediaFileStoreVersionProvider : IMediaFileStoreVersionProvider
    {
        private const string VersionKey = "v";
        private static readonly char[] QueryStringAndFragmentTokens = new[] { '?', '#' };

        private readonly IFileProvider _webRootFileProvider;

        private readonly IMemoryCache _tagHelperCache;

        private readonly IMemoryCache _tenantCache;

        private readonly IMediaFileStore _mediaFileStore;
        private readonly IFileVersionHashProvider _fileVersionHashProvider;

        public MediaFileStoreVersionProvider(
            IHostingEnvironment hostingEnvironment,
            TagHelperMemoryCacheProvider tagHelperCacheProvider,
            IMediaFileStore mediaFileStore,
            IMemoryCache tenantCache,
            IFileVersionHashProvider fileVersionHashProvider
            )
        {
            if (hostingEnvironment == null)
            {
                throw new ArgumentNullException(nameof(hostingEnvironment));
            }

            if (tagHelperCacheProvider == null)
            {
                throw new ArgumentNullException(nameof(tagHelperCacheProvider));
            }

            _webRootFileProvider = hostingEnvironment.WebRootFileProvider;
            _tagHelperCache = tagHelperCacheProvider.Cache;
            _mediaFileStore = mediaFileStore;
            _tenantCache = tenantCache;
            _fileVersionHashProvider = fileVersionHashProvider;
        }

        public async Task<string> AddFileVersionToPathAsync(PathString requestPathBase, string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            var resolvedPath = path;

            var queryStringOrFragmentStartIndex = path.IndexOfAny(QueryStringAndFragmentTokens);
            if (queryStringOrFragmentStartIndex != -1)
            {
                resolvedPath = path.Substring(0, queryStringOrFragmentStartIndex);
            }

            if (Uri.TryCreate(resolvedPath, UriKind.Absolute, out var uri) && !uri.IsFile)
            {
                // Don't append version if the path is absolute.
                return path;
            }

            if (_tagHelperCache.TryGetValue(path, out string value))
            {
                return value;
            }

            if (_tenantCache.TryGetValue(path,out value))
            {
                return value;
            }

            //check mediafilestore first
            {
                var publicPath = _mediaFileStore.MapPublicUrlToPath(resolvedPath);
                var fileStoreEntry = await _mediaFileStore.GetFileInfoAsync(publicPath);
                //TODO check further into this, believe it unecesary
                string fileStoreEntryPath = publicPath;
                if (fileStoreEntry == null &&
                    requestPathBase.HasValue &&
                    resolvedPath.StartsWith(requestPathBase.Value, StringComparison.OrdinalIgnoreCase))
                {
                    var requestPathBaseRelativePath = resolvedPath.Substring(requestPathBase.Value.Length);
                    fileStoreEntryPath = requestPathBaseRelativePath;
                    fileStoreEntry = await _mediaFileStore.GetFileInfoAsync(requestPathBaseRelativePath);
                }

                if (fileStoreEntry != null)
                {
                    using (var readStream = await _mediaFileStore.GetFileStreamAsync(fileStoreEntryPath))
                    {
                        value = QueryHelpers.AddQueryString(path, VersionKey, _fileVersionHashProvider.GetFileVersionHash(readStream));
                        value = _tenantCache.Set(path, value);
                        return value;
                    }
                }
            }

            //then check host webrootfileprovider
            {
                var cacheEntryOptions = new MemoryCacheEntryOptions();
                cacheEntryOptions.AddExpirationToken(_webRootFileProvider.Watch(resolvedPath));
                var fileInfo = _webRootFileProvider.GetFileInfo(resolvedPath);

                if (!fileInfo.Exists &&
                    requestPathBase.HasValue &&
                    resolvedPath.StartsWith(requestPathBase.Value, StringComparison.OrdinalIgnoreCase))
                {
                    var requestPathBaseRelativePath = resolvedPath.Substring(requestPathBase.Value.Length);
                    cacheEntryOptions.AddExpirationToken(_webRootFileProvider.Watch(requestPathBaseRelativePath));
                    fileInfo = _webRootFileProvider.GetFileInfo(requestPathBaseRelativePath);
                }

                if (fileInfo.Exists)
                {
                    using (var readStream = fileInfo.CreateReadStream())
                    {
                        value = QueryHelpers.AddQueryString(path, VersionKey, _fileVersionHashProvider.GetFileVersionHash(readStream));
                        cacheEntryOptions.SetSize(value.Length * sizeof(char));
                        //if found in host webroot set cache
                        value = _tagHelperCache.Set(path, value, cacheEntryOptions);
                        return value;
                    }
                }
                else
                {
                    //if not found in host webroot set in tenantcache
                    value = path;
                    cacheEntryOptions.SetSize(value.Length * sizeof(char));
                    value = _tagHelperCache.Set(path, value, cacheEntryOptions);
                    return value;
                }
            }
        }
    }
}