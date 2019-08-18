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
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Helpers;
using SixLabors.ImageSharp.Web.Middleware;
using SixLabors.ImageSharp.Web.Resolvers;

namespace OrchardCore.Media.Processing
{
    /// <summary>
    /// A cache for media assets, per tenant.
    /// </summary>
    public class MediaFileCache : IImageCache
    {
        private readonly IMediaCacheFileProvider _mediaCacheFileProvider;
        private readonly ImageSharpMiddlewareOptions _isOptions;
        private readonly FormatUtilities _formatUtilies;
        private readonly string _cachePath;

        public MediaFileCache(
            IMediaCacheFileProvider mediaCacheFileProvider,
            IOptions<ImageSharpMiddlewareOptions> isOptions,
            ILogger<MediaFileCache> logger
            )
        {
            _mediaCacheFileProvider = mediaCacheFileProvider;
            //TODO the is-cache could be in it's own folder.
            _cachePath = _mediaCacheFileProvider.Root;
            _isOptions = isOptions.Value;
            _formatUtilies = new FormatUtilities(_isOptions.Configuration);

            Logger = logger;
        }

        public ILogger Logger { get; }

        [Obsolete("This feature is unused and has been removed from ImageSharp.Web, remove when updating ImageSharp.Web.")]
        public IDictionary<string, string> Settings { get; }

        /// <inheritdoc/>
        public async Task<IImageResolver> GetAsync(string key)
        {
            // Remove extension to retrieve metadata, if extension has been applied.
            var extension = String.Empty;
            if (key.Contains("."))
            {
                extension = Path.GetExtension(key);
                key = Path.GetFileNameWithoutExtension(key);
            }

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

            // Apply extension back if supplied as part of cache key.
            if (!String.IsNullOrEmpty(extension))
            {
                path = path + extension;
            } else
            {
                path = this.ToImageFilePath(path, metadata);
            }

            IFileInfo fileInfo = this._mediaCacheFileProvider.GetFileInfo(path);

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

        /// <summary>
        /// Converts the key into a nested file path.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private string ToFilePath(string key) // TODO: Avoid the allocation here.
            => $"{string.Join("/", key.Substring(0, (int)_isOptions.CachedNameLength).ToCharArray())}/{key}";
    }
}
