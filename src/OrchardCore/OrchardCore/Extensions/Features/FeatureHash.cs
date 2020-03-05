using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.Environment.Cache;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Environment.Extensions.Features
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

        public async Task<int> GetFeatureHashAsync()
        {
            int serial;
            if (_memoryCache.TryGetValue(FeatureHashCacheKey, out serial))
            {
                return serial;
            }

            // Calculate a hash of all enabled features' id
            var enabledFeatures = await _featureManager.GetEnabledFeaturesAsync();
            serial = enabledFeatures
                .OrderBy(x => x.Id)
                .Aggregate(0, (a, f) => a * 7 + f.Id.GetHashCode());

            var options = new MemoryCacheEntryOptions()
                .AddExpirationToken(_signal.GetToken(FeaturesProvider.FeatureProviderCacheKey));

            _memoryCache.Set(FeatureHashCacheKey, serial, options);

            return serial;
        }

        public async Task<int> GetFeatureHashAsync(string featureId)
        {
            var cacheKey = string.Format("{0}:{1}", FeatureHashCacheKey, featureId);
            bool enabled;
            if (!_memoryCache.TryGetValue(cacheKey, out enabled))
            {
                var enabledFeatures = await _featureManager.GetEnabledFeaturesAsync();
                enabled =
                    enabledFeatures
                        .Any(x => x.Id.Equals(featureId));

                var options = new MemoryCacheEntryOptions()
                    .AddExpirationToken(_signal.GetToken(FeaturesProvider.FeatureProviderCacheKey));

                _memoryCache.Set(cacheKey, enabled, options);
            }

            return enabled ? 1 : 0;
        }
    }
}
