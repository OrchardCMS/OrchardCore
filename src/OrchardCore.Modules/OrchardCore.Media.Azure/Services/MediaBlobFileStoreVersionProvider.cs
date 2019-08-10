using System;
using System.Threading.Tasks;
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

        public async Task<string> AddFileVersionToPathAsync(string resolvedPath, string path)
        {
            var cacheKey = resolvedPath;

            // Path has already been correctly parsed before here.
            resolvedPath = _mediaFileStore.MapPublicUrlToPath(resolvedPath);

            var fileInfo = await _mediaFileStore.GetFileInfoAsync(resolvedPath) as BlobFile;

            if (fileInfo != null)
            {
                //TODO Consider expiring with a Change Token from Blob File Store.
                // Or consider a graceful cache refresh.
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(
                        TimeSpan.FromMinutes(_options.VersionHashCacheExpiryTime > 0 ? _options.VersionHashCacheExpiryTime : 120))
                    .SetSize(fileInfo.FileHash.Length * sizeof(char));

                // Set to cacheKey, so ShellFileVersionProvider can retrieve from cache.
                _cache.Set(cacheKey, fileInfo.FileHash, cacheEntryOptions);

                // Return actual path with query string
                return QueryHelpers.AddQueryString(path, VersionKey, fileInfo.FileHash);
            }

            return null;
        }
    }
}
