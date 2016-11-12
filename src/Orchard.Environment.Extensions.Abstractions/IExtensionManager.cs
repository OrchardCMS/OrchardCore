using Orchard.Environment.Extensions.Features;
using Orchard.Environment.Extensions.Loaders;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orchard.Environment.Extensions
{
    public interface IExtensionManager
    {
        IExtensionInfo GetExtension(string extensionId);
        IExtensionInfoList GetExtensions();
        Task<ExtensionEntry> LoadExtensionAsync(IExtensionInfo extensionInfo);
        Task<IEnumerable<ExtensionEntry>> LoadExtensionsAsync(IEnumerable<IExtensionInfo> extensionInfos);

        Task<FeatureEntry> LoadFeatureAsync(IFeatureInfo feature);
        Task<IEnumerable<FeatureEntry>> LoadFeaturesAsync(IEnumerable<IFeatureInfo> features);
    }
}
