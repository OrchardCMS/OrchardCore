using System;
using System.Collections.Concurrent;
using Orchard.Environment.Extensions.Features;

namespace Orchard.Environment.Extensions
{
    public class TypeFeatureProvider : ITypeFeatureProvider
    {
        private readonly ConcurrentDictionary<Type, IFeatureInfo> _features 
            = new ConcurrentDictionary<Type, IFeatureInfo>();

        private static readonly IFeatureInfo CoreFeature 		
             = new InternalFeatureInfo("Core", new InternalExtensionInfo("Core"));

        public IFeatureInfo GetFeatureForDependency(Type dependency)
        {
            IFeatureInfo feature = null;

            if(_features.TryGetValue(dependency, out feature))
            {
                return feature;
            }

            return CoreFeature;
            //throw new InvalidOperationException($"Could not resolve feature for type {dependency.Name}");
        }

        public void TryAdd(Type type, IFeatureInfo feature)
        {
            _features.TryAdd(type, feature);
        }
    }
}