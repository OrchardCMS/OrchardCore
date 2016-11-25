using Orchard.Environment.Extensions.Features;
using Orchard.Environment.Extensions.Loaders;
using System.Threading.Tasks;

namespace Orchard.Environment.Extensions
{
    public interface IExtensionManager
    {
        IExtensionInfo GetExtension(string extensionId);
        IExtensionInfoList GetExtensions();
        Task<ExtensionEntry> LoadExtensionAsync(IExtensionInfo extensionInfo);

        IFeatureInfoList GetDependentFeatures(string featureId);
        Task<FeatureEntry> LoadFeatureAsync(IFeatureInfo feature);
    }
}
