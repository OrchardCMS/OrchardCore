// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Helpers;
using SixLabors.ImageSharp.Web.Middleware;
using SixLabors.ImageSharp.Web.Resolvers;
using SixLabors.Memory;

namespace OrchardCore.Media.Processing
{
    //to replace this cache (with a blob storage one, for example, replace the registration against IImagecache with a new implementation)

    public class MediaFileCache : IImageCache
    {
        public const string Folder = "CacheFolder";

        public const string DefaultCacheFolder = "MediaCache";

        private readonly string _cacheRoot;

        private readonly IFileProvider _fileProvider;

        private readonly ImageSharpMiddlewareOptions _options;

        private readonly FormatUtilities _formatUtilies;

        public MediaFileCache(
            IOptions<ImageSharpMiddlewareOptions> options,
            IOptions<ShellOptions> shellOptions,
            ShellSettings shellSettings
            )
        {
            _cacheRoot = GetCacheRoot(shellOptions.Value, shellSettings);

            if (!Directory.Exists(_cacheRoot))
            {
                Directory.CreateDirectory(_cacheRoot);
            }
            _fileProvider = new PhysicalFileProvider(_cacheRoot);
            _options = options.Value;
            _formatUtilies = new FormatUtilities(this._options.Configuration);
        }

        //TODO use this (follow the IS pattern), but register it based on OC settings
        //Also pass the other cache related settings in here
        public IDictionary<string, string> Settings { get; }
            = new Dictionary<string, string>
            {
                { Folder, DefaultCacheFolder }
            };

        public async Task<IImageResolver> GetAsync(string key)
        {
            var path = ToFilePath(key);

            var metaFileInfo = _fileProvider.GetFileInfo(ToMetaDataFilePath(path));
            if (!metaFileInfo.Exists)
            {
                return null;
            }

            ImageMetaData metadata = default;
            using (var stream = metaFileInfo.CreateReadStream())
            {
                metadata = await ImageMetaData.ReadAsync(stream).ConfigureAwait(false);
            }

            var fileInfo = _fileProvider.GetFileInfo(ToImageFilePath(path, metadata));

            // Check to see if the file exists.
            if (!fileInfo.Exists)
            {
                return null;
            }

            return new PhysicalFileSystemResolver(fileInfo, metadata);
        }

        public async Task SetAsync(string key, Stream stream, ImageMetaData metadata)
        {
            var path = Path.Combine(_cacheRoot, ToFilePath(key));
            var imagePath = ToImageFilePath(path, metadata);
            var metaPath = ToMetaDataFilePath(path);
            var directory = Path.GetDirectoryName(path);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (var fileStream = File.Create(imagePath))
            {
                await stream.CopyToAsync(fileStream).ConfigureAwait(false);
            }

            using (var fileStream = File.Create(metaPath))
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
        private string ToImageFilePath(string path, in ImageMetaData metaData) => $"{path}.{_formatUtilies.GetExtensionFromContentType(metaData.ContentType)}";

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
        //private string ToFilePath(string key) => $"{string.Join("/", key.Substring(0, (int)_options.CachedNameLength).ToCharArray())}/{key}";

            //TODO renable this - just disabled for easy debugging of file system
        private string ToFilePath(string key) => key;

        private string GetCacheRoot(ShellOptions shellOptions, ShellSettings shellSettings)
        {
            return Path.Combine(shellOptions.ShellsApplicationDataPath, shellOptions.ShellsContainerName, shellSettings.Name, Settings[Folder]);
        }
    }
}