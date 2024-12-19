using OrchardCore.Environment.Extensions.Features;

namespace OrchardCore.Environment.Extensions;

public interface IExtensionManager
{
    IExtensionInfo GetExtension(string extensionId);
    IEnumerable<IExtensionInfo> GetExtensions();
    Task<ExtensionEntry> LoadExtensionAsync(IExtensionInfo extensionInfo);

    IEnumerable<IFeatureInfo> GetFeatures();
    IEnumerable<IFeatureInfo> GetFeatures(string[] featureIdsToLoad);
    IEnumerable<IFeatureInfo> GetFeatureDependencies(string featureId);
    IEnumerable<IFeatureInfo> GetDependentFeatures(string featureId);
    Task<IEnumerable<IFeatureInfo>> LoadFeaturesAsync();
    Task<IEnumerable<IFeatureInfo>> LoadFeaturesAsync(string[] featureIdsToLoad);
}
