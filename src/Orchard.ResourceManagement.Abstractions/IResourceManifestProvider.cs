namespace Orchard.ResourceManagement
{
    public interface IResourceManifestProvider
    {
        void BuildManifests(IResourceManifestBuilder builder);
    }
}
