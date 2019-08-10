using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Helpers;
using SixLabors.ImageSharp.Web.Middleware;
using SixLabors.ImageSharp.Web.Resolvers;

namespace OrchardCore.Media.Processing
{
    /// <summary>
    /// A cache for media assets, per tenant.
    /// </summary>
    public class MediaFileCache : IMediaImageCache
    {
        // Use a light locking mechanism.
        // This is only ever used to stop multiple writes occuring to the same cache file.
        // It makes reads from the cache much faster, as there is no locking performed at all.
        // It means on the first request of a file that is not in the cache, if multiple requests are made
        // at the same time, none of these requests will be served by the cache, but the requests will only
        // cause one cache write.
        private static readonly ConcurrentDictionary<string, string> _cacheWriteLock = new ConcurrentDictionary<string, string>();

        private readonly IFileProvider _fileProvider;
        private readonly ImageSharpMiddlewareOptions _isOptions;
        private readonly FormatUtilities _formatUtilies;
        private readonly string _cachePath;

        public MediaFileCache(
            IOptions<ImageSharpMiddlewareOptions> isOptions,
            IOptions<MediaOptions> mediaOptions,
            IOptions<ShellOptions> shellOptions,
            ShellSettings shellSettings,
            ILogger<MediaFileCache> logger
            )
        {
            _isOptions = isOptions.Value;
            _formatUtilies = new FormatUtilities(_isOptions.Configuration);

            _cachePath = GetMediaCachePath(shellOptions.Value, shellSettings, mediaOptions.Value.AssetsCachePath);

            if (!Directory.Exists(_cachePath))
            {
                Directory.CreateDirectory(_cachePath);
            }
            _fileProvider = new PhysicalFileProvider(_cachePath);

            Logger = logger;
        }

        public ILogger Logger { get; }

        [Obsolete("This feature is unused and has been removed from ImageSharp.Web, remove when updating ImageSharp.Web.")]
        public IDictionary<string, string> Settings { get; }

        /// <inheritdoc/>
        public Task ClearMediaCacheAsync()
        {
            //TODO consider a clear cache items older than xxx days option from the ui,
            // or a background task to do the same.
            var folders = _fileProvider.GetDirectoryContents(String.Empty);
            foreach(var fileInfo in folders)
            {
                if (fileInfo.IsDirectory)
                {
                    Directory.Delete(fileInfo.PhysicalPath, true);
                } else
                {
                    File.Delete(fileInfo.PhysicalPath);
                }
            }
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task<IMediaCacheFileResolver> GetMediaCacheFileAsync(string key)
        {
            return (IMediaCacheFileResolver)await this.GetAsync(key);
        }

        /// <inheritdoc/>
        public async Task<IImageResolver> GetAsync(string key)
        {
            string path = this.ToFilePath(key);

            IFileInfo metaFileInfo = this._fileProvider.GetFileInfo(this.ToMetaDataFilePath(path));
            if (!metaFileInfo.Exists)
            {
                return null;
            }

            ImageMetaData metadata = default;
            using (Stream stream = metaFileInfo.CreateReadStream())
            {
                metadata = await ImageMetaData.ReadAsync(stream).ConfigureAwait(false);
            }

            IFileInfo fileInfo = this._fileProvider.GetFileInfo(this.ToImageFilePath(path, metadata));

            // Check to see if the file exists.
            if (!fileInfo.Exists)
            {
                return null;
            }

            return new MediaCacheFileResolver(fileInfo, metadata);
        }

        /// <inheritdoc/>
        public async Task TrySetAsync(string key, string extension, Stream stream, ImageMetaData metadata)
        {
            // Add the cache key, if it fails, a write operation is already underway.
            if (!_cacheWriteLock.TryAdd(key, null))
                return;

            var imagePath = String.Empty;
            try
            {
                // File Provider will handle normalization of path.
                var path = Path.Combine(_cachePath, this.ToFilePath(key));
                imagePath = path + extension;
                var metaPath = this.ToMetaDataFilePath(path);
                var directory = Path.GetDirectoryName(imagePath);

                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using (var fileStream = File.Create(imagePath))
                {
                    await stream.CopyToAsync(fileStream);
                }

                using (var fileStream = File.Create(metaPath))
                {
                    await metadata.WriteAsync(fileStream).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error saving media cache file {0}", imagePath);
            }
            finally
            {
                _cacheWriteLock.TryRemove(key, out var removedPath);
            }
        }

        /// <inheritdoc/>
        public async Task SetAsync(string key, Stream stream, ImageMetaData metadata)
        {
            string path = Path.Combine(_cachePath, this.ToFilePath(key));
            string imagePath = this.ToImageFilePath(path, metadata);
            string metaPath = this.ToMetaDataFilePath(path);
            string directory = Path.GetDirectoryName(path);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (FileStream fileStream = File.Create(imagePath))
            {
                await stream.CopyToAsync(fileStream).ConfigureAwait(false);
            }

            using (FileStream fileStream = File.Create(metaPath))
            {
                await metadata.WriteAsync(fileStream).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Gets the path to the image file based on the supplied root and metadata.
        /// </summary>
        /// <param name="path">The root path.</param>
        /// <param name="metaData">The image metadata.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private string ToImageFilePath(string path, in ImageMetaData metaData)
            => $"{path}.{_formatUtilies.GetExtensionFromContentType(metaData.ContentType)}";

        /// <summary>
        /// Gets the path to the image file based on the supplied root.
        /// </summary>
        /// <param name="path">The root path.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private string ToMetaDataFilePath(string path) => $"{path}.meta";


        /// <summary>
        /// Converts the key into a nested file path.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private string ToFilePath(string key) // TODO: Avoid the allocation here.
            => $"{_cachePath}/{string.Join("/", key.Substring(0, (int)_isOptions.CachedNameLength).ToCharArray())}/{key}";

        private static string GetMediaCachePath(ShellOptions shellOptions, ShellSettings shellSettings, string _assetsCachePath)
        {
            return PathExtensions.Combine(shellOptions.ShellsApplicationDataPath, shellOptions.ShellsContainerName, shellSettings.Name, _assetsCachePath);
        }
    }
}
