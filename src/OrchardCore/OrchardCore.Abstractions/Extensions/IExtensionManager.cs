using OrchardCore.Environment.Extensions.Features;

namespace OrchardCore.Environment.Extensions;

public interface IExtensionManager
{
    IExtensionInfo GetExtension(string extensionId);
    IEnumerable<IExtensionInfo> GetExtensions();
    Task<ExtensionEntry> LoadExtensionAsync(IExtensionInfo extensionInfo);

    IEnumerable<IFeatureInfo> GetFeatures();
    [Obsolete("Use GetFeatures(IEnumerable<string> featureIdsToLoad) instead.")]
    IEnumerable<IFeatureInfo> GetFeatures(string[] featureIdsToLoad);
#pragma warning disable CS0618 // Type or member is obsolete
    IEnumerable<IFeatureInfo> GetFeatures(IEnumerable<string> featureIdsToLoad)
        => GetFeatures(featureIdsToLoad.ToArray());
#pragma warning restore CS0618 // Type or member is obsolete
    IEnumerable<IFeatureInfo> GetFeatureDependencies(string featureId);
    IEnumerable<IFeatureInfo> GetDependentFeatures(string featureId);
    Task<IEnumerable<IFeatureInfo>> LoadFeaturesAsync();
    [Obsolete("Use LoadFeaturesAsync(IEnumerable<string> featureIdsToLoad) instead.")]
    Task<IEnumerable<IFeatureInfo>> LoadFeaturesAsync(string[] featureIdsToLoad);
#pragma warning disable CS0618 // Type or member is obsolete
    Task<IEnumerable<IFeatureInfo>> LoadFeaturesAsync(IEnumerable<string> featureIdsToLoad)
        => LoadFeaturesAsync(featureIdsToLoad.ToArray());
#pragma warning restore CS0618 // Type or member is obsolete
}
