namespace Orchard.Environment.Extensions.Features
{
    public interface IFeatureManager
    {
        IFeatureInfoList GetFeatures(
            IExtensionInfo extensionInfo,
            IManifestInfo manifestInfo);
    }
}
