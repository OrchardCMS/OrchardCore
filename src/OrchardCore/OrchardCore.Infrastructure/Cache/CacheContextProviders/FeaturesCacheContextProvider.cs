using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Extensions.Features;

namespace OrchardCore.Environment.Cache.CacheContextProviders;

public class FeaturesCacheContextProvider : ICacheContextProvider
{
    private const string FeaturesPrefix = "features:";

    private readonly IServiceProvider _serviceProvider;
    private IFeatureHash _featureHash;

    public FeaturesCacheContextProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    // Resolved on demand as most cache contexts don't vary by features, while the feature hash
    // service pulls the whole shell features management graph with it on each request.
    private IFeatureHash FeatureHash => _featureHash ??= _serviceProvider.GetRequiredService<IFeatureHash>();

    public async Task PopulateContextEntriesAsync(IEnumerable<string> contexts, List<CacheContextEntry> entries)
    {
        if (contexts.ContainsContext("features"))
        {
            // Add a hash of the enabled features.
            var hash = await FeatureHash.GetFeatureHashAsync();
            entries.Add(new CacheContextEntry("features", hash.ToString(CultureInfo.InvariantCulture)));
        }
        else
        {
            foreach (var context in contexts.Where(ctx => ctx.StartsWith(FeaturesPrefix, StringComparison.OrdinalIgnoreCase)))
            {
                var featureName = context[FeaturesPrefix.Length..];
                var hash = await FeatureHash.GetFeatureHashAsync(featureName);

                entries.Add(new CacheContextEntry("features", hash.ToString(CultureInfo.InvariantCulture)));
            }
        }
    }
}
