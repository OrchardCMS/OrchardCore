using System;
using System.Collections.Generic;
using System.Linq;
using OrchardCore.Modules;

namespace OrchardCore.Environment.Extensions.Features
{
    public class CompositeFeaturesProvider : IFeaturesProvider
    {
        private readonly IFeaturesProvider[] _featuresProviders;
        public CompositeFeaturesProvider(params IFeaturesProvider[] featuresProviders)
        {
            _featuresProviders = featuresProviders ?? new IFeaturesProvider[0];
        }
        
        public CompositeFeaturesProvider(IEnumerable<IFeaturesProvider> featuresProviders)
        {
            if (featuresProviders == null)
            {
                throw new ArgumentNullException(nameof(featuresProviders));
            }
            _featuresProviders = featuresProviders.ToArray();
        }

        public IEnumerable<IFeatureInfo> GetFeatures(
            IExtensionInfo extensionInfo,
            IManifestInfo manifestInfo)
        {
            List<IFeatureInfo> featureInfos = 
                new List<IFeatureInfo>();

            foreach (var provider in _featuresProviders)
            {
                featureInfos.AddRange(provider.GetFeatures(extensionInfo, manifestInfo));
            }

            return featureInfos;
        }
    }
}
