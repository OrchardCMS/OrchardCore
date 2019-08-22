using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.Modules;

namespace OrchardCore.Media.Services
{

    [Feature("OrchardCore.Media.MediaCache")]
    public class MediaCacheManager : IMediaCacheManager
    {
        //private readonly ILogger<MediaCacheManager> _logger;
        private readonly IEnumerable<IMediaFileStoreCache> _mediaFileStoreCaches;

        //TODO change this to IEnumerable IMediaFileStoreCache (Provider) and collected instances for UI display.
        // so (simplified) IMediaFileStoreCache as parent, with children of supported IMediaCachePurgeProvider
        // i.e. Azure Blob Cache (H5)
        // provides children of
        // 1) Required : Purge All
        // 2) Optional : Purge by date / purge by name / whatever UI the optionals provide.

        // Plus this the last place that DEPENDS on IMediaCacheFileProvider which may not be registered now.
        public MediaCacheManager(
            //ILogger<MediaCacheManager> logger,
            IEnumerable<IMediaFileStoreCache> mediaFileStoreCaches
            )
        {
            //_logger = logger;
            // TODO So This should inject enumerables. of storecache
            _mediaFileStoreCaches = mediaFileStoreCaches;
        }

        public Task<bool> ClearMediaCacheAsync()
        {
            return _mediaFileStoreCaches.FirstOrDefault().ClearCacheAsync();
        }

    }
}
