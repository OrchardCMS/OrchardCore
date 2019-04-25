using System;
using System.Collections.Concurrent;
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
        private readonly string _cachePath;

        private readonly IFileProvider _fileProvider;

        private readonly ImageSharpMiddlewareOptions _options;

        private readonly FormatUtilities _formatUtilies;

        /// <summary>
        /// Maximum cache entries, disabled if 0
        /// </summary>
        private readonly int _maxCacheEntries;

        private readonly ConcurrentQueue<CacheEntry> _entries = new ConcurrentQueue<CacheEntry>();

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
                Task.Run(() => InitializeCacheEntries());
            }
        }

        public ILogger Logger { get; }

        private void InitializeCacheEntries()
        {
            try
            {
                var dirInfo = new DirectoryInfo(_cachePath);
                var files = dirInfo
                            .EnumerateDirectories()
                            .AsParallel()
                            .SelectMany(di => di.EnumerateFiles("*.*", SearchOption.AllDirectories))
                            .OrderBy(fi => fi.CreationTimeUtc)
                            .GroupBy(x => Path.GetFileNameWithoutExtension(x.Name))
                            .ToArray();

                //this won't work perfectly, as it's in the background, and an image maybe resized
                //and added to it during this processing after startup
                //we could enqueue resized images into a second queue, then clear it, at the end of this,
                //and not use the second queue anymore, but perfect cache integrity is not important
                //speed is
                foreach (var fileGroup in files)
                {
                    //TODO IS does not Evict from cache if options.MaxCacheDays is reached.
                    //it just refreshes on read (assuming image is read). So can remain dead in cache folder forever
                    //we could do this refresh here, but would need to run even if cache limiting is disabled (_maxMediaFileCacheEntries = 0)
                    //suspect non issue. either enable cache limiting and it will sort itself out
                    //(with x number images always dead in cache if using cdn), or don't cache limit
                    EnqueueCacheEntry(fileGroup.Key, fileGroup.Select(x => x.FullName).ToArray());
                }
                EvictCacheEntries();
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Erroring retrieving cache entries");
            }
        }

        private void EnqueueCacheEntry(string key, string[] filePaths)
        {
            _entries.Enqueue(new CacheEntry(key, filePaths));
        }

        //TODO obsolete this and use OC settings in constructor
        [Obsolete("Use OrchardCore.Media Settings to manage MediaFileCache")]
        public IDictionary<string, string> Settings { get; }

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
                EvictCacheEntries();
            }
        }

        private void EvictCacheEntries()
        {
            var _entriesToEvict = _entries.Count - _maxCacheEntries;
            if (_entriesToEvict > 0)
            {
                for (var i = 0; i < _entriesToEvict; i++)
                {
                    if (_entries.TryDequeue(out var entryToEvict))
                    {
                        try
                        {
                            foreach (var file in entryToEvict.FilePaths)
                            {
                                File.Delete(file);
                                Logger.LogDebug($"Evicting cache entry {file}");
                            }
                        }
                        //catch if file locked by PhysicalFileSystemResolver.OpenReadStream()
                        catch (Exception e)
                        {
                            Logger.LogWarning(e, $"Could not delete cache entry {entryToEvict.FileKey}");
                            //return to end cache for deletion another time.
                            //one file may no longer be present in cacheEntry.FilePaths
                            //however it will not throw when trying to delete if missing.
                            //multiple keys may end up in cache list, however cache key is irrelevant,
                            //and we do not rely on cache key to determine existance in cache
                            //so perfect integrity is not important.
                            //speed is preferable.
                            _entries.Enqueue(entryToEvict);
                        }
                    };
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
        private string ToFilePath(string key) => $"{string.Join("/", key.Substring(0, (int)_options.CachedNameLength).ToCharArray())}/{key}";

        private string GetCachePath(ShellOptions shellOptions, ShellSettings shellSettings, string cacheFolder)
        {
            return Path.Combine(shellOptions.ShellsApplicationDataPath, shellOptions.ShellsContainerName, shellSettings.Name, cacheFolder);
        }
    }
}