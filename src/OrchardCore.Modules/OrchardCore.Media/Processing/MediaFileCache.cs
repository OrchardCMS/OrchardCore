using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Helpers;
using SixLabors.ImageSharp.Web.Middleware;
using SixLabors.ImageSharp.Web.Resolvers;

namespace OrchardCore.Media.Processing
{
    /// <summary>
    /// A cache for media assets adapted under the Apache License, from ImageSharp.Web.
    /// </summary>
    public class MediaFileCache : IShellImageCache
    {
        private static readonly ConcurrentDictionary<string, string> _cacheLock = new ConcurrentDictionary<string, string>();

        private readonly IMediaCacheFileProvider _mediaCacheFileProvider;
        private readonly ImageSharpMiddlewareOptions _isOptions;
        private readonly FormatUtilities _formatUtilies;
        private readonly string _cachePath;

        public MediaFileCache(
            IMediaCacheFileProvider mediaCacheFileProvider,
            IOptions<ImageSharpMiddlewareOptions> isOptions,
            IOptions<MediaOptions> mediaOptions,
            IOptions<ShellOptions> shellOptions,
            ShellSettings shellSettings
            )
        {
            _mediaCacheFileProvider = mediaCacheFileProvider;
            _isOptions = isOptions.Value;
            _formatUtilies = new FormatUtilities(_isOptions.Configuration);

            _cachePath = GetMediaCachePath(shellOptions.Value, shellSettings, mediaOptions.Value.AssetsCachePath);

            if (!Directory.Exists(_cachePath))
            {
                Directory.CreateDirectory(_cachePath);
            }
        }

        public async Task TrySetAsync(string cacheFilePath, Stream stream)
        {
            // Add the cache key, if it fails, a write operation is already underway.
            if (!_cacheLock.TryAdd(cacheFilePath, null))
                return;

            // Remove leading slash required for PathString, File Provider will handle normalization of path
            var imagePath = Path.Combine(_cachePath, cacheFilePath.TrimStart('/'));
            var directory = Path.GetDirectoryName(imagePath);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // If file already exists, another write operation has created it.
            if (File.Exists(imagePath))
            {
                _cacheLock.TryRemove(cacheFilePath, out cacheFilePath);
                return;
            }

            using (var fileStream = File.Create(imagePath))
            {
                await stream.CopyToAsync(fileStream);
                _cacheLock.TryRemove(cacheFilePath, out cacheFilePath);
            }
        }

        [Obsolete("This feature is unused and has been removed from ImageSharp.Web, remove when updating ImageSharp.Web.")]
        public IDictionary<string, string> Settings { get; }

        public async Task<IImageResolver> GetAsync(string key)
        {
            string path = this.ToFilePath(key);

            IFileInfo metaFileInfo = this._mediaCacheFileProvider.GetFileInfo(this.ToMetaDataFilePath(path));
            if (!metaFileInfo.Exists)
            {
                return null;
            }

            ImageMetaData metadata = default;
            using (Stream stream = metaFileInfo.CreateReadStream())
            {
                metadata = await ImageMetaData.ReadAsync(stream).ConfigureAwait(false);
            }

            IFileInfo fileInfo = this._mediaCacheFileProvider.GetFileInfo(this.ToImageFilePath(path, metadata));

            // Check to see if the file exists.
            if (!fileInfo.Exists)
            {
                return null;
            }

            return new PhysicalFileSystemResolver(fileInfo, metadata);
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

        //TODO reenable nested file path when testing complete
        private string ToFilePath(string key) => key;

        ///// <summary>
        ///// Converts the key into a nested file path.
        ///// </summary>
        ///// <param name="key">The cache key.</param>
        ///// <returns>The <see cref="string"/>.</returns>
        //private string ToFilePath(string key) // TODO: Avoid the allocation here.
        //    => $"{_cacheOptions.CacheFolder}/{string.Join("/", key.Substring(0, (int)this._options.CachedNameLength).ToCharArray())}/{key}";

        public static string GetMediaCachePath(ShellOptions shellOptions, ShellSettings shellSettings, string _assetsCachePath)
        {
            return PathExtensions.Combine(shellOptions.ShellsApplicationDataPath, shellOptions.ShellsContainerName, shellSettings.Name, _assetsCachePath);
        }
    }
}
