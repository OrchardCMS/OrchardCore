using Orchard.Environment.Extensions.Features;
using System;
using Orchard.Environment.Extensions;

namespace Orchard.Tests.Stubs
{
    public class StubFeatureManager : IFeatureManager
    {
        public IFeatureInfoList GetFeatures(IExtensionInfo extensionInfo, IManifestInfo manifestInfo)
        {
            throw new NotImplementedException();
        }
    }
}