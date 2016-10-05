using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Orchard.Environment.Cache;

namespace Orchard.Environment.Extensions.Features
{
    public class FeatureHash : IFeatureHash
    {
        private const string FeatureHashCacheKey = "FeatureHash:Features";

        private readonly IFeatureManager _featureManager;
        private readonly IMemoryCache _memoryCache;
        private readonly ISignal _signal;

        public FeatureHash(
            IFeatureManager featureManager,
            IMemoryCache memoryCache,
            ISignal signal)
        {
            _memoryCache = memoryCache;
            _featureManager = featureManager;
            _signal = signal;
        }

        public async Task<int> GetFeatureHashAsync()
        {
            int serial;
            if (_memoryCache.TryGetValue(FeatureHashCacheKey, out serial))
            {
                return serial;
            }

            // Calculate a hash of all enabled features' name
            serial = (await _featureManager.GetEnabledFeaturesAsync())
                .OrderBy(x => x.Name)
                .Aggregate(0, (a, f) => a * 7 + f.Name.GetHashCode());

            var options = new MemoryCacheEntryOptions()
                .AddExpirationToken(_signal.GetToken(FeatureManager.FeatureManagerCacheKey));

            _memoryCache.Set(FeatureHashCacheKey, serial, options);

            return serial;
        }

        public async Task<int> GetFeatureHashAsync(string featureId)
        {
            var cacheKey = FeatureHashCacheKey + ":" + featureId;
            bool enabled;
            if (!_memoryCache.TryGetValue(cacheKey, out enabled))
            {
                enabled = 
                    (await _featureManager.GetEnabledFeaturesAsync())
                    .Any(x => x.Name.Equals(featureId));

                var options = new MemoryCacheEntryOptions()
                    .AddExpirationToken(_signal.GetToken(FeatureManager.FeatureManagerCacheKey));

                _memoryCache.Set(cacheKey, enabled, options);
            }


            return enabled ? 1 : 0;
        }
    }
}
