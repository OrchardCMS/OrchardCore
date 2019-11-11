using System.IO;
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
            var path = GetSitemapCachePath(webHostEnvironment, SitemapCachePath, shellSettings);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            _fileProvider = new PhysicalFileProvider(path);

            _logger = logger;
        }

        public Task<ISitemapCacheFileResolver> GetCachedSitemapAsync(string path)
        {
            var fileInfo = _fileProvider.GetFileInfo(path);
            if (fileInfo.Exists)
            {
                return Task.FromResult<ISitemapCacheFileResolver>(new PhysicalSitemapCacheFileResolver(fileInfo));
            }

            return Task.FromResult<ISitemapCacheFileResolver>(null);
        }

        public async Task SetSitemapCacheAsync(Stream stream, string path, CancellationToken cancellationToken)
        {
            var cachePath = Path.Combine(_fileProvider.Root, path);

            using (var fileStream = File.Create(cachePath))
            {
                stream.Position = 0;
                await stream.CopyToAsync(fileStream, StreamCopyBufferSize, cancellationToken);
            }
        }

        public Task ClearSitemapCacheAsync(string path)
        {
            var fileInfo = _fileProvider.GetFileInfo(path);
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

        private string GetSitemapCachePath(IWebHostEnvironment webHostEnvironment, string cachePath, ShellSettings shellSettings)
        {
            return PathExtensions.Combine(webHostEnvironment.WebRootPath, cachePath, shellSettings.Name);
        }
    }
}
