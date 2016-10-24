namespace Orchard.Environment.Extensions.Manifests
{
    public interface IManifestBuilder
    {
        IManifestInfo GetManifest(string subPath);
    }
}
