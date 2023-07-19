using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Environment.Extensions.Features;

namespace OrchardCore.Environment.Cache.CacheContextProviders
{
    public class FeaturesCacheContextProvider : ICacheContextProvider
    {
        private const string FeaturesPrefix = "features:";

        private readonly IFeatureHash _featureHash;

        public FeaturesCacheContextProvider(IFeatureHash featureHash)
        {
            _featureHash = featureHash;
        }

        public async Task PopulateContextEntriesAsync(IEnumerable<string> contexts, List<CacheContextEntry> entries)
        {
            if (contexts.Any(ctx => String.Equals(ctx, "features", StringComparison.OrdinalIgnoreCase)))
            {
                // Add a hash of the enabled features.
                var hash = await _featureHash.GetFeatureHashAsync();
                entries.Add(new CacheContextEntry("features", hash.ToString(CultureInfo.InvariantCulture)));
            }
            else
            {
                foreach (var context in contexts.Where(ctx => ctx.StartsWith(FeaturesPrefix, StringComparison.OrdinalIgnoreCase)))
                {
                    var featureName = context[FeaturesPrefix.Length..];
                    var hash = await _featureHash.GetFeatureHashAsync(featureName);

                    entries.Add(new CacheContextEntry("features", hash.ToString(CultureInfo.InvariantCulture)));
                }
            }
        }
    }
}
