namespace Orchard.Environment.Extensions
{
    public interface IExtensionProvider
    {
        double Order { get; }
        IExtensionInfo GetExtensionInfo(IManifestInfo manifestInfo, string subPath);
    }
}
