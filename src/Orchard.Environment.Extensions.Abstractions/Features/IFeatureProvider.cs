using System.Collections.Generic;

namespace Orchard.Environment.Extensions.Features
{
    public interface IFeatureProvider
    {
        IEnumerable<IFeatureInfo> GetFeatures(
            IExtensionInfo extensionInfo,
            IManifestInfo manifestInfo);
    }
}
