using System.Collections.Generic;

namespace OrchardCore.ResourceManagement
{
    public class ResourceManifestBuilder : IResourceManifestBuilder
    {
        public ResourceManifestBuilder()
        {
            ResourceManifests = new HashSet<ResourceManifest>();
        }

        internal HashSet<ResourceManifest> ResourceManifests { get; private set; }

        public ResourceManifest Add()
        {
            return Add(new ResourceManifest());
        }

        public ResourceManifest Add(ResourceManifest manifest)
        {
            ResourceManifests.Add(manifest);
            return manifest;
        }
    }
}
