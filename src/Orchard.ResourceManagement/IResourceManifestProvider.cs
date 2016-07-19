namespace Orchard.ResourceManagement
{
    public interface IResourceManifestProvider
    {
        void BuildManifests(ResourceManifestBuilder builder);
    }
}
