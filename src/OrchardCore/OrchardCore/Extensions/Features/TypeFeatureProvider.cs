using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using OrchardCore.Environment.Extensions.Features;

namespace OrchardCore.Environment.Extensions
{
    public class TypeFeatureProvider : ITypeFeatureProvider
    {
        private readonly ConcurrentDictionary<Type, IEnumerable<IFeatureInfo>> _features = new();

        public IEnumerable<IFeatureInfo> GetFeaturesForDependency(Type dependency)
        {
            if (_features.TryGetValue(dependency, out var features))
            {
                return features;
            }

            throw new InvalidOperationException($"Could not resolve features for type {dependency.Name}");
        }

        public IEnumerable<Type> GetTypesForFeature(IFeatureInfo feature)
        {
            return _features.Where(kv => kv.Value.Contains(feature)).Select(kv => kv.Key);
        }

        public void TryAdd(Type type, IFeatureInfo feature)
        {
            _features.AddOrUpdate(type, (key, value) => [value], (key, features, value) => features.Contains(value) ? features : features.Append(value).ToArray(), feature);
        }
    }
}
