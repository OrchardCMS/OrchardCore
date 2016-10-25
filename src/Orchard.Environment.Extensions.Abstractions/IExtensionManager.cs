using Orchard.Environment.Extensions.Features;
using Orchard.Environment.Extensions.Loaders;
using System.Collections.Generic;

namespace Orchard.Environment.Extensions
{
    public interface IExtensionManager
    {
        IExtensionInfo GetExtension(string extensionId);
        IExtensionInfoList GetExtensions();
        ExtensionEntry LoadExtension(IExtensionInfo extensionInfo);
        IEnumerable<ExtensionEntry> LoadExtensions(IEnumerable<IExtensionInfo> extensionInfos);

        FeatureEntry LoadFeature(IFeatureInfo feature);
        IEnumerable<FeatureEntry> LoadFeatures(IEnumerable<IFeatureInfo> features);
    }
}
