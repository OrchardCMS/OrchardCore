namespace Orchard.ResourceManagement
{
    public interface IResourceManifestBuilder
    {
        ResourceManifest Add();

        ResourceManifest Add(ResourceManifest manifest);
    }
}
