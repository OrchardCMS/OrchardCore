using System.Collections.Generic;

namespace Orchard.Environment.Extensions.Features
{
    public interface IFeatureManager
    {
        IEnumerable<IFeatureInfo> GetFeatures(
            IExtensionInfo extensionInfo,
            IManifestInfo manifestInfo);
    }
}
