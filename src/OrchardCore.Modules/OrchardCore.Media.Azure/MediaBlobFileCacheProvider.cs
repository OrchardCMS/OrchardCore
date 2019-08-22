using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Logging;
using OrchardCore.FileStorage;
using OrchardCore.Media.Azure.Models;

namespace OrchardCore.Media.Azure
{
    public class MediaBlobFileCacheProvider : PhysicalFileProvider, IMediaFileProvider, IMediaCacheFileProvider, IMediaFileStoreCache
    {
        // Use default stream copy buffer size to stay in gen0 garbage collection;
        private const int StreamCopyBufferSize = 81920;

        private readonly ILogger<MediaBlobFileCacheProvider> _logger;

        public MediaBlobFileCacheProvider(ILogger<MediaBlobFileCacheProvider> logger, PathString virtualPathBase, string root) : base(root)
        {
            _logger = logger;
            VirtualPathBase = virtualPathBase;
        }

        public MediaBlobFileCacheProvider(ILogger<MediaBlobFileCacheProvider> logger, PathString virtualPathBase, string root, ExclusionFilters filters) : base(root, filters)
        {
            _logger = logger;
            VirtualPathBase = virtualPathBase;
        }

        public PathString VirtualPathBase { get; }

        public dynamic GetDisplayModel()
        {
            return new MediaBlobFileCache();
        }

        public Task<bool> IsCachedAsync(string path)
        {
            // TODO Consider how to provide a graceful cache period on this, 
            // so we do not serve a file from cache, if it has been deleted from Blob storage.
            // One idea: Treat this cache like any other Cdn cache. Cache bust it.
            // We would need another memory cache here of file hashes for cached files.
            // We could then compare the cached file hash with the file hash
            // from the IFileStoreVersionProvider to see if the hash is the same.
            // If it is not, then remove the file from the disc cache.
            // The IFileStoreVersionProvider then becomes responsible for
            // gracefully expiring it's own memory cache of version hashes.
            // However heavy on memory cache, for a large quantity of files, and requires much 
            // initial creation of sha hashes.

            // Date comparison is also an option, with the IFileStoreVersionProvider also caching
            // last modified dates, and matching here. Not sure the dates will match however.

            // Also we can now store metadata here, either in memory, or in a YesSQL table.
            // So that provides other / better options for storing hashes, or other metadata,
            // like cache lifecyle times.

            var fileInfo = GetFileInfo(path);
            return Task.FromResult(fileInfo.Exists);
        }

        public async Task SetCacheAsync(Stream stream, IFileStoreEntry fileStoreEntry, CancellationToken cancellationToken)
        {
            // File store semantics include a leading slash.
            var cachePath = Path.Combine(Root, fileStoreEntry.Path.Substring(1));
            var directory = Path.GetDirectoryName(cachePath);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (var fileStream = File.Create(cachePath))
            {
                await stream.CopyToAsync(fileStream, StreamCopyBufferSize, cancellationToken);
            }
        }


        public Task<bool> ClearCacheAsync()
        {
            var purgedWithErrors = false;

            //TODO consider a clear cache items older than xxx days option from the ui,
            // or a background task to do the same.
            var folders = GetDirectoryContents(String.Empty);
            foreach (var fileInfo in folders)
            {
                if (fileInfo.IsDirectory)
                {
                    try
                    {
                        Directory.Delete(fileInfo.PhysicalPath, true);
                    }
                    catch (IOException ex)
                    {
                        _logger.LogError(ex, "Error deleting cache folder {Path}", fileInfo.PhysicalPath);
                        purgedWithErrors = true;
                    }
                }
                else
                {
                    try
                    {
                        File.Delete(fileInfo.PhysicalPath);
                    }
                    catch (IOException ex)
                    {
                        _logger.LogError(ex, "Error deleting cache file {Path}", fileInfo.PhysicalPath);
                        purgedWithErrors = true;
                    }
                }
            }
            return Task.FromResult(purgedWithErrors);
        }
    }
}
