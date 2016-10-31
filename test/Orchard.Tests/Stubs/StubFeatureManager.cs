using Orchard.Environment.Extensions.Features;
using System;
using System.Collections.Generic;
using Orchard.Environment.Extensions;

namespace Orchard.Tests.Stubs
{
    public class StubFeatureManager : IFeatureManager
    {
        public IList<IFeatureInfo> GetFeatures(IExtensionInfo extensionInfo, IManifestInfo manifestInfo)
        {
            throw new NotImplementedException();
        }
    }
}