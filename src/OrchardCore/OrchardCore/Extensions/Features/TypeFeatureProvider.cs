using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using OrchardCore.Environment.Extensions.Features;

namespace OrchardCore.Environment.Extensions
{
    public class TypeFeatureProvider : ITypeFeatureProvider
    {
        private readonly ConcurrentDictionary<Type, ConcurrentBag<IFeatureInfo>> _features = new();

        public IEnumerable<IFeatureInfo> GetFeaturesForDependency(Type dependency)
        {
            if (_features.TryGetValue(dependency, out var feature))
            {
                return feature;
            }

            throw new InvalidOperationException($"Could not resolve feature for type {dependency.Name}");
        }

        public void TryAdd(Type type, IFeatureInfo feature)
        {
            _features.AddOrUpdate(type, (t) => [feature], (key, existing) =>
            {
                existing.Add(feature);

                return existing;
            });
        }
    }
}
