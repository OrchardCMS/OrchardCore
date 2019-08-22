using System.Threading.Tasks;
using OrchardCore.Media.Azure.Models;

namespace OrchardCore.Media.Azure.Services
{
    public class MediaBlobCacheManagementProvider : IMediaCacheManagementProvider
    {
        private readonly static MediaBlobFileCache _displayModel = new MediaBlobFileCache();

        private readonly MediaBlobFileCacheProvider _mediaBlobFileCacheProvider;
        public MediaBlobCacheManagementProvider(
            MediaBlobFileCacheProvider mediaBlobFileCacheProvider
            )
        {
            _mediaBlobFileCacheProvider = mediaBlobFileCacheProvider;
        }

        public string Name => typeof(MediaBlobFileCache).Name;

        public Task<bool> ClearCacheAsync()
        {
            return _mediaBlobFileCacheProvider.ClearCacheAsync();
        }

        public dynamic GetDisplayModel()
        {
            return _displayModel;
        }
    }
}
