using System.Collections.Concurrent;
using OrchardCore.Environment.Extensions.Features;

namespace OrchardCore.Environment.Extensions;

public class TypeFeatureProvider : ITypeFeatureProvider
{
    private readonly ConcurrentDictionary<Type, IEnumerable<IFeatureInfo>> _features = new();

    public IExtensionInfo GetExtensionForDependency(Type dependency)
    {
        if (_features.TryGetValue(dependency, out var features))
        {
            return features.First().Extension;
        }

        throw new InvalidOperationException($"Could not resolve extension for type {dependency.Name}.");
    }

    public IFeatureInfo GetFeatureForDependency(Type dependency)
    {
        if (_features.TryGetValue(dependency, out var features))
        {
            // Gets the first feature that has the same ID as the extension, if any. 
            // Otherwise returns the first feature.
            return features.FirstOrDefault(feature => feature.Extension.Id == feature.Id) ?? features.First();
        }

        throw new InvalidOperationException($"Could not resolve main feature for type {dependency.Name}.");
    }

    public IEnumerable<IFeatureInfo> GetFeaturesForDependency(Type dependency)
    {
        if (_features.TryGetValue(dependency, out var features))
        {
            return features;
        }

        throw new InvalidOperationException($"Could not resolve features for type {dependency.Name}.");
    }

    public IEnumerable<Type> GetTypesForFeature(IFeatureInfo feature)
    {
        return _features.Where(kv => kv.Value.Contains(feature)).Select(kv => kv.Key);
    }

    public void TryAdd(Type type, IFeatureInfo feature)
    {
        var features = _features.AddOrUpdate(type, (key, value) => [value], (key, features, value) => features.Contains(value) ? features : features.Append(value).ToArray(), feature);

        if (features.Count() > 1 && (FeatureTypeDiscoveryAttribute.GetFeatureTypeDiscoveryForType(type)?.SingleFeatureOnly ?? false))
        {
            throw new InvalidOperationException($"The type {type} can only be assigned to a single feature. Make sure the type is not added to DI by multiple startup classes.");
        }
    }
}
