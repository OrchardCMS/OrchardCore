namespace Orchard.Environment.Extensions.Info
{
    public interface IManifestProvider
    {
        IManifestInfo GetManifest(string subPath);
    }
}
