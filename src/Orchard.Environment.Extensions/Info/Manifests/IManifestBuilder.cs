namespace Orchard.Environment.Extensions.Info.Manifests
{
    public interface IManifestBuilder
    {
        IManifestInfo GetManifest(string subPath);
    }
}
