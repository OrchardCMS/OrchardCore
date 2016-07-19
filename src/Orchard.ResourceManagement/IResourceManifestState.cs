using System.Collections.Generic;

namespace Orchard.ResourceManagement
{
    public interface IResourceManifestState
    {
        IEnumerable<IResourceManifest> Manifest { get; set; }
    }

    public class ResourceManifestState : IResourceManifestState
    {
        public IEnumerable<IResourceManifest> Manifest { get; set; }

    }

}
