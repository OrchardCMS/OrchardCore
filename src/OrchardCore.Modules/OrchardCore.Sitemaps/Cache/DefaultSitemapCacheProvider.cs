using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Sitemaps.Cache
{
    public class DefaultSitemapCacheProvider : ISitemapCacheProvider
    {
        // Use default stream copy buffer size to stay in gen0 garbage collection.
        private const int StreamCopyBufferSize = 81920;

        public const string SitemapCachePath = "sm-cache";

        private readonly PhysicalFileProvider _fileProvider;
        private readonly ILogger _logger;

        public DefaultSitemapCacheProvider(
            IWebHostEnvironment webHostEnvironment,
            ShellSettings shellSettings,
            ILogger<DefaultSitemapCacheProvider> logger
            )
        {
            var path = GetSitemapCachePath(webHostEnvironment, shellSettings, SitemapCachePath);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            _fileProvider = new PhysicalFileProvider(path);

            _logger = logger;
        }

        public Task<ISitemapCacheFileResolver> GetCachedSitemapAsync(string cacheFileName)
        {
            var fileInfo = _fileProvider.GetFileInfo(cacheFileName);
            if (fileInfo.Exists)
            {
                return Task.FromResult<ISitemapCacheFileResolver>(new PhysicalSitemapCacheFileResolver(fileInfo));
            }

            return Task.FromResult<ISitemapCacheFileResolver>(null);
        }

        public async Task SetSitemapCacheAsync(Stream stream, string cacheFileName, CancellationToken cancellationToken)
        {
            var cachePath = Path.Combine(_fileProvider.Root, cacheFileName);

            using var fileStream = File.Create(cachePath);
            stream.Position = 0;
            await stream.CopyToAsync(fileStream, StreamCopyBufferSize, cancellationToken);
        }

        public Task CleanSitemapCacheAsync(IEnumerable<string> validCacheFileNames)
        {
            var folders = _fileProvider.GetDirectoryContents(String.Empty);
            foreach (var fileInfo in folders)
            {
                if (fileInfo.IsDirectory)
                {
                    // Sitemap cache only stores files, so any folder that has been created by the user will be ignored.
                    continue;
                }
                else
                {
                    // Check if the file is valid and still needs to be cached.
                    if (validCacheFileNames.Contains(fileInfo.Name, StringComparer.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    try
                    {
                        File.Delete(fileInfo.PhysicalPath);
                    }
                    catch (IOException ex)
                    {
                        _logger.LogError(ex, "Error deleting cache file {Path}", fileInfo.PhysicalPath);
                    }
                }
            }

            return Task.CompletedTask;
        }

        public Task ClearSitemapCacheAsync(string cacheFileName)
        {
            var fileInfo = _fileProvider.GetFileInfo(cacheFileName);
            if (fileInfo.Exists)
            {
                try
                {
                    File.Delete(fileInfo.PhysicalPath);
                }
                catch (IOException ex)
                {
                    _logger.LogError(ex, "Error deleting cache file {Path}", fileInfo.PhysicalPath);
                }
            }

            return Task.CompletedTask;
        }

        public Task<bool> PurgeAllAsync()
        {
            var hasErrors = false;
            var folders = _fileProvider.GetDirectoryContents(String.Empty);
            foreach (var fileInfo in folders)
            {
                if (fileInfo.IsDirectory)
                {
                    // Sitemap cache only stores files, so any folder that has been created by the user will be ignored.
                    continue;
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

        public Task<bool> PurgeAsync(string cacheFileName)
        {
            var failed = false;
            var fileInfo = _fileProvider.GetFileInfo(cacheFileName);
            if (fileInfo.Exists)
            {
                try
                {
                    File.Delete(fileInfo.PhysicalPath);
                }
                catch (IOException ex)
                {
                    _logger.LogError(ex, "Error deleting cache file {Path}", fileInfo.PhysicalPath);
                    failed = true;
                }
            }
            else
            {
                _logger.LogError("Cache file {Name} does not exist", cacheFileName);
                failed = true;
            }

            return Task.FromResult(failed);
        }

        public Task<IEnumerable<string>> ListAsync()
        {
            var results = new List<string>();
            var folders = _fileProvider.GetDirectoryContents(String.Empty);
            foreach (var fileInfo in folders)
            {
                if (fileInfo.IsDirectory)
                {
                    // Sitemap cache only stores files, so any folder that has been created by the user will be ignored.
                    continue;
                }
                else
                {
                    results.Add(fileInfo.Name);
                }
            }

            return Task.FromResult<IEnumerable<string>>(results);
        }

        private static string GetSitemapCachePath(IWebHostEnvironment webHostEnvironment, ShellSettings shellSettings, string cachePath)
        {
            return PathExtensions.Combine(webHostEnvironment.WebRootPath, shellSettings.Name, cachePath);
        }
    }
}
