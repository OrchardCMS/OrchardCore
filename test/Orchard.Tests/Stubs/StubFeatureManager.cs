using Orchard.Environment.Extensions.Features;
using System;
using Orchard.Environment.Extensions;
using System.Collections.Generic;

namespace Orchard.Tests.Stubs
{
    public class StubFeatureManager : IFeatureManager
    {
        public IEnumerable<IFeatureInfo> GetFeatures(IExtensionInfo extensionInfo, IManifestInfo manifestInfo)
        {
            throw new NotImplementedException();
        }
    }
}