using Microsoft.Extensions.Caching.Memory;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Environment.Extensions.Features;

public class FeatureHash : IFeatureHash
{
    private const string FeatureHashCacheKey = "FeatureHash:Features";

    private readonly IShellFeaturesManager _featureManager;
    private readonly IMemoryCache _memoryCache;

    public FeatureHash(IShellFeaturesManager featureManager, IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
        _featureManager = featureManager;
    }

    public async Task<int> GetFeatureHashAsync()
    {
        if (_memoryCache.TryGetValue<int>(FeatureHashCacheKey, out var serial))
        {
            return serial;
        }

        // Calculate a hash of all enabled features' id
        var enabledFeatures = await _featureManager.GetEnabledFeaturesAsync();

        var hashCode = new HashCode();
        foreach (var feature in enabledFeatures)
        {
            hashCode.Add(feature.Id);
        }

        serial = hashCode.ToHashCode();

        _memoryCache.Set(FeatureHashCacheKey, serial);

        return serial;
    }

    public async Task<int> GetFeatureHashAsync(string featureId)
    {
        var cacheKey = string.Format("{0}:{1}", FeatureHashCacheKey, featureId);

        if (!_memoryCache.TryGetValue<bool>(cacheKey, out var enabled))
        {
            var enabledFeatures = await _featureManager.GetEnabledFeaturesAsync();

            enabled = enabledFeatures.Any(x => x.Id.Equals(featureId, StringComparison.Ordinal));

            _memoryCache.Set(cacheKey, enabled);
        }

        return enabled ? 1 : 0;
    }
}
