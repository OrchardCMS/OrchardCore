using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using OrchardCore.Environment.Extensions.Features;

namespace OrchardCore.Environment.Extensions
{
    public class TypeFeatureProvider : ITypeFeatureProvider
    {
        private readonly ConcurrentDictionary<Type, IFeatureInfo> _features = new();

        public IFeatureInfo GetFeatureForDependency(Type dependency)
        {
            if (_features.TryGetValue(dependency, out var feature))
            {
                return feature;
            }

            throw new InvalidOperationException($"Could not resolve feature for type {dependency.Name}");
        }

        public IEnumerable<Type> GetTypesForFeature(IFeatureInfo feature)
        {
            return _features.Where(kv => kv.Value == feature).Select(kv => kv.Key);
        }

        public void TryAdd(Type type, IFeatureInfo feature)
        {
            _features.TryAdd(type, feature);
        }
    }
}
