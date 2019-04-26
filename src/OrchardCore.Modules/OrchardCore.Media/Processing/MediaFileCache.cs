// Modified from PhysicalFileSystemCache.cs under the Apache License
// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Helpers;
using SixLabors.ImageSharp.Web.Middleware;
using SixLabors.ImageSharp.Web.Resolvers;

namespace OrchardCore.Media.Processing
{
    public class MediaFileCache : IImageCache
    {
        private static readonly AsyncKeyLock AsyncLock = new AsyncKeyLock();

        private readonly string _cachePath;

        private readonly IFileProvider _fileProvider;

        private readonly ImageSharpMiddlewareOptions _options;

        private readonly FormatUtilities _formatUtilies;

        /// <summary>
        /// Maximum cache entries, disabled if 0
        /// </summary>
        private readonly int _maxCacheEntries;

        private static readonly object _cacheEntrylock = new object();

        private readonly LinkedList<CacheEntry> _cacheEntries = new LinkedList<CacheEntry>();

        public MediaFileCache(
            ILogger<MediaFileCache> logger,
            IOptions<ImageSharpMiddlewareOptions> options,
            IOptions<ShellOptions> shellOptions,
            ShellSettings shellSettings,
            IShellConfiguration shellConfiguration
            )
        {
            Logger = logger;
            var configurationSection = shellConfiguration.GetSection("OrchardCore.Media");
            _maxCacheEntries = configurationSection.GetValue("MaxCacheEntries", 0);
            var mediaCachePath = configurationSection.GetValue("MediaCachePath", "MediaCache");

            _cachePath = GetCachePath(shellOptions.Value, shellSettings, mediaCachePath);

            if (!Directory.Exists(_cachePath))
            {
                Directory.CreateDirectory(_cachePath);
            }
            _fileProvider = new PhysicalFileProvider(_cachePath);
            _options = options.Value;
            _formatUtilies = new FormatUtilities(_options.Configuration);

            //fire and forget, do not care about exceptions, better to not impact startup speed.
            if (_maxCacheEntries != 0)
            {
                Task.Run(async () => await InitializeCacheEntries());
            }
        }

        public ILogger Logger { get; }

        private async Task InitializeCacheEntries()
        {
            try
            {
                var dirInfo = new DirectoryInfo(_cachePath);

                var files = dirInfo
                    .EnumerateFiles("*.*", SearchOption.TopDirectoryOnly)
                    .AsParallel()
                    .OrderBy(fi => fi.LastAccessTimeUtc)
                    .GroupBy(x => Path.GetFileNameWithoutExtension(x.Name))
                    .ToArray();
                //this won't work perfectly, as it's in the background, and an image may be resized
                //and added to it during this initialization
                //we could enqueue resized images into a second queue, then clear it, at the end of init,
                //and not use the second queue anymore, but perfect cache integrity is not important
                //speed is
                foreach (var fileGroup in files)
                {
                    //TODO IS does not Evict from cache if options.MaxCacheDays is reached.
                    //it just refreshes on read (assuming image is read). So can remain dead in cache folder forever
                    //we could do an eviction here, but would need to run even if cache limiting is disabled (_maxMediaFileCacheEntries = 0)
                    //suspect non issue. either enable cache limiting and it will sort itself out
                    //(with x number images always dead in cache if using cdn), or don't use cache limiting
                    EnqueueCacheEntry(fileGroup.Key, fileGroup.Select(x => x.FullName).ToArray());
                }
                await EvictCacheEntries();
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Erroring retrieving cache entries");
            }
        }

        private void EnqueueCacheEntry(string key, string[] filePaths)
        {
            lock (_cacheEntrylock)
            {
                _cacheEntries.AddLast(new CacheEntry(key, filePaths));
            }
        }

        [Obsolete("Use OrchardCore.Media Settings to manage MediaFileCache")]
        public IDictionary<string, string> Settings { get; }

        public async Task<IImageResolver> GetAsync(string key)
        {
            //use asynclock if it's delete locked on key
            //if locked then it should wait until unlocked, and then will return null because it's been deleted
            //if not locked, find in list, move to end of list, so will not be deleted anytime soon
            if (_maxCacheEntries != 0)
            {
                using (await AsyncLock.ReaderLockAsync(key).ConfigureAwait(false))
                {
                    //move to end of deletion queue
                    lock (_cacheEntrylock)
                    {
                        var cacheEntry = _cacheEntries.FirstOrDefault(x => x.FileKey == key);
                        if (cacheEntry != null)
                        {
                            _cacheEntries.Remove(cacheEntry);
                            _cacheEntries.AddLast(cacheEntry);
                        }
                    }
                    return await GetCacheImageAsync(key);
                }
            } else
            {
                return await GetCacheImageAsync(key);
            }
        }

        private async Task<IImageResolver> GetCacheImageAsync(string key)
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
            var path = Path.Combine(_cachePath, ToFilePath(key));
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

            if (_maxCacheEntries != 0)
            {
                EnqueueCacheEntry(key, new string[] { imagePath, metaPath });
                await EvictCacheEntries();
            }
        }

        private async Task EvictCacheEntries()
        {
            var _entriesToEvict = _cacheEntries.Count - _maxCacheEntries;
            if (_entriesToEvict > 0)
            {
                for (var i = 0; i < _entriesToEvict; i++)
                {
                    CacheEntry entryToEvict = null;
                    //find entry to evict, then remove it, so no reentry will try and delete it
                    lock (_cacheEntrylock)
                    {
                        entryToEvict = _cacheEntries.First();
                        _cacheEntries.RemoveFirst();
                    }
                    //then writelock it so that delete can complete before a read is attempted
                    using (await AsyncLock.WriterLockAsync(entryToEvict.FileKey).ConfigureAwait(false))
                    {
                        foreach (var file in entryToEvict.FilePaths)
                        {
                            try
                            {
                                File.Delete(file);
                                Logger.LogDebug($"Evicting cache entry {file}");
                            } //catch just in case
                            catch (IOException e)
                            {
                                Logger.LogWarning(e, $"Could not delete cache entry {file}");
                                //return to entry cache for deletion another time.
                                //one file may no longer be present in cacheEntry.FilePaths
                                //however it will not throw when trying to delete if missing.
                                //multiple files may end up in cache list, 
                                //so perfect integrity is not important.
                                //speed is preferable.
                                _cacheEntries.AddLast(entryToEvict);
                                break;
                            }

                        };
                    }
                }
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
        /// Gets the path to the metadata file based on the supplied root.
        /// </summary>
        /// <param name="path">The root path.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private string ToMetaDataFilePath(string path) => $"{path}.meta";

        /// <summary>
        /// Converts the key into a unnested file path.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <returns>The <see cref="string"/>.</returns>
        // Disabled nesting, as believe that just an obfuscation for when cache images stored
        // in /wwwroot and available through static files.
        // no longer available through static files, and this stops the need to recursivly delete
        // directories that may or may not be empty at different nested levels.

        //private string ToFilePath(string key) => $"{string.Join("/", key.Substring(0, (int)_options.CachedNameLength).ToCharArray())}/{key}";
        private string ToFilePath(string key) => key;

        private string GetCachePath(ShellOptions shellOptions, ShellSettings shellSettings, string cacheFolder)
        {
            return Path.Combine(shellOptions.ShellsApplicationDataPath, shellOptions.ShellsContainerName, shellSettings.Name, cacheFolder);
        }
    }
}