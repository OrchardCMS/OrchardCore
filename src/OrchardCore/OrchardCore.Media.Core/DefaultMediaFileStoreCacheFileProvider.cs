using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Logging;
using OrchardCore.FileStorage;

namespace OrchardCore.Media.Core
{
    public class DefaultMediaFileStoreCacheFileProvider : PhysicalFileProvider, IMediaFileStoreCacheFileProvider
    {
        /// <summary>
        /// The path in the wwwroot folder containing the asset cache.
        /// The tenants name will be prepended to this path.
        /// </summary>
        public const string AssetsCachePath = "ms-cache";

        // Use default stream copy buffer size to stay in gen0 garbage collection.
        private const int StreamCopyBufferSize = 81920;

        private readonly ILogger _logger;

        public DefaultMediaFileStoreCacheFileProvider(ILogger<DefaultMediaFileStoreCacheFileProvider> logger, PathString virtualPathBase, string root) : base(root)
        {
            _logger = logger;
            VirtualPathBase = virtualPathBase;
        }

        public DefaultMediaFileStoreCacheFileProvider(ILogger<DefaultMediaFileStoreCacheFileProvider> logger, PathString virtualPathBase, string root, ExclusionFilters filters) : base(root, filters)
        {
            _logger = logger;
            VirtualPathBase = virtualPathBase;
        }

        public PathString VirtualPathBase { get; }

        public Task<bool> IsCachedAsync(string path)
        {
            // Opportunity here to save metadata and/or provide cache validation / integrity checks.

            var fileInfo = GetFileInfo(path);
            return Task.FromResult(fileInfo.Exists);
        }

        public async Task SetCacheAsync(Stream stream, IFileStoreEntry fileStoreEntry, CancellationToken cancellationToken)
        {
            // File store semantics include a leading slash.
            var cachePath = Path.Combine(Root, fileStoreEntry.Path[1..]);
            var directory = Path.GetDirectoryName(cachePath);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // A file download may fail, so a partially downloaded file should be deleted so the next request can reprocess.
            // All exceptions here are recaught by the MediaFileStoreResolverMiddleware.
            try
            {
                if (File.Exists(cachePath))
                {
                    File.Delete(cachePath);
                }

                using var fileStream = File.Create(cachePath);
                await stream.CopyToAsync(fileStream, StreamCopyBufferSize, CancellationToken.None);
                await stream.FlushAsync(CancellationToken.None);

                if (fileStream.Length == 0)
                {
                    throw new Exception($"Error retrieving file (length equals 0 byte) : {cachePath}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving file {Path}", cachePath);
                if (File.Exists(cachePath))
                {
                    try
                    {
                        File.Delete(cachePath);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Error deleting file {Path}", cachePath);
                        throw;
                    }
                }
                throw;
            }
        }

        public Task<bool> PurgeAsync()
        {
            var hasErrors = false;
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
                        hasErrors = true;
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
                        hasErrors = true;
                    }
                }
            }

            return Task.FromResult(hasErrors);
        }

        public Task<bool> TryDeleteDirectoryAsync(string path)
        {
            var directoryInfo = GetFileInfo(path);

            try
            {
                Directory.Delete(directoryInfo.PhysicalPath, true);
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "Error deleting cache folder {Path}", directoryInfo.PhysicalPath);
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        public Task<bool> TryDeleteFileAsync(string path)
        {
            var fileInfo = GetFileInfo(path);

            if (fileInfo.Exists)
            {
                try
                {
                    File.Delete(fileInfo.PhysicalPath);
                    return Task.FromResult(true);
                }
                catch (IOException ex)
                {
                    _logger.LogError(ex, "Error deleting cache file {Path}", fileInfo.PhysicalPath);
                    return Task.FromResult(false);
                }
            }
            else
            {
                return Task.FromResult(false);
            }
        }
    }
}
