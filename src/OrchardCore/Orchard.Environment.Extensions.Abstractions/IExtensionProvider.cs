namespace Orchard.Environment.Extensions
{
    public interface IExtensionProvider
    {
        int Order { get; }
        IExtensionInfo GetExtensionInfo(IManifestInfo manifestInfo, string subPath);
    }
}
