using Orchard.Environment.Extensions.Features;
using Orchard.Environment.Extensions.Loaders;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Orchard.Environment.Extensions
{
    public interface IExtensionManager
    {
        IExtensionInfo GetExtension(string extensionId);
        IExtensionInfoList GetExtensions();
        Task<ExtensionEntry> LoadExtensionAsync(IExtensionInfo extensionInfo);

        IFeatureInfoList GetFeatureDependencies(string featureId);
        IFeatureInfoList GetDependentFeatures(string featureId, IFeatureInfo[] featuresToSearch);
        Task<IEnumerable<FeatureEntry>> LoadFeaturesAsync();
        Task<IEnumerable<FeatureEntry>> LoadFeaturesAsync(IFeatureInfo[] featuresToLoad);
    }
}
