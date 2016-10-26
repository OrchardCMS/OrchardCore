using Orchard.Environment.Extensions;
using System;
using Orchard.Environment.Extensions.Loaders;
using Orchard.Environment.Extensions.Features;
using System.Collections.Generic;

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

        public ExtensionEntry LoadExtension(IExtensionInfo extensionInfo)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ExtensionEntry> LoadExtensions(IEnumerable<IExtensionInfo> extensionInfos)
        {
            throw new NotImplementedException();
        }

        public FeatureEntry LoadFeature(IFeatureInfo feature)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<FeatureEntry> LoadFeatures(IEnumerable<IFeatureInfo> features)
        {
            throw new NotImplementedException();
        }
    }
}