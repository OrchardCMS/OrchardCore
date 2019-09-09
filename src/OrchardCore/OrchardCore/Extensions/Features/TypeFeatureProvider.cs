using System;
using System.Collections.Concurrent;
using OrchardCore.Environment.Extensions.Features;

namespace OrchardCore.Environment.Extensions
{
    public class TypeFeatureProvider : ITypeFeatureProvider
    {
        private readonly ConcurrentDictionary<Type, IFeatureInfo> _features
            = new ConcurrentDictionary<Type, IFeatureInfo>();

        /// <summary>
        /// Retrieves the feature that is tied to a given type.
        /// </summary>
        public IFeatureInfo GetFeatureForDependency(Type dependency)
        {
            IFeatureInfo feature = null;

            if (_features.TryGetValue(dependency, out feature))
            {
                return feature;
            }

            throw new InvalidOperationException($"Could not resolve feature for type {dependency.Name}");
        }

        /// <summary>
        /// Ties a given type to a given feature. Note: The last registration wins.
        /// </summary>
        public void Add(Type type, IFeatureInfo feature) => _features[type] = feature;
    }
}
