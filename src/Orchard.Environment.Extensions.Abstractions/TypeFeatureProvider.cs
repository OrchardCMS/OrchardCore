using Orchard.Environment.Extensions.Models;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Orchard.Environment.Extensions
{
    /// <summary>
    /// An implementation of this service is able to provide the <see cref="Feature"/> that
    /// any services was harvested from.
    /// </summary>
    public interface ITypeFeatureProvider
    {
        Feature GetFeatureForDependency(Type dependency);
        void TryAdd(Type type, Feature feature);
    }

    public class TypeFeatureProvider : ITypeFeatureProvider
    {
        private readonly IDictionary<Type, Feature> _features = new ConcurrentDictionary<Type, Feature>();

        public Feature GetFeatureForDependency(Type dependency)
        {
            if (_features.ContainsKey(dependency))
            {
                return _features[dependency];
            }
            return null;
        }

        public void TryAdd(Type type, Feature feature)
        {
            if (!_features.ContainsKey(type))
            {
                _features.Add(type, feature);
            }
        }
    }
}