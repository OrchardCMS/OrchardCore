using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using OrchardCore.FileStorage;
using OrchardCore.FileStorage.AzureBlob;

namespace OrchardCore.Media.Azure.Services
{
    public class MediaBlobFileStoreVersionProvider : IFileStoreVersionProvider
    {
        private const string VersionKey = "v";
        private readonly IMediaFileStore _mediaFileStore;
        private readonly IMemoryCache _cache;
        private readonly MediaBlobStorageOptions _options;

        //TODO Investigate using ISignalr from Media Controller when file is uploaded, deleted etc, to clear cache

        public MediaBlobFileStoreVersionProvider(
            IMediaFileStore mediaFileStore,
            IMemoryCache cache,
            IOptions<MediaBlobStorageOptions> options
            )
        {
            _mediaFileStore = mediaFileStore;
            _cache = cache;
            _options = options.Value;
        }

        public async Task<string> AddFileVersionToPathAsync(PathString requestPathBase, string path)
        {
            // Path has already been correctly parsed before here.
            var mappedPath = _mediaFileStore.MapPublicUrlToPath(requestPathBase + path);

            var fileInfo = await _mediaFileStore.GetFileInfoAsync(mappedPath) as BlobFile;

            //TODO test this without requestPathBase
            if (fileInfo != null)
            {
                var value = fileInfo.FileHash;
                //TODO expiry from configuration
                var cacheEntryOptions = new MemoryCacheEntryOptions();
                cacheEntryOptions.SetAbsoluteExpiration(TimeSpan.FromMinutes(_options.VersionHashCacheExpiryTime > 0 ? _options.VersionHashCacheExpiryTime : 120));
                cacheEntryOptions.SetSize(value.Length * sizeof(char));
                //TODO test
                // Set to original path, so ShellFileVersionProvider can retrieve from cache
                _cache.Set(requestPathBase + path, value, cacheEntryOptions);
                return QueryHelpers.AddQueryString(requestPathBase + path, VersionKey, value);
            }
            return null;
        }
    }
}
