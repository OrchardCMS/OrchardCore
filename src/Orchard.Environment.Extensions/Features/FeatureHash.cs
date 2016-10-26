using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Orchard.Environment.Cache;
using Orchard.Environment.Shell;

namespace Orchard.Environment.Extensions.Features
{
    public class FeatureHash : IFeatureHash
    {
        private const string FeatureHashCacheKey = "FeatureHash:Features";

        private readonly IShellFeaturesManager _featureManager;
        private readonly IMemoryCache _memoryCache;
        private readonly ISignal _signal;

        public FeatureHash(
            IShellFeaturesManager featureManager,
            IMemoryCache memoryCache,
            ISignal signal)
        {
            _memoryCache = memoryCache;
            _featureManager = featureManager;
            _signal = signal;
        }

        public int GetFeatureHash()
        {
            int serial;
            if (_memoryCache.TryGetValue(FeatureHashCacheKey, out serial))
            {
                return serial;
            }

            // Calculate a hash of all enabled features' name
            serial = _featureManager.EnabledFeatures()
                .OrderBy(x => x.Name)
                .Aggregate(0, (a, f) => a * 7 + f.Name.GetHashCode());

            var options = new MemoryCacheEntryOptions()
                .AddExpirationToken(_signal.GetToken(FeatureManager.FeatureManagerCacheKey));

            _memoryCache.Set(FeatureHashCacheKey, serial, options);

            return serial;
        }

        public int GetFeatureHash(string featureId)
        {
            var cacheKey = FeatureHashCacheKey + ":" + featureId;
            bool enabled;
            if (!_memoryCache.TryGetValue(cacheKey, out enabled))
            {
                enabled = 
                    _featureManager.EnabledFeatures()
                    .Any(x => x.Name.Equals(featureId));

                var options = new MemoryCacheEntryOptions()
                    .AddExpirationToken(_signal.GetToken(FeatureManager.FeatureManagerCacheKey));

                _memoryCache.Set(cacheKey, enabled, options);
            }


            return enabled ? 1 : 0;
        }
    }
}
