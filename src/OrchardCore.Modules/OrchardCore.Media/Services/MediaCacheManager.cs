using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace OrchardCore.Media.Services
{
    public class MediaCacheManager : IMediaCacheManager
    {
        private readonly ILogger<MediaCacheManager> _logger;
        private readonly IMediaCacheFileProvider _mediaCacheFileProvider;
        public MediaCacheManager(
            ILogger<MediaCacheManager> logger,
            IMediaCacheFileProvider mediaCacheFileProvider
            )
        {
            _logger = logger;
            _mediaCacheFileProvider = mediaCacheFileProvider;
        }

        public Task<bool> ClearMediaCacheAsync()
        {
            bool purgedWithErrors = false;
            //TODO consider a clear cache items older than xxx days option from the ui,
            // or a background task to do the same.
            var folders = _mediaCacheFileProvider.GetDirectoryContents(String.Empty);
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
