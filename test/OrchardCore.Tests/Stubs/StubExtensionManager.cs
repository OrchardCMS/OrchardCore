using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;

namespace OrchardCore.Tests.Stubs;

public class StubExtensionManager : IExtensionManager
{
    public IEnumerable<IFeatureInfo> GetDependentFeatures(string featureId)
    {
        throw new NotImplementedException();
    }

    public IExtensionInfo GetExtension(string extensionId)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IExtensionInfo> GetExtensions()
    {
        return [];
    }

    public IEnumerable<IFeatureInfo> GetFeatureDependencies(string featureId)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IFeatureInfo> GetFeatures()
    {
        return [];
    }

    public IEnumerable<IFeatureInfo> GetFeatures(string[] featureIdsToLoad)
    {
        throw new NotImplementedException();
    }

    public Task<ExtensionEntry> LoadExtensionAsync(IExtensionInfo extensionInfo)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<IFeatureInfo>> LoadFeaturesAsync()
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<IFeatureInfo>> LoadFeaturesAsync(string[] featureIdsToLoad)
    {
        throw new NotImplementedException();
    }
}
