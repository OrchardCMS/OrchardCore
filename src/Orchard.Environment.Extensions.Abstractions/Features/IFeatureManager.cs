using System.Collections.Generic;

namespace Orchard.Environment.Extensions.Features
{
    public interface IFeatureManager
    {
        IList<IFeatureInfo> GetFeatures(
            IExtensionInfo extensionInfo,
            IManifestInfo manifestInfo);


    }
}
