using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Configuration;

namespace Orchard.Environment.Extensions.Info.Manifests
{
    public class NotFoundManifestFile : IManifestInfo
    {
        public NotFoundManifestFile(
            IFileInfo manifest
            )
        {
            Manifest = manifest;
        }

        public IFileInfo Manifest { get; private set; }
        public bool Exists => Manifest.Exists;
        public string Name { get; } = null;
        public string Description { get; } = null;
        public IConfigurationRoot ConfigurationRoot { get; }
    }
}
