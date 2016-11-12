using Orchard.Environment.Extensions;
using System;
using Orchard.Environment.Extensions.Loaders;
using Orchard.Environment.Extensions.Features;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orchard.Tests.Stubs
{
    public class StubExtensionManager : IExtensionManager
    {
        public IExtensionInfo GetExtension(string extensionId)
        {
            throw new NotImplementedException();
        }

        public IExtensionInfoList GetExtensions()
        {
            throw new NotImplementedException();
        }

        public Task<ExtensionEntry> LoadExtensionAsync(IExtensionInfo extensionInfo)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ExtensionEntry>> LoadExtensionsAsync(IEnumerable<IExtensionInfo> extensionInfos)
        {
            throw new NotImplementedException();
        }

        public Task<FeatureEntry> LoadFeatureAsync(IFeatureInfo feature)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<FeatureEntry>> LoadFeaturesAsync(IEnumerable<IFeatureInfo> features)
        {
            throw new NotImplementedException();
        }
    }
}