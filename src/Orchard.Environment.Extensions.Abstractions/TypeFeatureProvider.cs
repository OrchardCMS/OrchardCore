using Orchard.Environment.Extensions.Models;
using System;
using System.Collections.Generic;

namespace Orchard.Environment.Extensions
{
    public interface ITypeFeatureProvider
    {
        Feature GetFeatureForDependency(Type dependency);
    }

    public class TypeFeatureProvider : ITypeFeatureProvider
    {
        private readonly IDictionary<Type, Feature> _features;
        public TypeFeatureProvider(IDictionary<Type, Feature> features)
        {
            _features = features;
        }

        public Feature GetFeatureForDependency(Type dependency)
        {
            if (_features.ContainsKey(dependency))
            {
                return _features[dependency];
            }
            return null;
        }
    }
}