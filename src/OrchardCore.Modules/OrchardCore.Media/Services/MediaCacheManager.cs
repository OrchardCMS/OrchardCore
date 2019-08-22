using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Media.Services
{
    public class MediaCacheManager : IMediaCacheManager
    {
        private readonly IEnumerable<IMediaCacheManagementProvider> _mediaCacheManagementProviders;

        public MediaCacheManager(
            IEnumerable<IMediaCacheManagementProvider> mediaCacheManagementProviders
            )
        {
            _mediaCacheManagementProviders = mediaCacheManagementProviders;
        }

        public Task<bool> ClearMediaCacheAsync(string cacheName)
        {
            var provider = _mediaCacheManagementProviders.FirstOrDefault(x => x.Name == cacheName);
            if (provider != null)
            {
                return provider.ClearCacheAsync();
            }

            return Task.FromResult(true);
        }

        public IEnumerable<dynamic> GetCaches()
        {
            var caches = new List<dynamic>();
            foreach (var provider in _mediaCacheManagementProviders)
            {
                caches.Add(provider.GetDisplayModel());
            }
            return caches;
        }
    }
}
